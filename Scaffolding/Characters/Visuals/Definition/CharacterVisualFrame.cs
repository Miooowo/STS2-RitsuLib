namespace STS2RitsuLib.Scaffolding.Characters.Visuals.Definition
{
    /// <summary>
    ///     One static frame in a data-only sprite sequence (path + hold time).
    /// </summary>
    /// <param name="TexturePath"><c>res://</c> texture path.</param>
    /// <param name="DurationSeconds">Display duration; must be positive when built via
    ///     <see cref="CharacterVisualFrameSequenceBuilder" />.</param>
    public readonly record struct CharacterVisualFrame(string TexturePath, float DurationSeconds);
}
