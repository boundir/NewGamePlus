using UnityEngine;
using Verse;
using RimWorld;

namespace Boundir.NewGamePlus
{
    public class Settings : ModSettings
    {
        private static Vector2 scrollPositionVector;

        public bool allowRoyalFavorRewards;
        public bool allowGoodwillRewards;
        public bool autoExpandHomeArea;
        public bool autoRebuild;
        public bool showZones;
        public bool workPriorities;

        public HostilityResponseMode threatResponseMode;

        public FloatRange outfitsHitpoints;

        public MedicalCareCategory medicalCareColonist;
        public MedicalCareCategory medicalCareColonyAnimal;
        public MedicalCareCategory medicalCarePrisoner;
        public MedicalCareCategory medicalCareSlave;
        public MedicalCareCategory medicalCareFriendlyFaction;
        public MedicalCareCategory medicalCareNeutralFaction;
        public MedicalCareCategory medicalCareHostileFaction;
        public MedicalCareCategory medicalCareNoFaction;
        public MedicalCareCategory medicalCareEntities;
        public MedicalCareCategory medicalCareGhouls;
        public MedicalCareCategory medicalCareWildlife;

        public bool dropOnFloor;
        public float billSearchRadius;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(value: ref allowRoyalFavorRewards, label: "allowRoyalFavorRewards", defaultValue: true);
            Scribe_Values.Look(value: ref allowGoodwillRewards, label: "allowGoodwillRewards", defaultValue: true);
            Scribe_Values.Look(value: ref autoExpandHomeArea, label: "autoExpandHomeArea", defaultValue: true);
            Scribe_Values.Look(value: ref autoRebuild, label: "autoRebuild", defaultValue: false);
            Scribe_Values.Look(value: ref showZones, label: "showZones", defaultValue: true);
            Scribe_Values.Look(value: ref workPriorities, label: "workPriorities", defaultValue: false);

            // Medical
            Scribe_Values.Look(value: ref medicalCareColonist, label: "MedGroupColonists", defaultValue: MedicalCareCategory.Best);
            Scribe_Values.Look(value: ref medicalCarePrisoner, label: "MedGroupPrisoners", defaultValue: MedicalCareCategory.HerbalOrWorse);
            Scribe_Values.Look(value: ref medicalCareSlave, label: "MedGroupSlaves", defaultValue: MedicalCareCategory.HerbalOrWorse);
            Scribe_Values.Look(value: ref medicalCareGhouls, label: "MedGroupGhouls", defaultValue: MedicalCareCategory.NoMeds);
            Scribe_Values.Look(value: ref medicalCareColonyAnimal, label: "MedGroupTamedAnimals", defaultValue: MedicalCareCategory.HerbalOrWorse);

            Scribe_Values.Look(value: ref medicalCareFriendlyFaction, label: "MedGroupFriendlyFaction", defaultValue: MedicalCareCategory.HerbalOrWorse);
            Scribe_Values.Look(value: ref medicalCareNeutralFaction, label: "MedGroupNeutralFaction", defaultValue: MedicalCareCategory.HerbalOrWorse);
            Scribe_Values.Look(value: ref medicalCareHostileFaction, label: "MedGroupHostileFaction", defaultValue: MedicalCareCategory.HerbalOrWorse);

            Scribe_Values.Look(value: ref medicalCareNoFaction, label: "MedGroupNoFaction", defaultValue: MedicalCareCategory.HerbalOrWorse);
            Scribe_Values.Look(value: ref medicalCareEntities, label: "medicalCareEntities", defaultValue: MedicalCareCategory.NoMeds);
            Scribe_Values.Look(value: ref medicalCareWildlife, label: "medicalCareWildlife", defaultValue: MedicalCareCategory.HerbalOrWorse);

            // Production
            Scribe_Values.Look(value: ref dropOnFloor, label: "dropOnFloor", defaultValue: false);
            Scribe_Values.Look(value: ref billSearchRadius, label: "billSearchRadius", defaultValue: 999f);
            Scribe_Values.Look(value: ref outfitsHitpoints, label: "outfitsHitpoints", defaultValue: FloatRange.ZeroToOne);
        }

