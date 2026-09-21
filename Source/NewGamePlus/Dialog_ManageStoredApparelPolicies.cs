using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Boundir.NewGamePlus
{
    public class Dialog_ManageStoredApparelPolicies : Dialog_ManageApparelPolicies
    {
        private static StoredPolicyCategory<ApparelPolicy> Store => PersistentStore.Data.apparel;

        public Dialog_ManageStoredApparelPolicies(ApparelPolicy policy) : base(policy)
        {
        }

        protected override ApparelPolicy CreateNewPolicy()
        {
            int id = Store.NextId();
            ApparelPolicy policy = new ApparelPolicy(id, "ApparelPolicy".Translate() + " " + id);
            policy.filter.SetAllow(ThingCategoryDefOf.Apparel, allow: true);
            Store.policies.Add(policy);
            return policy;
        }

        protected override ApparelPolicy GetDefaultPolicy()
        {
            return Store.Default();
        }

        protected override void SetDefaultPolicy(ApparelPolicy policy)
        {
            Store.SetDefault(policy);
        }

        protected override AcceptanceReport TryDeletePolicy(ApparelPolicy policy)
        {
            return Store.TryDelete(policy);
        }

        protected override List<ApparelPolicy> GetPolicies()
        {
            return Store.policies;
        }

        public override void PostClose()
        {
            base.PostClose();
            PersistentStore.Save();
        }
    }
}
