using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;

namespace Boundir.NewGamePlus
{
    [HarmonyPatch]
    public static class FactionRewards
    {
        [HarmonyPatch(typeof(Faction), MethodType.Constructor)]
        public static void Postfix(Faction __instance)
        {
            __instance.allowGoodwillRewards = NewGamePlus.settings.allowGoodwillRewards;
            __instance.allowRoyalFavorRewards = NewGamePlus.settings.allowRoyalFavorRewards;
        }
    }
}
