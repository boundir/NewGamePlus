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

        public void ExposeData()
        {
            Scribe_Values.Look(ref version, "version", CurrentVersion);
            Scribe_Deep.Look(ref storytellerPreset, "storytellerPreset");
            Scribe_Collections.Look(ref areas, "areas", LookMode.Deep);
            Scribe_Deep.Look(ref apparel, "apparel");
            Scribe_Deep.Look(ref food, "food");
            Scribe_Deep.Look(ref drugs, "drugs");
            Scribe_Deep.Look(ref reading, "reading");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                version = CurrentVersion;
                storytellerPreset = storytellerPreset ?? new StorytellerPreset();

                areas = areas ?? new List<AreaEntry>();
                areas.RemoveAll(a => a == null || a.label.NullOrEmpty());

                apparel = apparel ?? new StoredPolicyCategory<ApparelPolicy>();
                food = food ?? new StoredPolicyCategory<FoodPolicy>();
                drugs = drugs ?? new StoredPolicyCategory<DrugPolicy>();
                reading = reading ?? new StoredPolicyCategory<ReadingPolicy>();
            }
        }
    }
}
