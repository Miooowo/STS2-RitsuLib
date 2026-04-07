using System.Collections.Generic;

namespace STS2RitsuLib.Scaffolding.Characters.Visuals.Definition
{
    /// <summary>
    ///     Immutable combat / game-over visual cues: single textures and/or per-cue frame sequences. Built with
    ///     <see cref="CharacterCombatVisualCueSetBuilder" /> or <see cref="ModCharacterCombatVisuals.CueSet" />.
    /// </summary>
    /// <param name="TexturePathByCue">One texture per cue.</param>
    /// <param name="FrameSequenceByCue">Overrides <paramref name="TexturePathByCue" /> for the same cue key.</param>
    public sealed record CharacterCombatVisualCueSet(
        IReadOnlyDictionary<string, string>? TexturePathByCue = null,
        IReadOnlyDictionary<string, CharacterVisualFrameSequence>? FrameSequenceByCue = null);
}
