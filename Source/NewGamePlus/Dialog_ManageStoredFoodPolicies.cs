using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Boundir.NewGamePlus
{
    public class Dialog_ManageStoredFoodPolicies : Dialog_ManageFoodPolicies
    {
        private static StoredPolicyCategory<FoodPolicy> Store => PersistentStore.Data.food;

        public Dialog_ManageStoredFoodPolicies(FoodPolicy policy) : base(policy)
        {
        }

        protected override FoodPolicy CreateNewPolicy()
        {
            int id = Store.NextId();
            FoodPolicy policy = PolicyTransfer.MakeFreshFoodPolicy(id, "FoodPolicy".Translate() + " " + id);
            Store.policies.Add(policy);
            return policy;
        }

        protected override FoodPolicy GetDefaultPolicy()
        {
            return Store.Default();
        }

        protected override void SetDefaultPolicy(FoodPolicy policy)
        {
            Store.SetDefault(policy);
        }

        protected override AcceptanceReport TryDeletePolicy(FoodPolicy policy)
        {
            return Store.TryDelete(policy);
        }

        protected override List<FoodPolicy> GetPolicies()
        {
            return Store.policies;
        }

        public override void PostClose()
        {
            base.PostClose();
            PolicyTransfer.AfterStoredFoodEdit();
        }
    }
}
