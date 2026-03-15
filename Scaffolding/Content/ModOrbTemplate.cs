using Godot;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content.Patches;

namespace STS2RitsuLib.Scaffolding.Content
{
    public abstract class ModOrbTemplate : OrbModel, IModOrbAssetOverrides
    {
        protected virtual IEnumerable<string> RegisteredKeywordIds => [];
        protected virtual IEnumerable<IHoverTip> AdditionalHoverTips => [];
        public virtual OrbAssetProfile AssetProfile => OrbAssetProfile.Empty;
        public virtual string? CustomIconPath => AssetProfile.IconPath;
        public virtual string? CustomVisualsScenePath => AssetProfile.VisualsScenePath;

        protected sealed override IEnumerable<IHoverTip> ExtraHoverTips =>
            AdditionalHoverTips
                .Concat(RegisteredKeywordIds.ToHoverTips())
                .Concat(this.GetModKeywordHoverTips())
                .ToArray();

        public override Color DarkenedColor => Colors.DarkSlateGray;
    }
}
