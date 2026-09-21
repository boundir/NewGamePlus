using Verse;

namespace Boundir.NewGamePlus
{
    public class PersistentData : IExposable
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;

        public StorytellerPreset storytellerPreset = new StorytellerPreset();

        public void ExposeData()
        {
            Scribe_Values.Look(ref version, "version", CurrentVersion);
            Scribe_Deep.Look(ref storytellerPreset, "storytellerPreset");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                version = CurrentVersion;
                storytellerPreset = storytellerPreset ?? new StorytellerPreset();
            }
        }
    }
}
