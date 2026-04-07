namespace STS2RitsuLib.Scaffolding.Characters.Visuals.Definition
{
    /// <summary>
    ///     Fluent builder for <see cref="CharacterVisualFrameSequence" /> with per-frame durations.
    /// </summary>
    public sealed class CharacterVisualFrameSequenceBuilder
    {
        private readonly List<CharacterVisualFrame> _frames = [];
        private bool _loop;

        private CharacterVisualFrameSequenceBuilder()
        {
        }

        /// <summary>
        ///     Starts a new frame sequence definition.
        /// </summary>
        public static CharacterVisualFrameSequenceBuilder Create()
        {
            return new();
        }

        /// <summary>
        ///     Appends a frame after any existing ones.
        /// </summary>
        public CharacterVisualFrameSequenceBuilder Frame(string texturePath, float durationSeconds)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(texturePath);
            if (durationSeconds <= 0f)
                throw new ArgumentOutOfRangeException(nameof(durationSeconds), durationSeconds,
                    "Frame duration must be positive.");

            _frames.Add(new(texturePath, durationSeconds));
            return this;
        }

        /// <summary>
        ///     Sets whether the sequence loops after the last frame.
        /// </summary>
        public CharacterVisualFrameSequenceBuilder Loop(bool loop = true)
        {
            _loop = loop;
            return this;
        }

        /// <summary>
        ///     Materializes the sequence; at least one <see cref="Frame" /> is required.
        /// </summary>
        public CharacterVisualFrameSequence Build()
        {
            return _frames.Count == 0
                ? throw new InvalidOperationException("Add at least one frame before Build().")
                : new([.. _frames], _loop);
        }
    }
}
