using RimWorld;
using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Boundir.NewGamePlus
{
    static class Utils
    {
        public const float DEFAULT_GAP = 15f;
        public const float DEFAULT_TAB_SPACE = 34f;

        private static bool hostilityResponsePainting;

        public static void DescriptiveCheckbox(this Listing_Standard list, string label, string description, ref bool value, float tabSpace = DEFAULT_TAB_SPACE, float gap = DEFAULT_GAP)
        {
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            list.CheckboxLabeled(label: label.Translate(), checkOn: ref value);

            Text.Font = GameFont.Tiny;
            list.ColumnWidth -= tabSpace;
            GUI.color = Color.gray;
            list.Label(label: description.Translate());
            Text.Font = GameFont.Small;
            list.ColumnWidth += tabSpace;
            GUI.color = Color.white;

            list.Gap(gapHeight: gap);
        }

        public static void DescriptiveFloatRangeSliders(this Listing_Standard list, string label, string description, ref FloatRange value, float tabSpace = DEFAULT_TAB_SPACE)
        {
            TextAnchor anchor = Text.Anchor;

            Rect rect = list.GetRect(height: Text.LineHeight + list.verticalSpacing);

            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect: rect, label: label.Translate());

            Text.Anchor = TextAnchor.MiddleRight;
            string labelKey = value.min.ToStringByStyle(ToStringStyle.PercentZero) + " - " + value.max.ToStringByStyle(ToStringStyle.PercentZero);
            Widgets.Label(rect: rect, label: labelKey);

            Text.Anchor = anchor;

            Text.Font = GameFont.Tiny;
            list.ColumnWidth -= tabSpace;
            GUI.color = Color.gray;
            list.Label(label: description.Translate());
            Text.Font = GameFont.Small;
            list.ColumnWidth += tabSpace;
            GUI.color = Color.white;

            Text.Anchor = TextAnchor.MiddleRight;
            int id = list.CurHeight.GetHashCode();
            Widgets.FloatRange(rect: list.GetRect(height: Text.LineHeight), id: id, range: ref value, min: 0f, max: 1f, labelKey: "", valueStyle: ToStringStyle.PercentZero);

            Text.Anchor = anchor;

            list.Gap(gapHeight: DEFAULT_GAP);
        }

        public static void DescriptiveSection(this Listing_Standard list, string label, string description, float tabSpace = DEFAULT_TAB_SPACE, float gap = DEFAULT_GAP)
        {
            Text.Font = GameFont.Medium;
            list.Label(label.Translate());

            Text.Font = GameFont.Tiny;
            list.ColumnWidth -= tabSpace;
            GUI.color = Color.gray;
            list.Label(label: description.Translate());
            Text.Font = GameFont.Small;
            list.ColumnWidth += tabSpace;
            GUI.color = Color.white;

            list.Gap(gapHeight: gap);
        }

        public static void MedicalCareSelector(this Listing_Standard list, string label, ref MedicalCareCategory value)
        {
            Rect rectBase = list.GetRect(height: Text.LineHeight + list.verticalSpacing);
            Rect rect2 = new Rect(rectBase.x, list.CurHeight, rectBase.width, 28f);
            Rect rect3 = new Rect(rectBase.x, list.CurHeight, 230f, 28f);
            Rect rect4 = new Rect(rectBase.x + 230f, list.CurHeight, 140f, 28f);

            if (Mouse.IsOver(rect2))
            {
                Widgets.DrawLightHighlight(rect2);
            }

            TooltipHandler.TipRegionByKey(rect2, label + "Desc");
            Widgets.LabelFit(rect3, label.Translate());

            MedicalCareUtility.MedicalCareSetter(rect: rect4, medCare: ref value);

            list.Gap(gapHeight: DEFAULT_GAP);
        }

        public static void HostilityResponseSelector(this Listing_Standard list, ref HostilityResponseMode threatResponseMode, string label, string description, float tabSpace = DEFAULT_TAB_SPACE)
        {
            Text.Font = GameFont.Small;
            GUI.color = Color.white;

            float hostilitySelectorWidth = list.ColumnWidth - (Enum.GetValues(typeof(HostilityResponseMode)).Length * 24f);

            Rect rectBase = list.GetRect(height: Text.LineHeight + list.verticalSpacing);
            Rect rect = new Rect(x: hostilitySelectorWidth, y: list.CurHeight, width: 24f, height: 24f);
            Rect rectLabel = new Rect(rectBase.x, list.CurHeight, width: hostilitySelectorWidth, height: Text.LineHeight + list.verticalSpacing);
            Widgets.LabelFit(rectLabel, label.Translate() + ": " + threatResponseMode);

            foreach (HostilityResponseMode hostilityResponseMode in Enum.GetValues(typeof(HostilityResponseMode)))
            {
                Widgets.DrawHighlightIfMouseover(rect);
                MouseoverSounds.DoRegion(rect);
                GUI.DrawTexture(rect, hostilityResponseMode.GetIcon());
                Widgets.DraggableResult draggableResult = Widgets.ButtonInvisibleDraggable(rect);

                if (draggableResult == Widgets.DraggableResult.Dragged)
                {
                    hostilityResponsePainting = true;
                }

                bool draggableResultPressed;
                if (draggableResult != Widgets.DraggableResult.Pressed)
                {
                    draggableResultPressed = draggableResult == Widgets.DraggableResult.DraggedThenPressed;
                }
                else
                {
                    draggableResultPressed = true;
                }


                if ((hostilityResponsePainting && Mouse.IsOver(rect) && threatResponseMode != hostilityResponseMode) || draggableResultPressed)
                {
                    threatResponseMode = hostilityResponseMode;
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                }

                if (threatResponseMode == hostilityResponseMode)
                {
                    Widgets.DrawBox(rect, 2);
                }

                if (Mouse.IsOver(rect))
                {
                    TooltipHandler.TipRegion(rect, hostilityResponseMode.GetLabel());
                }

                rect.x += rect.width;
            }

            list.Gap(gapHeight: 30f);
            Text.Font = GameFont.Tiny;
            list.ColumnWidth -= tabSpace;
            GUI.color = Color.gray;
            list.Label(label: description.Translate());
            Text.Font = GameFont.Small;
            list.ColumnWidth += tabSpace;
            GUI.color = Color.white;

            list.Gap(gapHeight: DEFAULT_GAP);

        }

        public static void DescriptiveBillSearchRadiusSlider(this Listing_Standard list, string label, string description, ref float value, float min = 0f, float max = 100f, float tabSpace = DEFAULT_TAB_SPACE)
        {
            TextAnchor anchor = Text.Anchor;

            Rect rect = list.GetRect(height: Text.LineHeight + list.verticalSpacing);

            Text.Font = GameFont.Small;
            GUI.color = Color.white;

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect: rect, label: label.Translate());

            Text.Anchor = TextAnchor.MiddleRight;
            string text = ((value == 999f) ? "Unlimited".TranslateSimple() : value.ToString("F0"));
            Widgets.Label(rect, "IngredientSearchRadius".Translate() + ": " + text);

            Text.Anchor = anchor;

            Text.Font = GameFont.Tiny;
            list.ColumnWidth -= tabSpace;
            GUI.color = Color.gray;
            list.Label(label: description.Translate());
            Text.Font = GameFont.Small;
            list.ColumnWidth += tabSpace;
            GUI.color = Color.white;

            value = list.Slider(val: (value > 100f) ? 100f : value, min: min, max: max);

            if (value >= 100f)
            {
                value = 999f;
            }

            list.Gap(gapHeight: DEFAULT_GAP);
        }
    }
}
