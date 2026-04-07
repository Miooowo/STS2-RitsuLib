namespace STS2RitsuLib.Scaffolding.Characters.Visuals.Definition
{
    /// <summary>
    ///     Fluent builder for <see cref="CharacterCombatVisualCueSet" /> (single textures and frame sequences per cue).
    /// </summary>
    public sealed class CharacterCombatVisualCueSetBuilder
    {
        private readonly Dictionary<string, CharacterVisualFrameSequence> _sequences =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, string> _textures =
            new(StringComparer.OrdinalIgnoreCase);

        private CharacterCombatVisualCueSetBuilder()
        {
        }

        /// <summary>
        ///     Starts a new cue set definition.
        /// </summary>
        public static CharacterCombatVisualCueSetBuilder Create()
        {
            return new();
        }

        /// <summary>
        ///     Binds one static texture to a cue (e.g. <c>idle</c>, <c>die</c>). Removes a frame sequence for the same
        ///     cue key if present.
        /// </summary>
        public CharacterCombatVisualCueSetBuilder Single(string cueKey, string texturePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(cueKey);
            ArgumentException.ThrowIfNullOrWhiteSpace(texturePath);

            _textures[cueKey] = texturePath;
            _sequences.Remove(cueKey);
            return this;
        }

        /// <summary>
        ///     Binds a built frame sequence to a cue. Removes a single-texture entry for the same cue key if present.
        /// </summary>
        public CharacterCombatVisualCueSetBuilder Sequence(string cueKey, CharacterVisualFrameSequence sequence)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(cueKey);
            ArgumentNullException.ThrowIfNull(sequence);

            _sequences[cueKey] = sequence;
            _textures.Remove(cueKey);
            return this;
        }

        /// <summary>
        ///     Binds a frame sequence configured via <paramref name="configure" />.
        /// </summary>
        public CharacterCombatVisualCueSetBuilder Sequence(string cueKey,
            Action<CharacterVisualFrameSequenceBuilder> configure)
        {
            ArgumentNullException.ThrowIfNull(configure);

            var inner = CharacterVisualFrameSequenceBuilder.Create();
            configure(inner);
            return Sequence(cueKey, inner.Build());
        }

        /// <summary>
        ///     Produces an immutable cue set (empty dictionaries become <see langword="null" /> fields).
        /// </summary>
        public CharacterCombatVisualCueSet Build()
        {
            return new(
                _textures.Count > 0 ? _textures : null,
                _sequences.Count > 0 ? _sequences : null);
        }
    }
}
