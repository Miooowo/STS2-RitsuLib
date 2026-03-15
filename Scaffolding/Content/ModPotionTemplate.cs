using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Keywords;

namespace STS2RitsuLib.Scaffolding.Content
{
    public interface IModPotionAssetOverrides
    {
        PotionAssetProfile AssetProfile { get; }
        string? CustomImagePath { get; }
        string? CustomOutlinePath { get; }
    }

    public abstract class ModPotionTemplate : PotionModel, IModPotionAssetOverrides
    {
        protected virtual IEnumerable<string> RegisteredKeywordIds => [];
        protected virtual IEnumerable<IHoverTip> AdditionalHoverTips => [];

        public sealed override IEnumerable<IHoverTip> ExtraHoverTips =>
            AdditionalHoverTips
                .Concat(RegisteredKeywordIds.ToHoverTips())
                .Concat(this.GetModKeywordHoverTips())
                .ToArray();

        public virtual PotionAssetProfile AssetProfile => PotionAssetProfile.Empty;
        public virtual string? CustomImagePath => AssetProfile.ImagePath;
        public virtual string? CustomOutlinePath => AssetProfile.OutlinePath;
    }
}
