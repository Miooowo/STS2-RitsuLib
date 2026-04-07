using System.Collections.Generic;

namespace STS2RitsuLib.Scaffolding.Characters.Visuals.Definition
{
    /// <summary>
    ///     Immutable ordered frames for one combat / game-over cue (runtime playback under creature visuals).
    /// </summary>
    /// <param name="Frames">At least one entry.</param>
    /// <param name="Loop">Whether to restart after the last frame.</param>
    public sealed record CharacterVisualFrameSequence(
        IReadOnlyList<CharacterVisualFrame> Frames,
        bool Loop = false);
}
