using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Boundir.NewGamePlus
{
    public class BillRecord : IExposable, IRenameable
    {
        public RecipeDef recipe;
        public bool suspended;
        public float ingredientSearchRadius = 999f;
        public IntRange allowedSkillRange = new IntRange(0, 20);
        public bool slavesOnly;
        public bool mechsOnly;
        public bool nonMechsOnly;
        public ThingFilter ingredientFilter;

        public BillRepeatModeDef repeatMode;
        public int repeatCount = 1;
        public int targetCount = 10;
        public bool pauseWhenSatisfied;
        public int unpauseWhenYouHave = 5;
        public bool includeEquipped;
        public bool includeTainted;
        public FloatRange hpRange = FloatRange.ZeroToOne;
        public QualityRange qualityRange = QualityRange.All;
        public bool limitToAllowedStuff;
        public BillStoreModeDef storeMode;
        public string customName;

        public bool labelPinned;

        public string BaseLabel => recipe != null ? recipe.label.CapitalizeFirst() : "?";

        public string RenamableLabel
        {
            get { return customName.NullOrEmpty() ? BaseLabel : customName; }
            set { customName = (value.NullOrEmpty() || value == BaseLabel) ? null : value; }
        }

        public string InspectLabel => RenamableLabel;

        public string Label => RenamableLabel;

        public BillRecord Clone()
        {
            BillRecord copy = (BillRecord)MemberwiseClone();
            if (ingredientFilter != null)
            {
                copy.ingredientFilter = new ThingFilter();
                copy.ingredientFilter.CopyAllowancesFrom(ingredientFilter);
            }

            return copy;
        }

        public static BillRecord FromBill(Bill_Production bill)
        {
            BillRecord record = new BillRecord
            {
                recipe = bill.recipe,
                suspended = bill.suspended,
                ingredientSearchRadius = bill.ingredientSearchRadius,
                allowedSkillRange = bill.allowedSkillRange,
                slavesOnly = bill.SlavesOnly,
                mechsOnly = bill.MechsOnly,
                nonMechsOnly = bill.NonMechsOnly,
                repeatMode = bill.repeatMode,
                repeatCount = bill.repeatCount,
                targetCount = bill.targetCount,
                pauseWhenSatisfied = bill.pauseWhenSatisfied,
                unpauseWhenYouHave = bill.unpauseWhenYouHave,
                includeEquipped = bill.includeEquipped,
                includeTainted = bill.includeTainted,
                hpRange = bill.hpRange,
                qualityRange = bill.qualityRange,
                limitToAllowedStuff = bill.limitToAllowedStuff
            };

            BillStoreModeDef mode = bill.GetStoreMode();
            record.storeMode = mode == BillStoreModeDefOf.SpecificStockpile ? BillStoreModeDefOf.BestStockpile : mode;

            if (bill.RenamableLabel != bill.BaseLabel)
            {
                record.customName = bill.RenamableLabel;
            }

            record.ingredientFilter = new ThingFilter();
            record.ingredientFilter.CopyAllowancesFrom(bill.ingredientFilter);

            if (bill.recipe.fixedIngredientFilter != null)
            {
                foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
                {
                    if (!bill.recipe.fixedIngredientFilter.Allows(def))
                    {
                        record.ingredientFilter.SetAllow(def, allow: false);
                    }
                }
            }

            return record;
        }

        public Bill_Production MakeBill()
        {
            Bill bill = BillUtility.MakeNewBill(recipe, null);
            if (!(bill is Bill_Production production) || bill is Bill_Autonomous)
            {
                return null;
            }

            production.suspended = suspended;
            production.ingredientSearchRadius = ingredientSearchRadius;
            production.allowedSkillRange = allowedSkillRange;
            if (slavesOnly)
            {
                production.SetAnySlaveRestriction();
            }
            else if (mechsOnly)
            {
                production.SetAnyMechRestriction();
            }
            else if (nonMechsOnly)
            {
                production.SetAnyNonMechRestriction();
            }

            if (ingredientFilter != null)
            {
                production.ingredientFilter.CopyAllowancesFrom(ingredientFilter);
            }

            production.repeatMode = repeatMode ?? BillRepeatModeDefOf.RepeatCount;
            production.repeatCount = repeatCount;
            production.targetCount = targetCount;
            production.pauseWhenSatisfied = pauseWhenSatisfied;
            production.unpauseWhenYouHave = unpauseWhenYouHave;
            production.includeEquipped = includeEquipped;
            production.includeTainted = includeTainted;
            production.hpRange = hpRange;
            production.qualityRange = qualityRange;
            production.limitToAllowedStuff = limitToAllowedStuff;
            production.SetStoreMode(storeMode ?? BillStoreModeDefOf.BestStockpile);
            if (!customName.NullOrEmpty())
            {
                production.RenamableLabel = customName;
            }

            return production;
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref recipe, "recipe");
            Scribe_Values.Look(ref suspended, "suspended", defaultValue: false);
            Scribe_Values.Look(ref ingredientSearchRadius, "ingredientSearchRadius", 999f);
            Scribe_Values.Look(ref allowedSkillRange, "allowedSkillRange", new IntRange(0, 20));
            Scribe_Values.Look(ref slavesOnly, "slavesOnly", defaultValue: false);
            Scribe_Values.Look(ref mechsOnly, "mechsOnly", defaultValue: false);
            Scribe_Values.Look(ref nonMechsOnly, "nonMechsOnly", defaultValue: false);
            Scribe_Deep.Look(ref ingredientFilter, "ingredientFilter");
            Scribe_Defs.Look(ref repeatMode, "repeatMode");
            Scribe_Values.Look(ref repeatCount, "repeatCount", 1);
            Scribe_Values.Look(ref targetCount, "targetCount", 10);
            Scribe_Values.Look(ref pauseWhenSatisfied, "pauseWhenSatisfied", defaultValue: false);
            Scribe_Values.Look(ref unpauseWhenYouHave, "unpauseWhenYouHave", 5);
            Scribe_Values.Look(ref includeEquipped, "includeEquipped", defaultValue: false);
            Scribe_Values.Look(ref includeTainted, "includeTainted", defaultValue: false);
            Scribe_Values.Look(ref hpRange, "hpRange", FloatRange.ZeroToOne);
            Scribe_Values.Look(ref qualityRange, "qualityRange", QualityRange.All);
            Scribe_Values.Look(ref limitToAllowedStuff, "limitToAllowedStuff", defaultValue: false);
            Scribe_Defs.Look(ref storeMode, "storeMode");
            Scribe_Values.Look(ref customName, "customName");
            Scribe_Values.Look(ref labelPinned, "labelPinned", defaultValue: false);
        }
    }

    /// <summary>
    /// Serves both as a library group and as one bench inside a <see cref="ColonyBillCapture"/>.
    /// </summary>
    public class BenchBillPreset : IExposable
    {
        public ThingDef benchDef;
        public List<BillRecord> bills = new List<BillRecord>();

        /// <summary>
        /// True when a record with these exact settings is already stored here. Used by
        /// capture and by "copy to library" to avoid storing the same bill twice.
        /// </summary>
        public bool Contains(string settingsKey)
        {
            for (int i = 0; i < bills.Count; i++)
            {
                if (BillRecordKey.For(bills[i]) == settingsKey)
                {
                    return true;
                }
            }

            return false;
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref benchDef, "benchDef");
            Scribe_Collections.Look(ref bills, "records", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (bills == null)
                {
                    bills = new List<BillRecord>();
                }

                bills.RemoveAll(b => b?.recipe == null);
            }
        }
    }
}
