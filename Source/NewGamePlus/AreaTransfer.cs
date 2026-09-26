using System.Linq;
using RimWorld;
using Verse;

namespace Boundir.NewGamePlus
{
    public static class AreaTransfer
    {
        public static void CaptureFromGame(Game game)
        {
            Map map = Find.CurrentMap ?? game.Maps.FirstOrDefault(m => m.IsPlayerHome);
            if (map == null)
            {
                return;
            }

            PersistentStore.Data.areas.Clear();
            foreach (Area_Allowed area in map.areaManager.AllAreas.OfType<Area_Allowed>())
            {
                PersistentStore.Data.areas.Add(new AreaEntry
                {
                    label = area.Label,
                    color = area.Color
                });
            }
        }

        public static void ImportAll(Game game)
        {
            if (PersistentStore.Data.areas.Count == 0)
            {
                return;
            }

            foreach (Map map in game.Maps)
            {
                if (!map.IsPlayerHome)
                {
                    continue;
                }

                int dropped = 0;
                foreach (AreaEntry entry in PersistentStore.Data.areas)
                {
                    if (map.areaManager.GetLabeled(entry.label) != null)
                    {
                        continue;
                    }

                    if (!map.areaManager.TryMakeNewAllowed(out Area_Allowed area))
                    {
                        dropped++;
                        continue;
                    }

                    area.SetLabel(entry.label);
                    area.SetColor(entry.color);
                }

                if (dropped > 0)
                {
                    Log.Warning("[NewGamePlus] " + dropped + " stored allowed area(s) were not created on " + map
                        + ": the game allows at most 10 allowed areas per map.");
                }
            }
        }
    }
}
