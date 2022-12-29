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
    public static class GameSettings
    {
        [HarmonyPatch(typeof(PlaySettings), MethodType.Constructor)]
        public static void Postfix(PlaySettings __instance)
        {
            __instance.autoHomeArea = NewGamePlus.settings.autoExpandHomeArea;
            __instance.autoRebuild = NewGamePlus.settings.autoRebuild;
            __instance.showZones = NewGamePlus.settings.showZones;

            // Medical Care
            __instance.defaultCareForColonyHumanlike = NewGamePlus.settings.medicalCareColonist;
            __instance.defaultCareForColonyAnimal = NewGamePlus.settings.medicalCareColonyAnimal;

            if (ModsConfig.IdeologyActive)
            {
                __instance.defaultCareForColonySlave = NewGamePlus.settings.medicalCareSlave;
            }

            __instance.defaultCareForNeutralFaction = NewGamePlus.settings.medicalCareNeutralFaction;
            __instance.defaultCareForNeutralAnimal = NewGamePlus.settings.medicalCareNeutralAnimal;

            __instance.defaultCareForColonyPrisoner = NewGamePlus.settings.medicalCarePrisoner;
            __instance.defaultCareForHostileFaction = NewGamePlus.settings.medicalCareHostileFaction;
        }
    }
}
