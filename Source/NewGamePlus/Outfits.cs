using RimWorld;

namespace Boundir.NewGamePlus
{
    public static class Outfits
    {
        public static void OutfitHitPoints(OutfitDatabase __instance)
        {
            foreach (Outfit outfit in __instance.AllOutfits)
            {
                outfit.filter.AllowedHitPointsPercents = NewGamePlus.settings.outfitsHitpoints;
            }
        }

        public static void OnNewOutfit(ref Outfit __result)
        {
            if (__result is Outfit outfit)
            {
                outfit.filter.AllowedHitPointsPercents = NewGamePlus.settings.outfitsHitpoints;
            }
        }
    }
}
