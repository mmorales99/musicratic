using Musicratic.Hub.Domain.Entities;
using Musicratic.Hub.Domain.Enums;
using HubEntity = Musicratic.Hub.Domain.Entities.Hub;

namespace Musicratic.TestUtilities.Builders;

public sealed class HubBuilder
{
    private string _name = "Test Hub";
    private HubType _type = HubType.Venue;
    private Guid _ownerId = Guid.NewGuid();
    private HubSettings _settings = new();
    private string _code = "hub-" + Guid.NewGuid().ToString("N")[..8];

    public HubBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public HubBuilder WithType(HubType type)
    {
        _type = type;
        return this;
    }

    public HubBuilder WithOwner(Guid ownerId)
    {
        _ownerId = ownerId;
        return this;
    }

    public HubBuilder WithSettings(HubSettings settings)
    {
        _settings = settings;
        return this;
    }

    public HubBuilder WithCode(string code)
    {
        _code = code;
        return this;
    }

    public HubEntity Build() => HubEntity.Create(_name, _type, _ownerId, _settings, _code);
}
