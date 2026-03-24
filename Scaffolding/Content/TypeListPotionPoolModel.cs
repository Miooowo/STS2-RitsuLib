using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content.Patches;

namespace STS2RitsuLib.Scaffolding.Content
{
    public abstract class TypeListPotionPoolModel : PotionPoolModel, IModBigEnergyIconPool, IModTextEnergyIconPool
    {
        protected abstract IEnumerable<Type> PotionTypes { get; }

        /// <inheritdoc cref="IModBigEnergyIconPool.BigEnergyIconPath" />
        public virtual string? BigEnergyIconPath => null;

        /// <inheritdoc cref="IModTextEnergyIconPool.TextEnergyIconPath" />
        public virtual string? TextEnergyIconPath => null;

        protected sealed override IEnumerable<PotionModel> GenerateAllPotions()
        {
            return PotionTypes
                .Select(type => ModelDb.GetById<PotionModel>(ModelDb.GetId(type)))
                .ToArray();
        }
    }
}
