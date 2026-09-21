using System;
using System.IO;
using Verse;

namespace Boundir.NewGamePlus
{
    public static class PersistentStore
    {
        private static PersistentData data;
        private static bool loaded;

        public static PersistentData Data
        {
            get
            {
                EnsureLoaded();
                return data;
            }
        }

        public static string FilePath
        {
            get
            {
                // Mirrors the private GenFilePaths.FolderUnderSaveData.
                string folder = Path.Combine(GenFilePaths.SaveDataFolderPath, "NewGamePlus");
                DirectoryInfo directory = new DirectoryInfo(folder);
                if (!directory.Exists)
                {
                    directory.Create();
                }

                return Path.Combine(folder, "NewGamePlusData.xml");
            }
        }

        public static void EnsureLoaded()
        {
            if (loaded)
            {
                return;
            }

            loaded = true;
            data = new PersistentData();

            try
            {
                if (!File.Exists(FilePath))
                {
                    return;
                }

                Scribe.loader.InitLoading(FilePath);
                try
                {
                    PersistentData loadedData = null;
                    Scribe_Deep.Look(ref loadedData, "data");
                    Scribe.loader.FinalizeLoading();

                    if (loadedData != null)
                    {
                        data = loadedData;
                    }
                }
                catch
                {
                    Scribe.ForceStop();
                    throw;
                }
            }
            catch (Exception e)
            {
                Log.Warning("[NewGamePlus] Could not load persistent data from " + FilePath + ", starting fresh. " + e);
                data = new PersistentData();
            }
        }

        public static void Save()
        {
            EnsureLoaded();

            if (Scribe.mode != LoadSaveMode.Inactive)
            {
                Log.Warning("[NewGamePlus] Skipped saving persistent data: another scribe operation is in progress.");
                return;
            }

            try
            {
                SafeSaver.Save(FilePath, "NewGamePlusData", delegate
                {
                    PersistentData toSave = data;
                    Scribe_Deep.Look(ref toSave, "data");
                });
            }
            catch (Exception e)
            {
                Log.Error("[NewGamePlus] Failed to save persistent data to " + FilePath + ": " + e);
            }
        }

        public static void SaveIfLoaded()
        {
            if (loaded)
            {
                Save();
            }
        }
    }
}
