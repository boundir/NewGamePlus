using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace Boundir.NewGamePlus
{
    public static class PoliciesTabUI
    {
        public static void Draw(Rect rect)
        {
            Listing_Standard list = new Listing_Standard();
            list.Begin(rect);

            Text.Font = GameFont.Tiny;
            GUI.color = Color.gray;
            list.Label("NGP_PoliciesTabDesc".Translate());
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            list.Gap();

            PersistentData data = PersistentStore.Data;

            DrawCategory(list, "NGP_ApparelPolicies", data.apparel.Active, data.apparel.policies.Count,
                openEditor: delegate
                {
                    PolicyTransfer.SeedApparelIfEmpty();
                    Find.WindowStack.Add(new Dialog_ManageStoredApparelPolicies(data.apparel.Default()));
                },
                capture: () => PolicyTransfer.CaptureApparel(Current.Game),
                reset: data.apparel.Clear);

            DrawCategory(list, "NGP_FoodPolicies", data.food.Active, data.food.policies.Count,
                openEditor: delegate
                {
                    PolicyTransfer.SeedFoodIfEmpty();
                    Find.WindowStack.Add(new Dialog_ManageStoredFoodPolicies(data.food.Default()));
                },
                capture: () => PolicyTransfer.CaptureFood(Current.Game),
                reset: data.food.Clear);

            DrawCategory(list, "NGP_DrugPolicies", data.drugs.Active, data.drugs.policies.Count,
                openEditor: delegate
                {
                    PolicyTransfer.SeedDrugsIfEmpty();
                    Find.WindowStack.Add(new Dialog_ManageStoredDrugPolicies(data.drugs.Default()));
                },
                capture: () => PolicyTransfer.CaptureDrugs(Current.Game),
                reset: data.drugs.Clear);

            DrawCategory(list, "NGP_ReadingPolicies", data.reading.Active, data.reading.policies.Count,
                openEditor: delegate
                {
                    PolicyTransfer.SeedReadingIfEmpty();
                    Find.WindowStack.Add(new Dialog_ManageStoredReadingPolicies(data.reading.Default()));
                },
                capture: () => PolicyTransfer.CaptureReading(Current.Game),
                reset: data.reading.Clear);

            list.End();
        }

        private static void DrawCategory(Listing_Standard list, string labelKey, bool active, int count, Action openEditor, Action capture, Action reset)
        {
            Rect header = list.GetRect(26f);
            Widgets.Label(header, labelKey.Translate());

            Text.Font = GameFont.Tiny;
            GUI.color = Color.gray;
            Text.Anchor = TextAnchor.UpperRight;
            Widgets.Label(header, active ? "NGP_StoredCount".Translate(count).ToString() : "NGP_UsingVanilla".Translate().ToString());
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            GUI.color = Color.white;

            Rect row = list.GetRect(30f);
            float width = (row.width - 20f) / 3f;
            Rect editRect = new Rect(row.x, row.y, width, 30f);
            Rect captureRect = new Rect(editRect.xMax + 10f, row.y, width, 30f);
            Rect resetRect = new Rect(captureRect.xMax + 10f, row.y, width, 30f);

            if (Widgets.ButtonText(editRect, "NGP_EditPolicies".Translate()))
            {
                openEditor();
            }

            if (Utils.DisableableButton(captureRect, "NGP_CaptureFromGame".Translate(), Current.Game != null))
            {
                if (active)
                {
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("NGP_ConfirmCaptureOverwrite".Translate(), delegate
                    {
                        capture();
                        PersistentStore.Save();
                    }));
                }
                else
                {
                    capture();
                    PersistentStore.Save();
                }
            }

            if (Utils.DisableableButton(resetRect, "NGP_ResetToVanilla".Translate(), active))
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("NGP_ConfirmResetPolicies".Translate(), delegate
                {
                    reset();
                    PersistentStore.Save();
                }));
            }

            list.GapLine();
        }
    }
}
