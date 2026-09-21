using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Boundir.NewGamePlus
{
    public class ColonyBillCapture : IExposable, IRenameable
    {
        public int colonyId;

        /// Faction.OfPlayer.Name - what the player calls the colony.
        public string colonyName;

        /// Find.World.info.name - the planet, which is not the colony name.
        public string worldName;

        /// Set when the player renames the group in our UI; wins over both names above.
        public string customName;

        /// Unix seconds of the last refresh, for display and for newest-first ordering.
        public long capturedAtUnix;

        public List<BenchBillPreset> benches = new List<BenchBillPreset>();

        public string Label => RenamableLabel;

        // IRenameable, so the same rename dialog serves bills and colony groups.
        public string BaseLabel
        {
            get
            {
                if (!colonyName.NullOrEmpty())
                {
                    return colonyName;
                }

                if (!worldName.NullOrEmpty())
                {
                    return worldName;
                }

                return "NGP_UnknownColony".Translate();
            }
        }

        public string RenamableLabel
        {
            get { return customName.NullOrEmpty() ? BaseLabel : customName; }
            set { customName = (value.NullOrEmpty() || value == BaseLabel) ? null : value; }
        }

        public string InspectLabel => RenamableLabel;

        public int BillCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < benches.Count; i++)
                {
                    count += benches[i].bills.Count;
                }

                return count;
            }
        }

        public BenchBillPreset BenchGroup(ThingDef benchDef, bool create)
        {
            BenchBillPreset group = benches.FirstOrDefault(b => b.benchDef == benchDef);
            if (group == null && create)
            {
                group = new BenchBillPreset { benchDef = benchDef };
                benches.Add(group);
            }

            return group;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref colonyId, "colonyId", 0);
            Scribe_Values.Look(ref colonyName, "colonyName");
            Scribe_Values.Look(ref worldName, "worldName");
            Scribe_Values.Look(ref customName, "customName");
            Scribe_Values.Look(ref capturedAtUnix, "capturedAtUnix", 0L);
            Scribe_Collections.Look(ref benches, "benches", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (benches == null)
                {
                    benches = new List<BenchBillPreset>();
                }

                benches.RemoveAll(b => b == null || b.benchDef == null || b.bills.Count == 0);
            }
        }
    }
}
