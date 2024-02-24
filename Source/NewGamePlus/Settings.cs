using UnityEngine;
using Verse;
using RimWorld;
using System.Collections.Generic;

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
        public MedicalCareCategory medicalCareNeutralFaction;
        public MedicalCareCategory medicalCareNeutralAnimal;
        public MedicalCareCategory medicalCareHostileFaction;

        public bool dropOnFloor;
        public float billSearchRadius;

        public List<Outfit> listOutfits;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(value: ref allowRoyalFavorRewards, label: "allowRoyalFavorRewards", defaultValue: true);
            Scribe_Values.Look(value: ref allowGoodwillRewards, label: "allowGoodwillRewards", defaultValue: true);
            Scribe_Values.Look(value: ref autoExpandHomeArea, label: "autoExpandHomeArea", defaultValue: true);
            Scribe_Values.Look(value: ref autoRebuild, label: "autoRebuild", defaultValue: false);
            Scribe_Values.Look(value: ref showZones, label: "showZones", defaultValue: true);
            Scribe_Values.Look(value: ref workPriorities, label: "workPriorities", defaultValue: false);
            Scribe_Values.Look(value: ref outfitsHitpoints, label: "outfitsHitpoints", defaultValue: new FloatRange(min: 0f, max: 100f));
            Scribe_Values.Look(value: ref medicalCareColonist, label: "medicalCareColonist", defaultValue: MedicalCareCategory.Best);
            Scribe_Values.Look(value: ref medicalCareColonyAnimal, label: "medicalCareColonyAnimal", defaultValue: MedicalCareCategory.HerbalOrWorse);
            Scribe_Values.Look(value: ref medicalCarePrisoner, label: "medicalCarePrisoner", defaultValue: MedicalCareCategory.HerbalOrWorse);
            Scribe_Values.Look(value: ref medicalCareSlave, label: "medicalCareSlave", defaultValue: MedicalCareCategory.HerbalOrWorse);
            Scribe_Values.Look(value: ref medicalCareNeutralFaction, label: "medicalCareNeutralFaction", defaultValue: MedicalCareCategory.HerbalOrWorse);
            Scribe_Values.Look(value: ref medicalCareNeutralAnimal, label: "medicalCareNeutralAnimal", defaultValue: MedicalCareCategory.HerbalOrWorse);
            Scribe_Values.Look(value: ref medicalCareHostileFaction, label: "medicalCareHostileFaction", defaultValue: MedicalCareCategory.HerbalOrWorse);
            Scribe_Values.Look(value: ref dropOnFloor, label: "dropOnFloor", defaultValue: false);
            Scribe_Values.Look(value: ref billSearchRadius, label: "billSearchRadius", defaultValue: 999f);
        }

        public void DoWindowContents(Rect rect)
        {
            float columnWidth = rect.width / 2 - 40f;
            float tabSpace = 34f;

            Listing_Standard list = new Listing_Standard
            {
                ColumnWidth = columnWidth
            };

            Rect listRect = new Rect(x: 0, y: 0, width: rect.width - 20f, height: 700f);
            Widgets.BeginScrollView(outRect: rect, scrollPosition: ref scrollPositionVector, viewRect: listRect, showScrollbars: true);
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
            list.DescriptiveBillSearchRadiusSlider(label: "SearchRadius", description: "SearchRadiusDesc", value: ref billSearchRadius, min: 3f);
            list.DescriptiveFloatRangeSliders(label: "Outfits", description: "OutfitsDesc", value: ref outfitsHitpoints, tabSpace: tabSpace);

            list.GapLine();

            list.DescriptiveSection(label: "DefaultMedicineSettings", description: "DefaultMedicineSettingsDesc");
            list.MedicalCareSelector(label: "MedGroupColonist", value: ref medicalCareColonist);
            list.MedicalCareSelector(label: "MedGroupColonyAnimal", value: ref medicalCareColonyAnimal);
            list.MedicalCareSelector(label: "MedGroupImprisonedColonist", value: ref medicalCarePrisoner);

            if (ModsConfig.IdeologyActive)
            {
                list.MedicalCareSelector(label: "MedGroupEnslavedColonist", value: ref medicalCareSlave);
            }

            list.MedicalCareSelector(label: "MedGroupNeutralFaction", value: ref medicalCareNeutralFaction);
            list.MedicalCareSelector(label: "MedGroupNeutralAnimal", value: ref medicalCareNeutralAnimal);
            list.MedicalCareSelector(label: "MedGroupHostileFaction", value: ref medicalCareHostileFaction);

            list.End();

            Widgets.EndScrollView();
        }
    }
}
