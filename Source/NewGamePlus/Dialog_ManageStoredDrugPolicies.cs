using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Boundir.NewGamePlus
{
    public class Dialog_ManageStoredDrugPolicies : Dialog_ManageDrugPolicies
    {
        private static StoredPolicyCategory<DrugPolicy> Store => PersistentStore.Data.drugs;

        public Dialog_ManageStoredDrugPolicies(DrugPolicy policy) : base(policy)
        {
        }

        protected override DrugPolicy CreateNewPolicy()
        {
            int id = Store.NextId();
            DrugPolicy policy = new DrugPolicy(id, "DrugPolicy".Translate() + " " + id);
            Store.policies.Add(policy);
            return policy;
        }

        protected override DrugPolicy GetDefaultPolicy()
        {
            return Store.Default();
        }

        protected override void SetDefaultPolicy(DrugPolicy policy)
        {
            Store.SetDefault(policy);
        }

        protected override AcceptanceReport TryDeletePolicy(DrugPolicy policy)
        {
            return Store.TryDelete(policy);
        }

        protected override List<DrugPolicy> GetPolicies()
        {
            return Store.policies;
        }

        public override void PostClose()
        {
            base.PostClose();
            PolicyTransfer.AfterStoredDrugsEdit();
        }
    }
}
