using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content.Patches;

namespace STS2RitsuLib.Scaffolding.Content
{
    public abstract class ModPowerTemplate : PowerModel, IModPowerAssetOverrides
    {
        protected virtual IEnumerable<string> RegisteredKeywordIds => [];
        protected virtual IEnumerable<IHoverTip> AdditionalHoverTips => [];
        protected virtual bool IncludeEnergyHoverTip => false;
        public virtual PowerAssetProfile AssetProfile => PowerAssetProfile.Empty;
        public virtual string? CustomIconPath => AssetProfile.IconPath;
        public virtual string? CustomBigIconPath => AssetProfile.BigIconPath;

        protected sealed override IEnumerable<IHoverTip> ExtraHoverTips => BuildExtraHoverTips();

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
