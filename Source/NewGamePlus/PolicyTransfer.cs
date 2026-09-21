using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Boundir.NewGamePlus
{
    public static class PolicyTransfer
    {
        private static bool IsIdeologyFoodPolicy(FoodPolicy policy)
        {
            if (!ModsConfig.IdeologyActive)
            {
                return false;
            }

            string label = policy.label;
            return label == "FoodRestrictionVegetarian".Translate()
                || label == "FoodRestrictionCarnivore".Translate()
                || label == "FoodRestrictionCannibal".Translate()
                || label == "FoodRestrictionInsectMeat".Translate();
        }

        private static T Copy<T>(T source, Func<int, string, T> create) where T : Policy
        {
            T copy = create(source.id, source.label);
            copy.CopyFrom(source);
            copy.label = source.label;
            return copy;
        }

        private static void Capture<T>(StoredPolicyCategory<T> store, List<T> dbPolicies, Func<int, string, T> create, Func<T, bool> exclude = null) where T : Policy
        {
            store.policies.Clear();
            foreach (T policy in dbPolicies)
            {
                if (exclude != null && exclude(policy))
                {
                    continue;
                }

                store.policies.Add(Copy(policy, create));
            }
        }

        public static void CaptureApparel(Game game)
        {
            Capture(PersistentStore.Data.apparel, game.outfitDatabase.AllOutfits, (id, label) => new ApparelPolicy(id, label));
        }

        public static void CaptureFood(Game game)
        {
            Capture(PersistentStore.Data.food, game.foodRestrictionDatabase.AllFoodRestrictions, (id, label) => new FoodPolicy(id, label), IsIdeologyFoodPolicy);
        }

        public static void CaptureDrugs(Game game)
        {
            Capture(PersistentStore.Data.drugs, game.drugPolicyDatabase.AllPolicies, (id, label) => new DrugPolicy(id, label));
        }

        public static void CaptureReading(Game game)
        {
            Capture(PersistentStore.Data.reading, game.readingPolicyDatabase.AllReadingPolicies, (id, label) => new ReadingPolicy(id, label));
        }

        /// <summary>
        /// Replaces the starting policies of a freshly created game with the stored set.
        /// </summary>
        private static void Import<T>(StoredPolicyCategory<T> store, List<T> dbPolicies, Func<T> makeNew, Func<T, AcceptanceReport> tryDelete, Func<T, bool> keep = null) where T : Policy
        {
            if (!store.Active || dbPolicies.Count == 0)
            {
                return;
            }

            T stored = store.Default();
            T dbDefault = dbPolicies[0];
            dbDefault.CopyFrom(stored);
            dbDefault.label = stored.label;

            foreach (T policy in dbPolicies.Skip(1).ToList())
            {
                if (keep != null && keep(policy))
                {
                    continue;
                }

                tryDelete(policy);
            }

            for (int i = 1; i < store.policies.Count; i++)
            {
                T added = makeNew();
                added.CopyFrom(store.policies[i]);
                added.label = store.policies[i].label;
            }
        }

        public static void ImportAll(Game game)
        {
            PersistentData data = PersistentStore.Data;

            Import(data.apparel, game.outfitDatabase.AllOutfits, game.outfitDatabase.MakeNewOutfit, game.outfitDatabase.TryDelete);
            Import(data.food, game.foodRestrictionDatabase.AllFoodRestrictions, game.foodRestrictionDatabase.MakeNewFoodRestriction, game.foodRestrictionDatabase.TryDelete, IsIdeologyFoodPolicy);
            Import(data.drugs, game.drugPolicyDatabase.AllPolicies, game.drugPolicyDatabase.MakeNewDrugPolicy, game.drugPolicyDatabase.TryDelete);
            Import(data.reading, game.readingPolicyDatabase.AllReadingPolicies, game.readingPolicyDatabase.MakeNewReadingPolicy, game.readingPolicyDatabase.TryDelete);

            if (data.food.Active)
            {
                game.foodRestrictionDatabase.CreateIdeologyFoodRestrictions();
            }
        }

        // First-time editing from the main menu starts from the vanilla starting policies.
        public static void SeedApparelIfEmpty()
        {
            if (PersistentStore.Data.apparel.Active)
            {
                return;
            }

            try
            {
                Capture(PersistentStore.Data.apparel, new OutfitDatabase().AllOutfits, (id, label) => new ApparelPolicy(id, label));
            }
            catch (Exception e)
            {
                Log.Error("[NewGamePlus] Could not seed apparel policies from vanilla: " + e);
            }
        }

        public static void SeedFoodIfEmpty()
        {
            if (PersistentStore.Data.food.Active)
            {
                return;
            }

            try
            {
                Capture(PersistentStore.Data.food, new FoodRestrictionDatabase().AllFoodRestrictions, (id, label) => new FoodPolicy(id, label), IsIdeologyFoodPolicy);
            }
            catch (Exception e)
            {
                Log.Error("[NewGamePlus] Could not seed food policies from vanilla: " + e);
            }
        }

        public static void SeedDrugsIfEmpty()
        {
            if (PersistentStore.Data.drugs.Active)
            {
                return;
            }

            try
            {
                Capture(PersistentStore.Data.drugs, new DrugPolicyDatabase().AllPolicies, (id, label) => new DrugPolicy(id, label));
            }
            catch (Exception e)
            {
                Log.Error("[NewGamePlus] Could not seed drug policies from vanilla: " + e);
            }
        }

        public static void SeedReadingIfEmpty()
        {
            if (PersistentStore.Data.reading.Active)
            {
                return;
            }

            try
            {
                Capture(PersistentStore.Data.reading, new ReadingPolicyDatabase().AllReadingPolicies, (id, label) => new ReadingPolicy(id, label));
            }
            catch (Exception e)
            {
                Log.Error("[NewGamePlus] Could not seed reading policies from vanilla: " + e);
            }
        }
    }
}
