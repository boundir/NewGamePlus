using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;

namespace Boundir.NewGamePlus
{
    [HarmonyPatch]
    public static class Outfits
    {
        [HarmonyPatch(typeof(OutfitDatabase), MethodType.Constructor)]
        public static void Postfix(OutfitDatabase __instance)
        {
            foreach (var outfit in __instance.AllOutfits)
            {
                outfit.filter.AllowedHitPointsPercents = NewGamePlus.settings.outfits;
            }
        }
    }
}
