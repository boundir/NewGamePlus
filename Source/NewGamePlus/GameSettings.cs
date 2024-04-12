using RimWorld;
using Verse;

namespace Boundir.NewGamePlus
{
    public static class GameSettings
    {
        public static void DefaultPlaySettings(PlaySettings __instance)
        {
            __instance.autoHomeArea = NewGamePlus.settings.autoExpandHomeArea;
            __instance.autoRebuild = NewGamePlus.settings.autoRebuild;
            __instance.showZones = NewGamePlus.settings.showZones;

            // Medical Care
            __instance.defaultCareForColonist = NewGamePlus.settings.medicalCareColonist;
            __instance.defaultCareForPrisoner = NewGamePlus.settings.medicalCarePrisoner;
            __instance.defaultCareForTamedAnimal = NewGamePlus.settings.medicalCareColonyAnimal;

            if (ModsConfig.IdeologyActive)
            {
                __instance.defaultCareForSlave = NewGamePlus.settings.medicalCareSlave;
            }

            if (ModsConfig.AnomalyActive)
            {
                __instance.defaultCareForEntities = NewGamePlus.settings.medicalCareEntities;
                __instance.defaultCareForGhouls = NewGamePlus.settings.medicalCareGhouls;
            }

            __instance.defaultCareForFriendlyFaction = NewGamePlus.settings.medicalCareFriendlyFaction;
            __instance.defaultCareForNeutralFaction = NewGamePlus.settings.medicalCareNeutralFaction;
            __instance.defaultCareForHostileFaction = NewGamePlus.settings.medicalCareHostileFaction;

            __instance.defaultCareForNoFaction = NewGamePlus.settings.medicalCareNoFaction;
            __instance.defaultCareForWildlife = NewGamePlus.settings.medicalCareWildlife;

            __instance.useWorkPriorities = NewGamePlus.settings.workPriorities;
        }

        public static void OnNewColony(DifficultyDef __instance)
        {
            __instance.isCustom = true;
        }
    }
}
