using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Musicratic.Hub.Application;
using Musicratic.Hub.Domain.Repositories;

namespace Musicratic.Hub.Infrastructure.Services;

public sealed class AttachmentExpiryBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<AttachmentExpiryOptions> options,
    ILogger<AttachmentExpiryBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMinutes(options.Value.IntervalMinutes);
        logger.LogInformation(
            "Attachment expiry background service started. Interval: {IntervalMinutes}m",
            options.Value.IntervalMinutes);

        using var timer = new PeriodicTimer(interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ProcessExpiredAttachments(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing expired attachments");
            }
        }

        logger.LogInformation("Attachment expiry background service stopped");
    }

    private async Task ProcessExpiredAttachments(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var attachmentRepository = scope.ServiceProvider
            .GetRequiredService<IHubAttachmentRepository>();
        var unitOfWork = scope.ServiceProvider
            .GetRequiredService<IHubUnitOfWork>();

        var expired = await attachmentRepository.GetExpiredActive(cancellationToken);

        if (expired.Count == 0)
            return;

        foreach (var attachment in expired)
        {
            attachment.Detach();
            attachmentRepository.Update(attachment);
        }

        await unitOfWork.SaveChanges(cancellationToken);

        logger.LogInformation(
            "Detached {Count} expired hub attachments", expired.Count);
    }
}
