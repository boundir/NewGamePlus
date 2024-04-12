using RimWorld;

namespace Boundir.NewGamePlus
{
    public static class Outfits
    {
        public static void OutfitHitPoints(OutfitDatabase __instance)
        {
            foreach (ApparelPolicy outfit in __instance.AllOutfits)
            {
                outfit.filter.AllowedHitPointsPercents = NewGamePlus.settings.outfitsHitpoints;
            }
        }

        public static void OnNewOutfit(ref ApparelPolicy __result)
        {
            if (__result is ApparelPolicy outfit)
            {
                outfit.filter.AllowedHitPointsPercents = NewGamePlus.settings.outfitsHitpoints;
            }
        }
    }
}
