using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        // Mirrors OutfitDatabase.MakeNewOutfit, plus the mod's hit point default.
        public static ApparelPolicy MakeFreshApparelPolicy(int id, string label)
        {
            ApparelPolicy policy = new ApparelPolicy(id, label);
            policy.filter.SetAllow(ThingCategoryDefOf.Apparel, allow: true);
            policy.filter.AllowedHitPointsPercents = NewGamePlus.settings.outfitsHitpoints;
            return policy;
        }

        // Mirrors FoodRestrictionDatabase.MakeNewFoodRestriction.
        public static FoodPolicy MakeFreshFoodPolicy(int id, string label)
        {
            FoodPolicy policy = new FoodPolicy(id, label);
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

            return policy;
        }

        private static List<ThingDef> ApparelDomain()
        {
            return MakeFreshApparelPolicy(0, "").filter.AllowedThingDefs.ToList();
        }

        private static List<ThingDef> FoodDomain()
        {
            return MakeFreshFoodPolicy(0, "").filter.AllowedThingDefs.ToList();
        }

        private static List<ThingDef> ReadingDomain()
        {
            return DefDatabase<ThingDef>.AllDefs.Where(d => d.thingClass.SameOrSubclassOf<Book>()).ToList();
        }

        private static bool AllowsAllOf(ThingFilter filter, List<ThingDef> domain)
        {
            for (int i = 0; i < domain.Count; i++)
            {
                if (!filter.Allows(domain[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static void RecomputeAllowAll<T>(StoredPolicyCategory<T> store, Func<T, ThingFilter> filterOf, List<ThingDef> domain) where T : Policy
        {
            store.allowAllIds.Clear();
            foreach (T policy in store.policies)
            {
                if (AllowsAllOf(filterOf(policy), domain))
                {
                    store.allowAllIds.Add(policy.id);
                }
            }
        }

        public static void RecomputeAllowAll()
        {
            PersistentData data = PersistentStore.Data;
            RecomputeAllowAll(data.apparel, p => p.filter, ApparelDomain());
            RecomputeAllowAll(data.food, p => p.filter, FoodDomain());
            RecomputeAllowAll(data.reading, p => p.defFilter, ReadingDomain());
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
            RecomputeAllowAll();
        }

        public static void CaptureFood(Game game)
        {
            Capture(PersistentStore.Data.food, game.foodRestrictionDatabase.AllFoodRestrictions, (id, label) => new FoodPolicy(id, label), IsIdeologyFoodPolicy);
            RecomputeAllowAll();
        }

        public static void CaptureDrugs(Game game)
        {
            Capture(PersistentStore.Data.drugs, game.drugPolicyDatabase.AllPolicies, (id, label) => new DrugPolicy(id, label));
        }

        public static void CaptureReading(Game game)
        {
            Capture(PersistentStore.Data.reading, game.readingPolicyDatabase.AllReadingPolicies, (id, label) => new ReadingPolicy(id, label));
            RecomputeAllowAll();
        }

        /// <summary>
        /// Replaces the starting policies of a freshly created game with the stored set.
        /// </summary>
        private static void Import<T>(StoredPolicyCategory<T> store, List<T> dbPolicies, Func<T> makeNew, Func<T, AcceptanceReport> tryDelete,
            Func<T, ThingFilter> filterOf = null, Func<List<ThingDef>> domainGetter = null, Func<T, bool> keep = null) where T : Policy
        {
            if (!store.Active || dbPolicies.Count == 0)
            {
                return;
            }

            List<ThingDef> domain = null;

            void ReopenIfAllowAll(T source, T target)
            {
                if (filterOf == null || domainGetter == null || !store.allowAllIds.Contains(source.id))
                {
                    return;
                }

                domain = domain ?? domainGetter();
                ThingFilter filter = filterOf(target);
                for (int i = 0; i < domain.Count; i++)
                {
                    filter.SetAllow(domain[i], allow: true);
                }
            }

            T stored = store.Default();
            T dbDefault = dbPolicies[0];
            dbDefault.CopyFrom(stored);
            dbDefault.label = stored.label;
            ReopenIfAllowAll(stored, dbDefault);

            foreach (T policy in dbPolicies.Skip(1).ToList())
            {
                if (keep != null && keep(policy))
                {
                    continue;
                }

                AcceptanceReport report = tryDelete(policy);
                if (!report.Accepted)
                {
                    Log.Warning("[NewGamePlus] Could not remove starting policy '" + policy.label + "': " + report.Reason);
                }
            }

            for (int i = 1; i < store.policies.Count; i++)
            {
                T added = makeNew();
                added.CopyFrom(store.policies[i]);
                added.label = store.policies[i].label;
                ReopenIfAllowAll(store.policies[i], added);
            }
        }

        public static void ImportAll(Game game)
        {
            PersistentData data = PersistentStore.Data;

            Import(data.apparel, game.outfitDatabase.AllOutfits, game.outfitDatabase.MakeNewOutfit, game.outfitDatabase.TryDelete,
                p => p.filter, ApparelDomain);
            Import(data.food, game.foodRestrictionDatabase.AllFoodRestrictions, game.foodRestrictionDatabase.MakeNewFoodRestriction, game.foodRestrictionDatabase.TryDelete,
                p => p.filter, FoodDomain, IsIdeologyFoodPolicy);
            Import(data.drugs, game.drugPolicyDatabase.AllPolicies, game.drugPolicyDatabase.MakeNewDrugPolicy, game.drugPolicyDatabase.TryDelete);
            Import(data.reading, game.readingPolicyDatabase.AllReadingPolicies, game.readingPolicyDatabase.MakeNewReadingPolicy, game.readingPolicyDatabase.TryDelete,
                p => p.defFilter, ReadingDomain);

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
                RecomputeAllowAll();
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
                RecomputeAllowAll();
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

        public static void AfterStoredApparelEdit()
        {
            StoredPolicyCategory<ApparelPolicy> store = PersistentStore.Data.apparel;
            if (MatchesVanilla(store.policies, new OutfitDatabase().AllOutfits, p => FilterPolicyKey(p, p.filter)))
            {
                store.Clear();
            }
            else
            {
                RecomputeAllowAll();
            }

            PersistentStore.Save();
        }

        public static void AfterStoredFoodEdit()
        {
            StoredPolicyCategory<FoodPolicy> store = PersistentStore.Data.food;
            List<FoodPolicy> vanilla = new FoodRestrictionDatabase().AllFoodRestrictions.Where(p => !IsIdeologyFoodPolicy(p)).ToList();
            if (MatchesVanilla(store.policies, vanilla, p => FilterPolicyKey(p, p.filter)))
            {
                store.Clear();
            }
            else
            {
                RecomputeAllowAll();
            }

            PersistentStore.Save();
        }

        public static void AfterStoredDrugsEdit()
        {
            StoredPolicyCategory<DrugPolicy> store = PersistentStore.Data.drugs;
            if (MatchesVanilla(store.policies, new DrugPolicyDatabase().AllPolicies, DrugPolicyKey))
            {
                store.Clear();
            }

            PersistentStore.Save();
        }

        public static void AfterStoredReadingEdit()
        {
            StoredPolicyCategory<ReadingPolicy> store = PersistentStore.Data.reading;
            if (MatchesVanilla(store.policies, new ReadingPolicyDatabase().AllReadingPolicies,
                p => FilterPolicyKey(p, p.defFilter) + "|" + BillRecordKey.FilterKey(p.effectFilter)))
            {
                store.Clear();
            }
            else
            {
                RecomputeAllowAll();
            }

            PersistentStore.Save();
        }

        private static bool MatchesVanilla<T>(List<T> stored, List<T> vanilla, Func<T, string> key) where T : Policy
        {
            if (stored.Count != vanilla.Count)
            {
                return false;
            }

            for (int i = 0; i < stored.Count; i++)
            {
                if (key(stored[i]) != key(vanilla[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static string FilterPolicyKey(Policy policy, ThingFilter filter)
        {
            return policy.id + "|" + policy.label + "|" + BillRecordKey.FilterKey(filter);
        }

        private static string DrugPolicyKey(DrugPolicy policy)
        {
            StringBuilder sb = new StringBuilder(128);
            sb.Append(policy.id).Append('|').Append(policy.label);
            for (int i = 0; i < policy.Count; i++)
            {
                DrugPolicyEntry entry = policy[i];
                sb.Append('|').Append(entry.drug?.defName ?? "?")
                    .Append(entry.allowedForAddiction ? '1' : '0')
                    .Append(entry.allowedForJoy ? '1' : '0')
                    .Append(entry.allowScheduled ? '1' : '0')
                    .Append(entry.takeToInventory)
                    .Append(':').Append(entry.daysFrequency.ToString("F2"))
                    .Append(':').Append(entry.onlyIfMoodBelow.ToString("F2"))
                    .Append(':').Append(entry.onlyIfJoyBelow.ToString("F2"));
            }

            return sb.ToString();
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
                RecomputeAllowAll();
            }
            catch (Exception e)
            {
                Log.Error("[NewGamePlus] Could not seed reading policies from vanilla: " + e);
            }
        }
    }
}
