using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;
using MegaCrit.Sts2.Core.Timeline;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2RitsuLib.Timeline.Scaffolding
{
    /// <summary>
    ///     Card-unlock epoch whose gated card types are declared in the content pack via
    ///     <see cref="TimelineColumnPackEntry{TStory}" /> (not on the epoch subclass). Keeps <see cref="QueueUnlocks" />,
    ///     <see cref="EpochModel.UnlockText" />, and <see cref="Unlocks.ModUnlockRegistry" /> in sync from one manifest
    ///     registration.
    /// </summary>
    public abstract class PackDeclaredCardUnlockEpochTemplate : ModEpochTemplate
    {
        /// <summary>
        ///     Cards resolved from <see cref="ModEpochGatedContentRegistry" /> for this epoch’s <see cref="EpochModel.Id" />.
        /// </summary>
        public IReadOnlyList<CardModel> Cards => ModEpochGatedContentRegistry.ResolveCards(Id);

        /// <inheritdoc />
        public override string UnlockText => CreateCardUnlockText(Cards.ToList());

        /// <summary>
        ///     Additional epoch types to append when this epoch unlocks.
        /// </summary>
        protected virtual IEnumerable<Type> ExpansionEpochTypes => [];

        /// <inheritdoc />
        public override EpochModel[] GetTimelineExpansion()
        {
            return ExpansionEpochTypes.Select(type => Get(GetId(type))).ToArray();
        }

        /// <inheritdoc />
        public override void QueueUnlocks()
        {
            if (Cards.Count == 0)
                throw new InvalidOperationException(
                    $"Pack-declared card epoch '{Id}' has no cards in {nameof(ModEpochGatedContentRegistry)}. " +
                    "Register gated cards for this epoch via TimelineColumnPackEntry (e.g. .Epoch<TEpoch>(e => e.Cards(...))) with a non-empty list.");

            NTimelineScreen.Instance.QueueCardUnlock(Cards);

            var expansion = GetTimelineExpansion();
            if (expansion.Length > 0)
                QueueTimelineExpansion(expansion);
        }
    }
}
