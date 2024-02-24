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

            __instance.useWorkPriorities = NewGamePlus.settings.workPriorities;
        }

        public static void OnNewColony(DifficultyDef __instance)
        {
            __instance.isCustom = true;
        }
    }
}
