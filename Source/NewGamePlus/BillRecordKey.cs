using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using RimWorld;
using Verse;

namespace Boundir.NewGamePlus
{
    internal static class BillRecordKey
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        public static string For(BillRecord record)
        {
            if (record?.recipe == null)
            {
                return "~";
            }

            StringBuilder sb = new StringBuilder(256);
            sb.Append(record.recipe.defName);
            sb.Append('|').Append(record.suspended ? '1' : '0');
            sb.Append('|').Append(record.ingredientSearchRadius.ToString("F1", Inv));
            sb.Append('|').Append(record.allowedSkillRange.min).Append(':').Append(record.allowedSkillRange.max);
            sb.Append('|').Append(record.slavesOnly ? '1' : '0')
                          .Append(record.mechsOnly ? '1' : '0')
                          .Append(record.nonMechsOnly ? '1' : '0');
            sb.Append('|').Append(record.repeatMode?.defName ?? "");
            sb.Append('|').Append(record.repeatCount);
            sb.Append('|').Append(record.targetCount);
            sb.Append('|').Append(record.pauseWhenSatisfied ? '1' : '0');
            sb.Append('|').Append(record.unpauseWhenYouHave);
            sb.Append('|').Append(record.includeEquipped ? '1' : '0');
            sb.Append('|').Append(record.includeTainted ? '1' : '0');
            sb.Append('|').Append(record.hpRange.min.ToString("F3", Inv)).Append(':')
                          .Append(record.hpRange.max.ToString("F3", Inv));
            sb.Append('|').Append((int)record.qualityRange.min).Append(':').Append((int)record.qualityRange.max);
            sb.Append('|').Append(record.limitToAllowedStuff ? '1' : '0');
            sb.Append('|').Append(record.storeMode?.defName ?? "");
            sb.Append('|').Append(FilterKey(record.ingredientFilter));
            return sb.ToString();
        }

        public static string FilterKey(ThingFilter filter)
        {
            if (filter == null)
            {
                return "~";
            }

            StringBuilder sb = new StringBuilder(256);

            List<string> allowed = new List<string>();
            foreach (ThingDef def in filter.AllowedThingDefs)
            {
                if (def != null)
                {
                    allowed.Add(def.defName);
                }
            }

            allowed.Sort(StringComparer.Ordinal);
            sb.Append(allowed.Count).Append('[');
            for (int i = 0; i < allowed.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(',');
                }

                sb.Append(allowed[i]);
            }

            sb.Append(']');

            List<SpecialThingFilterDef> specials = DefDatabase<SpecialThingFilterDef>.AllDefsListForReading;
            List<string> disallowed = new List<string>();
            for (int i = 0; i < specials.Count; i++)
            {
                if (!filter.Allows(specials[i]))
                {
                    disallowed.Add(specials[i].defName);
                }
            }

            disallowed.Sort(StringComparer.Ordinal);
            sb.Append("|sf[").Append(string.Join(",", disallowed.ToArray())).Append(']');

            FloatRange hitPoints = filter.AllowedHitPointsPercents;
            sb.Append("|hp").Append(hitPoints.min.ToString("F3", Inv)).Append(':')
                            .Append(hitPoints.max.ToString("F3", Inv));

            FloatRange mentalBreak = filter.AllowedMentalBreakChance;
            sb.Append("|mb").Append(mentalBreak.min.ToString("F3", Inv)).Append(':')
                            .Append(mentalBreak.max.ToString("F3", Inv));

            QualityRange quality = filter.AllowedQualityLevels;
            sb.Append("|q").Append((int)quality.min).Append(':').Append((int)quality.max);

            return sb.ToString();
        }
    }
}
