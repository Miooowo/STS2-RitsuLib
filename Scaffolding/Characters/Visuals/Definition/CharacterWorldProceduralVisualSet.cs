namespace STS2RitsuLib.Scaffolding.Characters.Visuals.Definition
{
    /// <summary>
    ///     Data-only definitions for merchant / rest-site visuals so mod authors can skip authoring dedicated
    ///     <c>tscn</c> scenes. Built with <see cref="CharacterWorldProceduralVisualSetBuilder" /> or
    ///     <see cref="ModCharacterWorldSceneVisuals" />.
    /// </summary>
    /// <param name="Merchant">Merchant-room shell + cues (e.g. <c>relaxed_loop</c>, <c>die</c>).</param>
    /// <param name="RestSite">Rest-site shell + cues (e.g. <c>overgrowth_loop</c>, <c>hive_loop</c>, <c>glory_loop</c>).</param>
    public sealed record CharacterWorldProceduralVisualSet(
        CharacterMerchantWorldDefinition? Merchant = null,
        CharacterRestSiteWorldDefinition? RestSite = null);

    /// <summary>
    ///     Merchant-room procedural visuals: same cue model as combat (<see cref="CharacterCombatVisualCueSet" />).
    /// </summary>
    /// <param name="CueSet">Texture / frame sequences keyed by animation name.</param>
    public sealed record CharacterMerchantWorldDefinition(CharacterCombatVisualCueSet CueSet);

    /// <summary>
    ///     Rest-site procedural visuals: cue keys match vanilla Spine loop names per act.
    /// </summary>
    /// <param name="CueSet">Typically <c>overgrowth_loop</c>, <c>hive_loop</c>, <c>glory_loop</c>.</param>
    public sealed record CharacterRestSiteWorldDefinition(CharacterCombatVisualCueSet CueSet);
}
