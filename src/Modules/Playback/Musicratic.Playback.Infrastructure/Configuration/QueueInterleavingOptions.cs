namespace Musicratic.Playback.Infrastructure.Configuration;

public sealed class QueueInterleavingOptions
{
    public const string SectionName = "Playback:QueueInterleaving";

    /// <summary>
    /// Every N list tracks, insert 1 proposal. Default: 3.
    /// </summary>
    public int ListToProposalRatio { get; set; } = 3;
}
