using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Boundir.NewGamePlus
{
    internal static class BillDetails
    {
        private const float LabelFraction = 0.34f;
        private const float Padding = 6f;
        private const int MaxIngredientNames = 8;

        public static List<KeyValuePair<string, string>> Lines(BillRecord record, ThingDef originBench = null, string originColony = null)
        {
            List<KeyValuePair<string, string>> lines = new List<KeyValuePair<string, string>>();
            if (record == null)
            {
                return lines;
            }

            Add(lines, "NGP_DetailRecipe", record.recipe != null ? record.recipe.LabelCap.ToString() : "?");

            if (originBench != null || !originColony.NullOrEmpty())
            {
                string benchLabel = originBench != null ? originBench.LabelCap.ToString() : null;
                string origin;
                if (originColony.NullOrEmpty())
                {
                    origin = benchLabel;
                }
                else if (benchLabel == null)
                {
                    origin = originColony;
                }
                else
                {
                    origin = originColony + " - " + benchLabel;
                }

                Add(lines, "NGP_DetailOrigin", origin);
            }

            Add(lines, "NGP_DetailIngredients", IngredientSummary(record));
            Add(lines, "NGP_DetailRepeat", RepeatSummary(record));

            if (record.repeatMode == BillRepeatModeDefOf.TargetCount && record.pauseWhenSatisfied)
            {
                Add(lines, "NGP_DetailPause", "NGP_DetailUnpauseAt".Translate(record.unpauseWhenYouHave));
            }

            if (record.qualityRange != QualityRange.All)
            {
                Add(lines, "NGP_DetailQuality", record.qualityRange.ToString());
            }

            if (record.hpRange != FloatRange.ZeroToOne)
            {
                Add(lines, "NGP_DetailHitPoints",
                    record.hpRange.min.ToStringByStyle(ToStringStyle.PercentZero) + " - "
                    + record.hpRange.max.ToStringByStyle(ToStringStyle.PercentZero));
            }

            if (record.allowedSkillRange.min != 0 || record.allowedSkillRange.max != 20)
            {
                Add(lines, "NGP_DetailSkill", record.allowedSkillRange.min + " - " + record.allowedSkillRange.max);
            }

            BillStoreModeDef storeMode = record.storeMode ?? BillStoreModeDefOf.BestStockpile;
            Add(lines, "NGP_DetailStoreMode", storeMode.LabelCap);

            if (record.slavesOnly)
            {
                Add(lines, "NGP_DetailPawnRestriction", "NGP_DetailSlavesOnly".Translate());
            }
            else if (record.mechsOnly)
            {
                Add(lines, "NGP_DetailPawnRestriction", "NGP_DetailMechsOnly".Translate());
            }
            else if (record.nonMechsOnly)
            {
                Add(lines, "NGP_DetailPawnRestriction", "NGP_DetailNonMechsOnly".Translate());
            }

            Add(lines, "NGP_DetailSearchRadius", record.ingredientSearchRadius >= Utils.UNLIMITED_BILL_RADIUS
                ? "Unlimited".TranslateSimple().ToString()
                : record.ingredientSearchRadius.ToString("F0"));

            if (record.suspended)
            {
                Add(lines, "NGP_DetailSuspended", "NGP_Yes".Translate());
            }

            if (record.includeEquipped)
            {
                Add(lines, "NGP_DetailIncludeEquipped", "NGP_Yes".Translate());
            }

            if (record.includeTainted)
            {
                Add(lines, "NGP_DetailIncludeTainted", "NGP_Yes".Translate());
            }

            if (record.limitToAllowedStuff)
            {
                Add(lines, "NGP_DetailLimitToAllowedStuff", "NGP_Yes".Translate());
            }

            return lines;
        }

        private static void Add(List<KeyValuePair<string, string>> lines, string labelKey, string value)
        {
            lines.Add(new KeyValuePair<string, string>(labelKey.Translate(), value));
        }

        public static float Height(List<KeyValuePair<string, string>> lines, float width)
        {
            if (lines == null || lines.Count == 0)
            {
                return 0f;
            }

            GameFont font = Text.Font;
            Text.Font = GameFont.Tiny;

            Columns(width, out float labelWidth, out float valueWidth);
            float height = 2f * Padding;
            for (int i = 0; i < lines.Count; i++)
            {
                height += Mathf.Max(Text.LineHeight, Text.CalcHeight(lines[i].Value, valueWidth));
            }

            Text.Font = font;
            return height;
        }

        public static void Draw(Rect rect, List<KeyValuePair<string, string>> lines)
        {
            if (lines == null || lines.Count == 0)
            {
                return;
            }

            GameFont font = Text.Font;
            TextAnchor anchor = Text.Anchor;
            Color color = GUI.color;

            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.UpperLeft;

            Columns(rect.width, out float labelWidth, out float valueWidth);
            float x = rect.x + Padding;
            float y = rect.y + Padding;

            for (int i = 0; i < lines.Count; i++)
            {
                float lineHeight = Mathf.Max(Text.LineHeight, Text.CalcHeight(lines[i].Value, valueWidth));

                GUI.color = new Color(0.7f, 0.7f, 0.7f);
                Widgets.Label(new Rect(x, y, labelWidth, lineHeight), lines[i].Key);

                GUI.color = color;
                Widgets.Label(new Rect(x + labelWidth + Padding, y, valueWidth, lineHeight), lines[i].Value);

                y += lineHeight;
            }

            Text.Font = font;
            Text.Anchor = anchor;
            GUI.color = color;
        }

        private static void Columns(float width, out float labelWidth, out float valueWidth)
        {
            labelWidth = Mathf.Floor((width - 2f * Padding) * LabelFraction);
            valueWidth = width - 2f * Padding - labelWidth - Padding;
        }

        public static string RepeatSummary(BillRecord record)
        {
            if (record.repeatMode == BillRepeatModeDefOf.Forever)
            {
                return "Forever".Translate();
            }

            if (record.repeatMode == BillRepeatModeDefOf.TargetCount)
            {
                return "NGP_DetailRepeatTarget".Translate(record.targetCount);
            }

            return "NGP_DetailRepeatCount".Translate(record.repeatCount);
        }

        public static string RepeatShort(BillRecord record)
        {
            if (record.repeatMode == BillRepeatModeDefOf.Forever)
            {
                return "Forever".Translate();
            }

            if (record.repeatMode == BillRepeatModeDefOf.TargetCount)
            {
                return "/" + record.targetCount;
            }

            return record.repeatCount + "x";
        }

        private static string IngredientSummary(BillRecord record)
        {
            ThingFilter filter = record.ingredientFilter;
            if (filter == null)
            {
                return "NGP_DetailIngredientsDefault".Translate();
            }

            List<string> names = new List<string>();
            foreach (ThingDef def in filter.AllowedThingDefs)
            {
                if (def != null)
                {
                    names.Add(def.LabelCap.ToString());
                }
            }

            if (names.Count == 0)
            {
                return "NGP_DetailIngredientsNone".Translate();
            }

            names.Sort(StringComparer.CurrentCulture);

            int total = record.recipe?.fixedIngredientFilter?.AllowedDefCount ?? 0;
            if (total > 0 && names.Count >= total)
            {
                return "NGP_DetailIngredientsAll".Translate(names.Count);
            }

            if (names.Count > MaxIngredientNames)
            {
                return "NGP_DetailIngredientsSome".Translate(
                    names.Count, total, names.GetRange(0, MaxIngredientNames).ToCommaList());
            }

            return names.ToCommaList();
        }
    }
}
