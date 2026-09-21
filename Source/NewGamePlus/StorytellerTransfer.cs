using System;
using RimWorld;
using Verse;

namespace Boundir.NewGamePlus
{
    public static class StorytellerTransfer
    {
        private static readonly WeakReference lastSeededPage = new WeakReference(null);

        public static void ApplyPreset(Page_SelectStoryteller __instance, ref StorytellerDef ___storyteller, ref DifficultyDef ___difficulty, ref Difficulty ___difficultyValues)
        {
            StorytellerPreset preset = PersistentStore.Data.storytellerPreset;
            if (preset == null || !preset.enabled)
            {
                return;
            }

            if (lastSeededPage.Target == __instance)
            {
                return;
            }

            lastSeededPage.Target = __instance;

            if (preset.storyteller != null && preset.storyteller.listVisible)
            {
                ___storyteller = preset.storyteller;
            }

            Difficulty values = preset.BuildDifficultyValues();
            if (values != null)
            {
                ___difficulty = preset.difficulty;
                ___difficultyValues = values;
            }

            if (preset.permadeathChosen && Current.ProgramState == ProgramState.Entry && Find.GameInitData != null)
            {
                Find.GameInitData.permadeathChosen = true;
                Find.GameInitData.permadeath = preset.permadeath;
            }
        }

        public static void CaptureFromGame(Game game)
        {
            Storyteller storyteller = game?.storyteller;
            if (storyteller == null)
            {
                return;
            }

            StorytellerPreset preset = PersistentStore.Data.storytellerPreset;

            preset.storyteller = storyteller.def;
            preset.difficulty = storyteller.difficultyDef;
            StorytellerPreset.CopyDifficulty(storyteller.difficulty, preset.difficultyValues);

            if (game.Info != null)
            {
                preset.permadeathChosen = true;
                preset.permadeath = game.Info.permadeathMode;
            }
        }
    }
}
