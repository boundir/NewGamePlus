using Verse;

namespace Boundir.NewGamePlus
{
    public class PersistentData : IExposable
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;

        public void ExposeData()
        {
            Scribe_Values.Look(ref version, "version", CurrentVersion);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                version = CurrentVersion;
            }
        }
    }
}
