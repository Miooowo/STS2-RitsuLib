namespace STS2RitsuLib.Scaffolding.Characters.Visuals.Definition
{
    /// <summary>
    ///     Mod-author entry for data-only combat visual definitions (textures and frame sequences without authoring
    ///     <c>tscn</c> animation nodes). Runtime application uses <see cref="Visuals.ModCreatureVisualPlayback" />.
    /// </summary>
    public static class ModCharacterCombatVisuals
    {
        /// <summary>
        ///     Begins a <see cref="CharacterCombatVisualCueSet" /> builder.
        /// </summary>
        public static CharacterCombatVisualCueSetBuilder CueSet() => CharacterCombatVisualCueSetBuilder.Create();

        /// <summary>
        ///     Begins a <see cref="CharacterVisualFrameSequence" /> builder.
        /// </summary>
        public static CharacterVisualFrameSequenceBuilder FrameSequence() =>
            CharacterVisualFrameSequenceBuilder.Create();
    }
}
