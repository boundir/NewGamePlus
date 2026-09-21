using System;
using System.Reflection;
using RimWorld;
using Verse;

namespace Boundir.NewGamePlus
{
    public class StorytellerPreset : IExposable
    {
        public bool enabled;

        public StorytellerDef storyteller;

        public DifficultyDef difficulty;

        public Difficulty difficultyValues = new Difficulty();

        public bool permadeathChosen;

        public bool permadeath;

        public bool AnyChoiceStored => storyteller != null || difficulty != null || permadeathChosen;

        public void ExposeData()
        {
            Scribe_Values.Look(ref enabled, "enabled", defaultValue: false);
            Scribe_Defs.Look(ref storyteller, "storyteller");
            Scribe_Defs.Look(ref difficulty, "difficulty");
            Scribe_Deep.Look(ref difficultyValues, "difficultyValues");
            Scribe_Values.Look(ref permadeathChosen, "permadeathChosen", defaultValue: false);
            Scribe_Values.Look(ref permadeath, "permadeath", defaultValue: false);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                difficultyValues = difficultyValues ?? new Difficulty();
            }
        }

        public void SelectDifficulty(DifficultyDef def)
        {
            if (def == null)
            {
                difficulty = null;
                return;
            }

            if (!def.isCustom)
            {
                difficultyValues.CopyFrom(def);
            }
            else if (def != difficulty)
            {
                difficultyValues.CopyFrom(DifficultyDefOf.Rough);
            }

            difficulty = def;
        }

        public Difficulty BuildDifficultyValues()
        {
            if (difficulty == null)
            {
                return null;
            }

            Difficulty values = new Difficulty();
            CopyDifficulty(difficultyValues, values);

            if (!difficulty.isCustom)
            {
                values.CopyFrom(difficulty);
            }

            return values;
        }

        public static void CopyDifficulty(Difficulty from, Difficulty to)
        {
            if (from == null || to == null)
            {
                return;
            }

            foreach (FieldInfo field in typeof(Difficulty).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (field.IsLiteral || field.IsInitOnly)
                {
                    continue;
                }

                try
                {
                    field.SetValue(to, field.GetValue(from));
                }
                catch (Exception e)
                {
                    Log.Warning("[NewGamePlus] Could not copy difficulty field " + field.Name + ": " + e.Message);
                }
            }
        }
    }
}
