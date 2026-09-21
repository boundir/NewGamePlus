using System.Collections.Generic;
using Verse;

namespace Boundir.NewGamePlus
{
    public class PersistentData : IExposable
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;

        public StorytellerPreset storytellerPreset = new StorytellerPreset();

        public List<AreaEntry> areas = new List<AreaEntry>();

        public void ExposeData()
        {
            Scribe_Values.Look(ref version, "version", CurrentVersion);
            Scribe_Deep.Look(ref storytellerPreset, "storytellerPreset");
            Scribe_Collections.Look(ref areas, "areas", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                version = CurrentVersion;
                storytellerPreset = storytellerPreset ?? new StorytellerPreset();

                areas = areas ?? new List<AreaEntry>();
                areas.RemoveAll(a => a == null || a.label.NullOrEmpty());
            }
        }
    }
}
