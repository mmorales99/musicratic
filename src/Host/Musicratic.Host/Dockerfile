# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy central package management and build props first for layer caching
COPY Directory.Build.props Directory.Packages.props ./

# Copy all project files for restore
COPY Shared/Musicratic.Shared.Domain/Musicratic.Shared.Domain.csproj Shared/Musicratic.Shared.Domain/
COPY Shared/Musicratic.Shared.Application/Musicratic.Shared.Application.csproj Shared/Musicratic.Shared.Application/
COPY Shared/Musicratic.Shared.Contracts/Musicratic.Shared.Contracts.csproj Shared/Musicratic.Shared.Contracts/
COPY Shared/Musicratic.Shared.Infrastructure/Musicratic.Shared.Infrastructure.csproj Shared/Musicratic.Shared.Infrastructure/

COPY Modules/Auth/Musicratic.Auth.Domain/Musicratic.Auth.Domain.csproj Modules/Auth/Musicratic.Auth.Domain/
COPY Modules/Auth/Musicratic.Auth.Application/Musicratic.Auth.Application.csproj Modules/Auth/Musicratic.Auth.Application/
COPY Modules/Auth/Musicratic.Auth.Infrastructure/Musicratic.Auth.Infrastructure.csproj Modules/Auth/Musicratic.Auth.Infrastructure/
COPY Modules/Auth/Musicratic.Auth.Api/Musicratic.Auth.Api.csproj Modules/Auth/Musicratic.Auth.Api/

COPY Modules/Hub/Musicratic.Hub.Domain/Musicratic.Hub.Domain.csproj Modules/Hub/Musicratic.Hub.Domain/
COPY Modules/Hub/Musicratic.Hub.Application/Musicratic.Hub.Application.csproj Modules/Hub/Musicratic.Hub.Application/
COPY Modules/Hub/Musicratic.Hub.Infrastructure/Musicratic.Hub.Infrastructure.csproj Modules/Hub/Musicratic.Hub.Infrastructure/
COPY Modules/Hub/Musicratic.Hub.Api/Musicratic.Hub.Api.csproj Modules/Hub/Musicratic.Hub.Api/

COPY Modules/Playback/Musicratic.Playback.Domain/Musicratic.Playback.Domain.csproj Modules/Playback/Musicratic.Playback.Domain/
COPY Modules/Playback/Musicratic.Playback.Application/Musicratic.Playback.Application.csproj Modules/Playback/Musicratic.Playback.Application/
COPY Modules/Playback/Musicratic.Playback.Infrastructure/Musicratic.Playback.Infrastructure.csproj Modules/Playback/Musicratic.Playback.Infrastructure/
COPY Modules/Playback/Musicratic.Playback.Api/Musicratic.Playback.Api.csproj Modules/Playback/Musicratic.Playback.Api/

COPY Modules/Voting/Musicratic.Voting.Domain/Musicratic.Voting.Domain.csproj Modules/Voting/Musicratic.Voting.Domain/
COPY Modules/Voting/Musicratic.Voting.Application/Musicratic.Voting.Application.csproj Modules/Voting/Musicratic.Voting.Application/
COPY Modules/Voting/Musicratic.Voting.Infrastructure/Musicratic.Voting.Infrastructure.csproj Modules/Voting/Musicratic.Voting.Infrastructure/
COPY Modules/Voting/Musicratic.Voting.Api/Musicratic.Voting.Api.csproj Modules/Voting/Musicratic.Voting.Api/

COPY Modules/Economy/Musicratic.Economy.Domain/Musicratic.Economy.Domain.csproj Modules/Economy/Musicratic.Economy.Domain/
COPY Modules/Economy/Musicratic.Economy.Application/Musicratic.Economy.Application.csproj Modules/Economy/Musicratic.Economy.Application/
COPY Modules/Economy/Musicratic.Economy.Infrastructure/Musicratic.Economy.Infrastructure.csproj Modules/Economy/Musicratic.Economy.Infrastructure/
COPY Modules/Economy/Musicratic.Economy.Api/Musicratic.Economy.Api.csproj Modules/Economy/Musicratic.Economy.Api/

COPY Modules/Analytics/Musicratic.Analytics.Domain/Musicratic.Analytics.Domain.csproj Modules/Analytics/Musicratic.Analytics.Domain/
COPY Modules/Analytics/Musicratic.Analytics.Application/Musicratic.Analytics.Application.csproj Modules/Analytics/Musicratic.Analytics.Application/
COPY Modules/Analytics/Musicratic.Analytics.Infrastructure/Musicratic.Analytics.Infrastructure.csproj Modules/Analytics/Musicratic.Analytics.Infrastructure/
COPY Modules/Analytics/Musicratic.Analytics.Api/Musicratic.Analytics.Api.csproj Modules/Analytics/Musicratic.Analytics.Api/

COPY Modules/Social/Musicratic.Social.Domain/Musicratic.Social.Domain.csproj Modules/Social/Musicratic.Social.Domain/
COPY Modules/Social/Musicratic.Social.Application/Musicratic.Social.Application.csproj Modules/Social/Musicratic.Social.Application/
COPY Modules/Social/Musicratic.Social.Infrastructure/Musicratic.Social.Infrastructure.csproj Modules/Social/Musicratic.Social.Infrastructure/
COPY Modules/Social/Musicratic.Social.Api/Musicratic.Social.Api.csproj Modules/Social/Musicratic.Social.Api/

COPY Modules/Notification/Musicratic.Notification.Domain/Musicratic.Notification.Domain.csproj Modules/Notification/Musicratic.Notification.Domain/
COPY Modules/Notification/Musicratic.Notification.Application/Musicratic.Notification.Application.csproj Modules/Notification/Musicratic.Notification.Application/
COPY Modules/Notification/Musicratic.Notification.Infrastructure/Musicratic.Notification.Infrastructure.csproj Modules/Notification/Musicratic.Notification.Infrastructure/
COPY Modules/Notification/Musicratic.Notification.Api/Musicratic.Notification.Api.csproj Modules/Notification/Musicratic.Notification.Api/

COPY BFF/Musicratic.BFF.Web/Musicratic.BFF.Web.csproj BFF/Musicratic.BFF.Web/
COPY BFF/Musicratic.BFF.Mobile/Musicratic.BFF.Mobile.csproj BFF/Musicratic.BFF.Mobile/

COPY Host/Musicratic.Host/Musicratic.Host.csproj Host/Musicratic.Host/

RUN dotnet restore Host/Musicratic.Host/Musicratic.Host.csproj

# Copy everything and publish
COPY . .
RUN dotnet publish Host/Musicratic.Host/Musicratic.Host.csproj \
    -c Release -o /app/publish --no-restore \
    -p:ErrorOnDuplicatePublishOutputFiles=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app

RUN addgroup -S appgroup && adduser -S appuser -G appgroup
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Musicratic.Host.dll"]
