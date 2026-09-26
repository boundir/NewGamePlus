using System;
using Verse;

namespace Boundir.NewGamePlus
{
    public static class OnGameSave
    {
        public static void AfterSaveGame()
        {
            try
            {
                if (!NewGamePlus.settings.captureBillsOnSave)
                {
                    return;
                }

                Game game = Current.Game;
                if (game == null)
                {
                    return;
                }

                BillPresets.ExportFromGame(game);
                PersistentStore.Save();
            }
            catch (Exception e)
            {
                Log.Error("[NewGamePlus] Failed to export bill presets on save: " + e);
            }
        }
    }
}