        public void DoWindowContents(Rect rect)
        {
            float columnWidth = rect.width / 2 - 40f;
            float tabSpace = 34f;
            float windowHeight = 950f;

            Listing_Standard list = new Listing_Standard
            {
                ColumnWidth = columnWidth
            };


            Rect listRect = new Rect(x: 0, y: 0, width: rect.width - 20f, height: windowHeight);
            Widgets.BeginScrollView(outRect: rect, scrollPosition: ref scrollPositionVector, viewRect: listRect, showScrollbars: true);
            rect.height = windowHeight;
            list.Begin(rect);

            list.DescriptiveSection(label: "DefaultGameplaySettings", description: "DefaultGameplaySettingsDesc");
            list.DescriptiveCheckbox(label: "AutoExpandHomeArea", description: "AutoExpandHomeAreaDesc", value: ref autoExpandHomeArea, tabSpace: tabSpace);
            list.DescriptiveCheckbox(label: "AutoRebuild", description: "AutoRebuildDesc", value: ref autoRebuild, tabSpace: tabSpace);
            list.DescriptiveCheckbox(label: "ShowZone", description: "ShowZoneDesc", value: ref showZones, tabSpace: tabSpace);
            list.DescriptiveCheckbox(label: "RoyalFavorReward", description: "RoyalFavorRewardDesc", value: ref allowRoyalFavorRewards, tabSpace: tabSpace);
            list.DescriptiveCheckbox(label: "GoodwillReward", description: "GoodwillRewardDesc", value: ref allowGoodwillRewards, tabSpace: tabSpace);
            list.DescriptiveCheckbox(label: "ManualWorkPriorities", description: "ManualWorkPrioritiesDesc", value: ref workPriorities);

            list.NewColumn();
            list.Indent();

            list.DescriptiveSection(label: "DefaultProductionSettings", description: "DefaultProductionSettingsDesc");
            list.DescriptiveCheckbox(label: "DropOnFloor", description: "DropOnFloorDesc", value: ref dropOnFloor, tabSpace: tabSpace);
            list.DescriptiveBillSearchRadiusSlider(label: "SearchRadius", description: "SearchRadiusDesc", value: ref billSearchRadius);
            list.DescriptiveFloatRangeSliders(label: "Outfits", description: "OutfitsDesc", value: ref outfitsHitpoints, tabSpace: tabSpace);

            list.GapLine();

            list.DescriptiveSection(label: "DefaultMedicineSettings", description: "DefaultMedicineSettingsDesc", tabSpace: 0f);

            list.MedicalCareSelector(label: "MedGroupColonists", value: ref medicalCareColonist);
            list.MedicalCareSelector(label: "MedGroupPrisoners", value: ref medicalCarePrisoner);

            if (ModsConfig.IdeologyActive)
            {
                list.MedicalCareSelector(label: "MedGroupSlaves", value: ref medicalCareSlave);
            }

            if (ModsConfig.AnomalyActive)
            {
                list.MedicalCareSelector(label: "MedGroupGhouls", value: ref medicalCareGhouls);
            }

            list.MedicalCareSelector(label: "MedGroupTamedAnimals", value: ref medicalCareColonyAnimal);

            list.Gap();

            list.MedicalCareSelector(label: "MedGroupFriendlyFaction", value: ref medicalCareFriendlyFaction);
            list.MedicalCareSelector(label: "MedGroupNeutralFaction", value: ref medicalCareNeutralFaction);
            list.MedicalCareSelector(label: "MedGroupHostileFaction", value: ref medicalCareHostileFaction);

            list.Gap();

            list.MedicalCareSelector(label: "MedGroupNoFaction", value: ref medicalCareNoFaction);
            list.MedicalCareSelector(label: "MedGroupWildlife", value: ref medicalCareWildlife);

            if (ModsConfig.AnomalyActive)
            {
                list.MedicalCareSelector(label: "MedGroupEntities", value: ref medicalCareEntities);
            }

            list.End();

            Widgets.EndScrollView();
        }
    }
}
