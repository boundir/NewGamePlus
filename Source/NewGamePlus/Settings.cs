using UnityEngine;
using Verse;
using RimWorld;
using System.Reflection;

namespace Boundir.NewGamePlus
{
    public class Settings : ModSettings
    {
        public bool allowRoyalFavorRewards = false;
        public bool allowGoodwillRewards = false;
        public bool autoExpandHomeArea = false;
        public bool autoRebuild = true;
        public bool showZones = false;
        public FloatRange outfits = new FloatRange(.50f, 1f);
        public MedicalCareCategory medicalCareColonist = MedicalCareCategory.NoMeds;
        public MedicalCareCategory medicalCareColonyAnimal = MedicalCareCategory.NoMeds;
        public MedicalCareCategory medicalCarePrisoner = MedicalCareCategory.NoMeds;
        public MedicalCareCategory medicalCareSlave = MedicalCareCategory.NoMeds;
        public MedicalCareCategory medicalCareNeutralFaction = MedicalCareCategory.NoMeds;
        public MedicalCareCategory medicalCareNeutralAnimal = MedicalCareCategory.NoMeds;
        public MedicalCareCategory medicalCareHostileFaction = MedicalCareCategory.NoMeds;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref allowRoyalFavorRewards, "allowRoyalFavorRewards");
            Scribe_Values.Look(ref allowGoodwillRewards, "allowGoodwillRewards");
            Scribe_Values.Look(ref autoExpandHomeArea, "autoExpandHomeArea");
            Scribe_Values.Look(ref autoRebuild, "autoRebuild");
            Scribe_Values.Look(ref showZones, "showZones");
            Scribe_Values.Look(ref outfits, "outfits");
            Scribe_Values.Look(ref medicalCareColonist, "medicalCareColonist");
            Scribe_Values.Look(ref medicalCareColonyAnimal, "medicalCareColonyAnimal");
            Scribe_Values.Look(ref medicalCarePrisoner, "medicalCarePrisoner");
            Scribe_Values.Look(ref medicalCareSlave, "medicalCareSlave");
            Scribe_Values.Look(ref medicalCareNeutralFaction, "medicalCareNeutralFaction");
            Scribe_Values.Look(ref medicalCareNeutralAnimal, "medicalCareNeutralAnimal");
            Scribe_Values.Look(ref medicalCareHostileFaction, "medicalCareHostileFaction");
        }

        public void DoWindowContents(Rect rect)
        {
            float columnWidth = (rect.width - 2) / 2 - 2;
            float tabSpace = 34f;

            Listing_Standard list = new Listing_Standard
            {
                ColumnWidth = columnWidth
            };

            list.Begin(rect);
            list.Gap();

            list.DescriptiveCheckbox(label: "RoyalFavorReward", description: "RoyalFavorRewardDesc", ref allowRoyalFavorRewards, tabSpace: tabSpace);
            list.DescriptiveCheckbox(label: "GoodwillReward", description: "GoodwillRewardDesc", ref allowGoodwillRewards, tabSpace: tabSpace);
            list.DescriptiveCheckbox(label: "AutoExpandHomeArea", description: "AutoExpandHomeAreaDesc", ref autoExpandHomeArea, tabSpace: tabSpace);
            list.DescriptiveCheckbox(label: "AutoRebuild", description: "AutoRebuildDesc", ref autoRebuild, tabSpace: tabSpace);
            list.DescriptiveCheckbox(label: "ShowZone", description: "ShowZoneDesc", ref showZones, tabSpace: tabSpace);
            list.DescriptiveFloatRangeSliders(label: "Outfits", description: "OutfitsDesc", ref outfits, tabSpace: tabSpace);

            list.NewColumn();
            list.Indent();

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
        }
    }
}
