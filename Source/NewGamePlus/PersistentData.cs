using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Boundir.NewGamePlus
{
    public class PersistentData : IExposable
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;

        public StorytellerPreset storytellerPreset = new StorytellerPreset();

        public List<AreaEntry> areas = new List<AreaEntry>();

        public StoredPolicyCategory<ApparelPolicy> apparel = new StoredPolicyCategory<ApparelPolicy>();
        public StoredPolicyCategory<FoodPolicy> food = new StoredPolicyCategory<FoodPolicy>();
        public StoredPolicyCategory<DrugPolicy> drugs = new StoredPolicyCategory<DrugPolicy>();
        public StoredPolicyCategory<ReadingPolicy> reading = new StoredPolicyCategory<ReadingPolicy>();

        public List<BenchBillPreset> billLibrary = new List<BenchBillPreset>();

        public List<ColonyBillCapture> billCaptures = new List<ColonyBillCapture>();

        public int billStamp;

        public void ExposeData()
        {
            Scribe_Values.Look(ref version, "version", CurrentVersion);
            Scribe_Deep.Look(ref storytellerPreset, "storytellerPreset");
            Scribe_Collections.Look(ref areas, "areas", LookMode.Deep);
            Scribe_Deep.Look(ref apparel, "apparel");
            Scribe_Deep.Look(ref food, "food");
            Scribe_Deep.Look(ref drugs, "drugs");
            Scribe_Deep.Look(ref reading, "reading");
            Scribe_Collections.Look(ref billLibrary, "billLibrary", LookMode.Deep);
            Scribe_Collections.Look(ref billCaptures, "billCaptures", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                version = CurrentVersion;
                storytellerPreset = storytellerPreset ?? new StorytellerPreset();

                areas = areas ?? new List<AreaEntry>();
                if (areas.RemoveAll(a => a == null || a.label.NullOrEmpty()) > 0)
                {
                    PersistentStore.NotifyPrunedOnLoad();
                }

                apparel = apparel ?? new StoredPolicyCategory<ApparelPolicy>();
                food = food ?? new StoredPolicyCategory<FoodPolicy>();
                drugs = drugs ?? new StoredPolicyCategory<DrugPolicy>();
                reading = reading ?? new StoredPolicyCategory<ReadingPolicy>();

                billLibrary = billLibrary ?? new List<BenchBillPreset>();
                if (billLibrary.RemoveAll(p => p == null || p.benchDef == null || p.bills.Count == 0) > 0)
                {
                    PersistentStore.NotifyPrunedOnLoad();
                }

                billCaptures = billCaptures ?? new List<ColonyBillCapture>();
                if (billCaptures.RemoveAll(c => c == null || c.benches.Count == 0) > 0)
                {
                    PersistentStore.NotifyPrunedOnLoad();
                }

                billStamp++;
            }
        }
    }
}
