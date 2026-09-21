using UnityEngine;
using Verse;

namespace Boundir.NewGamePlus
{
    internal static class BillsUI
    {
        public const float SectionHeight = 25f;

        public const float GroupHeight = 28f;
        public const float RowHeight = 32f;
        public const float IconSize = 24f;

        public const float Stride = 28f;

        public const float DetailGap = 4f;

        public static string GroupKey(ColonyBillCapture capture, BenchBillPreset group)
        {
            string owner = capture == null ? "lib" : capture.colonyId.ToString();
            return owner + "/" + (group.benchDef?.defName ?? "?");
        }

        public static bool Expander(Rect rect, bool expanded, string tooltipKey = null)
        {
            return Widgets.ButtonImage(
                rect,
                expanded ? TexButton.Collapse : TexButton.Reveal,
                Color.white,
                doMouseoverSound: true,
                tooltip: tooltipKey == null ? null : tooltipKey.Translate().ToString());
        }

        public static void RecipeIcon(Rect rect, BillRecord record)
        {
            if (record.recipe?.UIIcon != null)
            {
                GUI.DrawTexture(rect, record.recipe.UIIcon);
            }
        }
    }
}
