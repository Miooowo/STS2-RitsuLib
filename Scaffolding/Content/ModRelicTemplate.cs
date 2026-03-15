using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content.Patches;

namespace STS2RitsuLib.Scaffolding.Content
{
    public abstract class ModRelicTemplate : RelicModel, IModRelicAssetOverrides
    {
        protected virtual IEnumerable<string> RegisteredKeywordIds => [];
        protected virtual IEnumerable<IHoverTip> AdditionalHoverTips => [];
        protected virtual bool IncludeEnergyHoverTip => false;

        protected sealed override IEnumerable<IHoverTip> ExtraHoverTips => BuildExtraHoverTips();
        public virtual RelicAssetProfile AssetProfile => RelicAssetProfile.Empty;
        public virtual string? CustomIconPath => AssetProfile.IconPath;
        public virtual string? CustomIconOutlinePath => AssetProfile.IconOutlinePath;
        public virtual string? CustomBigIconPath => AssetProfile.BigIconPath;

        private List<IHoverTip> BuildExtraHoverTips()
        {
            var tips = new List<IHoverTip>();

            if (IncludeEnergyHoverTip)
                tips.Add(HoverTipFactory.ForEnergy(this));

            tips.AddRange(AdditionalHoverTips);
            tips.AddRange(RegisteredKeywordIds.ToHoverTips());
            tips.AddRange(this.GetModKeywordHoverTips());
            return tips;
        }
    }
}
