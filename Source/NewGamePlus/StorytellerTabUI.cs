using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Boundir.NewGamePlus
{
    public static class StorytellerTabUI
    {
        private static Vector2 scrollPosition;

        private static float contentHeight;

        private const float CHOICE_COLUMN_WIDTH = 460f;

        private const float FOOTER_HEIGHT = 30f;
        private const float DESCRIPTION_HEIGHT = 52f;

        private static Action<Listing_Standard, Difficulty> drawCustomLeft;
        private static Action<Listing_Standard, Difficulty> drawCustomRight;
        private static bool customDrawersResolved;

        private static readonly Listing_Standard choiceListing = new Listing_Standard();

        public static void Draw(Rect rect)
        {
            StorytellerPreset preset = PersistentStore.Data.storytellerPreset;

            Rect noteRect = new Rect(rect.x, rect.y, rect.width, DESCRIPTION_HEIGHT);
            Text.Font = GameFont.Tiny;
            GUI.color = Color.gray;
            Widgets.Label(noteRect, "NGP_StorytellerTabDesc".Translate());
            Text.Font = GameFont.Small;
            GUI.color = Color.white;

            Rect footer = new Rect(rect.x, rect.yMax - FOOTER_HEIGHT - 4f, rect.width, FOOTER_HEIGHT);
            Rect outRect = new Rect(rect.x, noteRect.yMax + 4f, rect.width, footer.y - noteRect.yMax - 12f);

            Rect viewRect = new Rect(0f, 0f, outRect.width - 20f, Mathf.Max(contentHeight, outRect.height));
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            float y = DrawChoices(viewRect, preset);

            if (preset.difficulty != null && preset.difficulty.isCustom)
            {
                y = DrawCustomDifficulty(viewRect, preset.difficultyValues, y + 12f);
            }

            contentHeight = y + 12f;

            Widgets.EndScrollView();

            DrawFooter(footer, preset);
        }

        private static float DrawChoices(Rect viewRect, StorytellerPreset preset)
        {
            choiceListing.maxOneColumn = true;
            choiceListing.ColumnWidth = Mathf.Min(CHOICE_COLUMN_WIDTH, viewRect.width);
            choiceListing.Begin(new Rect(0f, 0f, choiceListing.ColumnWidth, 9999f));

            choiceListing.CheckboxLabeled("NGP_StorytellerEnable".Translate(), ref preset.enabled, "NGP_StorytellerEnableDesc".Translate());

            if (preset.enabled && !preset.AnyChoiceStored)
            {
                GUI.color = ColoredText.SubtleGrayColor;
                Text.Font = GameFont.Tiny;
                choiceListing.Label("NGP_StorytellerNothingStored".Translate());
                Text.Font = GameFont.Small;
                GUI.color = Color.white;
            }

            choiceListing.Gap();
            choiceListing.Label("NGP_StorytellerSection".Translate());

            if (choiceListing.RadioButton("NGP_LeaveUnchosen".Translate(), preset.storyteller == null, 8f, "NGP_LeaveUnchosenDesc".Translate(), 0f))
            {
                preset.storyteller = null;
            }

            foreach (StorytellerDef def in DefDatabase<StorytellerDef>.AllDefs.Where(d => d.listVisible).OrderBy(d => d.listOrder))
            {
                if (choiceListing.RadioButton(def.LabelCap, preset.storyteller == def, 8f, def.description, 0f))
                {
                    preset.storyteller = def;
                }
            }

            choiceListing.Gap();
            choiceListing.Label("NGP_DifficultySection".Translate());

            if (choiceListing.RadioButton("NGP_LeaveUnchosen".Translate(), preset.difficulty == null, 8f, "NGP_LeaveUnchosenDesc".Translate(), 0f))
            {
                preset.SelectDifficulty(null);
            }

            foreach (DifficultyDef def in DefDatabase<DifficultyDef>.AllDefs)
            {
                TaggedString label = def.LabelCap;
                if (def.isCustom)
                {
                    label += "...";
                }

                if (choiceListing.RadioButton(label, preset.difficulty == def, 8f, def.description.ResolveTags(), 0f))
                {
                    preset.SelectDifficulty(def);
                }
            }

            choiceListing.Gap();
            choiceListing.Label("NGP_SaveModeSection".Translate());

            if (choiceListing.RadioButton("NGP_LeaveUnchosen".Translate(), !preset.permadeathChosen, 8f, "NGP_LeaveUnchosenDesc".Translate(), 0f))
            {
                preset.permadeathChosen = false;
            }

            if (choiceListing.RadioButton("ReloadAnytimeMode".Translate(), preset.permadeathChosen && !preset.permadeath, 8f, "ReloadAnytimeModeInfo".Translate(), 0f))
            {
                preset.permadeathChosen = true;
                preset.permadeath = false;
            }

            if (choiceListing.RadioButton("CommitmentMode".TranslateWithBackup("PermadeathMode"), preset.permadeathChosen && preset.permadeath, 8f, "PermadeathModeInfo".Translate(), 0f))
            {
                preset.permadeathChosen = true;
                preset.permadeath = true;
            }

            if (ModsConfig.AnomalyActive)
            {
                DrawAnomalyPlaystyles(preset);
            }

            float used = choiceListing.CurHeight;
            choiceListing.End();
            return used;
        }

        private static void DrawAnomalyPlaystyles(StorytellerPreset preset)
        {
            choiceListing.Gap();
            choiceListing.Label("ChooseAnomalyPlaystyle".Translate());

            Difficulty values = preset.difficultyValues;

            Scenario scenario = Find.Scenario;

            foreach (AnomalyPlaystyleDef def in DefDatabase<AnomalyPlaystyleDef>.AllDefs)
            {
                bool allowed = scenario == null || !scenario.standardAnomalyPlaystyleOnly || def == AnomalyPlaystyleDefOf.Standard;
                string tooltip = def.LabelCap.AsTipTitle() + "\n" + def.description;
                if (!allowed)
                {
                    tooltip = tooltip + "\n\n" + ("DisabledByScenario".Translate() + ": " + scenario.name).Colorize(ColorLibrary.RedReadable);
                }

                if (choiceListing.RadioButton(def.LabelCap, values.AnomalyPlaystyleDef == def, 8f, tooltip, 0f) && allowed)
                {
                    if (def.overrideThreatFraction)
                    {
                        values.overrideAnomalyThreatsFraction = values.overrideAnomalyThreatsFraction ?? 0.15f;
                    }
                    else
                    {
                        values.overrideAnomalyThreatsFraction = null;
                    }

                    values.AnomalyPlaystyleDef = def;
                }
            }
        }

        private static float DrawCustomDifficulty(Rect viewRect, Difficulty difficultyValues, float y)
        {
            ResolveCustomDrawers();

            Listing_Standard listing = new Listing_Standard
            {
                ColumnWidth = viewRect.width / 2f - 17f
            };

            Rect rect = new Rect(0f, y, viewRect.width, 9999f);
            listing.Begin(rect);

            Text.Font = GameFont.Medium;
            listing.Indent(15f);
            listing.Label("DifficultyCustomSectionLabel".Translate());
            listing.Outdent(15f);
            Text.Font = GameFont.Small;
            listing.Gap();

            if (listing.ButtonText("DifficultyReset".Translate()))
            {
                MakeResetDifficultyFloatMenu(difficultyValues);
            }

            if (drawCustomLeft == null || drawCustomRight == null)
            {
                GUI.color = ColoredText.WarningColor;
                listing.Label("NGP_CustomDifficultyUnavailable".Translate());
                GUI.color = Color.white;

                float fallbackHeight = rect.y + listing.CurHeight;
                listing.End();
                return fallbackHeight;
            }

            float headerHeight = listing.CurHeight;
            drawCustomLeft(listing, difficultyValues);
            listing.NewColumn();
            listing.Gap(headerHeight);
            drawCustomRight(listing, difficultyValues);

            float height = rect.y + listing.MaxColumnHeightSeen;
            listing.End();

            GUI.enabled = true;
            return height;
        }

        private static void DrawFooter(Rect footer, StorytellerPreset preset)
        {
            float buttonWidth = (footer.width - 10f) / 2f;
            Rect captureRect = new Rect(footer.x, footer.y, buttonWidth, footer.height);
            Rect resetRect = new Rect(captureRect.xMax + 10f, footer.y, buttonWidth, footer.height);

            bool canCapture = Current.Game != null && Current.Game.storyteller != null;
            if (Utils.DisableableButton(captureRect, "NGP_CaptureFromGame".Translate(), canCapture))
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("NGP_ConfirmCaptureStoryteller".Translate(), delegate
                {
                    StorytellerTransfer.CaptureFromGame(Current.Game);
                    PersistentStore.Save();
                }));
            }

            if (Utils.DisableableButton(resetRect, "NGP_ResetToVanilla".Translate(), preset.AnyChoiceStored || preset.enabled))
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("NGP_ConfirmResetStoryteller".Translate(), delegate
                {
                    PersistentStore.Data.storytellerPreset = new StorytellerPreset();
                    PersistentStore.Save();
                }));
            }
        }

        private static void MakeResetDifficultyFloatMenu(Difficulty difficultyValues)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (DifficultyDef def in DefDatabase<DifficultyDef>.AllDefs.Where(d => !d.isCustom))
            {
                DifficultyDef local = def;
                options.Add(new FloatMenuOption(local.LabelCap, delegate
                {
                    difficultyValues.CopyFrom(local);
                }));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private static void ResolveCustomDrawers()
        {
            if (customDrawersResolved)
            {
                return;
            }

            customDrawersResolved = true;

            Type[] signature = { typeof(Listing_Standard), typeof(Difficulty) };
            MethodInfo left = AccessTools.Method(typeof(StorytellerUI), "DrawCustomLeft", signature);
            MethodInfo right = AccessTools.Method(typeof(StorytellerUI), "DrawCustomRight", signature);

            if (left == null || right == null)
            {
                Log.Error("[NewGamePlus] StorytellerUI.DrawCustomLeft/DrawCustomRight not found - the custom difficulty panel will not be shown in the mod settings. Named difficulties still work.");
                return;
            }

            drawCustomLeft = (Action<Listing_Standard, Difficulty>)Delegate.CreateDelegate(typeof(Action<Listing_Standard, Difficulty>), left);
            drawCustomRight = (Action<Listing_Standard, Difficulty>)Delegate.CreateDelegate(typeof(Action<Listing_Standard, Difficulty>), right);
        }
    }
}
