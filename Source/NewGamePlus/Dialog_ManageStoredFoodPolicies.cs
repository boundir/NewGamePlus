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
            FoodPolicy policy = new FoodPolicy(id, "FoodPolicy".Translate() + " " + id);
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs.Where(x => x.GetStatValueAbstract(StatDefOf.Nutrition) > 0f))
            {
                policy.filter.SetAllow(def, allow: true);
            }

            if (ModsConfig.IdeologyActive)
            {
                policy.filter.SetAllow(SpecialThingFilterDefOf.AllowVegetarian, allow: true);
                policy.filter.SetAllow(SpecialThingFilterDefOf.AllowCarnivore, allow: true);
                policy.filter.SetAllow(SpecialThingFilterDefOf.AllowCannibal, allow: true);
                policy.filter.SetAllow(SpecialThingFilterDefOf.AllowInsectMeat, allow: true);
            }

            if (ModsConfig.BiotechActive)
            {
                policy.filter.SetAllow(ThingDefOf.HemogenPack, allow: false);
            }

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
            PersistentStore.Save();
        }
    }
}
