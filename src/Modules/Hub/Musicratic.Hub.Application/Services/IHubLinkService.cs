namespace Musicratic.Hub.Application.Services;

public interface IHubLinkService
{
    string GenerateJoinUrl(string hubCode);

    string GenerateSignature(string hubCode);

    bool ValidateSignature(string hubCode, string signature);
}
