using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Boundir.NewGamePlus
{
    static class Utils
    {
        public const float DEFAULT_GAP = 15f;
        public const float DEFAULT_TAB_SPACE = 34f;

        public static void DescriptiveCheckbox(this Listing_Standard list, string label, string description, ref bool value, float tabSpace = DEFAULT_TAB_SPACE)
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

            list.Gap(gapHeight: DEFAULT_GAP);
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

        public static void DescriptiveSection(this Listing_Standard list, string label, string description, float tabSpace = DEFAULT_TAB_SPACE)
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

            list.Gap(gapHeight: DEFAULT_GAP);
        }

        public static void MedicalCareSelector(this Listing_Standard list, string label, ref MedicalCareCategory value)
        {
            Rect rect = list.GetRect(height: Text.LineHeight + list.verticalSpacing);

            Rect rectLabel = rect.LeftPartPixels(width: 200f);
            Rect rectIcon = new Rect(x: rect.x + 225f, y: rect.y, width: 140f, height: rect.height);

            Widgets.LabelFit(rect: rectLabel, label: label.Translate());
            MedicalCareUtility.MedicalCareSetter(rect: rectIcon, medCare: ref value);

            list.Gap(gapHeight: DEFAULT_GAP);
        }
    }
}
