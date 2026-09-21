using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Boundir.NewGamePlus
{
    public static class BillsTabImport
    {
        private static readonly int ButtonSlot = ModLister.GetActiveModWithIdentifier("falconne.bwm", ignorePostfix: true) != null ? 3 : 1;

        private static readonly Func<ITab_Bills, Building_WorkTable> SelTableGetter;
        private static readonly AccessTools.FieldRef<Vector2> WinSizeRef;
        private static readonly AccessTools.FieldRef<float> PasteXRef;
        private static readonly AccessTools.FieldRef<float> PasteYRef;
        private static readonly AccessTools.FieldRef<float> PasteSizeRef;
        private static readonly bool resolved;

        static BillsTabImport()
        {
            try
            {
                SelTableGetter = AccessTools.MethodDelegate<Func<ITab_Bills, Building_WorkTable>>(
                    AccessTools.PropertyGetter(typeof(ITab_Bills), "SelTable"));

                WinSizeRef = AccessTools.StaticFieldRefAccess<Vector2>(AccessTools.Field(typeof(ITab_Bills), "WinSize"));
                PasteXRef = AccessTools.StaticFieldRefAccess<float>(AccessTools.Field(typeof(ITab_Bills), "PasteX"));
                PasteYRef = AccessTools.StaticFieldRefAccess<float>(AccessTools.Field(typeof(ITab_Bills), "PasteY"));
                PasteSizeRef = AccessTools.StaticFieldRefAccess<float>(AccessTools.Field(typeof(ITab_Bills), "PasteSize"));

                resolved = true;
            }
            catch (Exception e)
            {
                Log.Warning("New Game Plus: could not bind ITab_Bills internals (" + e.Message
                    + "); the bill import button is disabled. RimWorld has probably changed.");
            }
        }

        public static void DrawImportButton(ITab_Bills __instance)
        {
            if (!resolved)
            {
                return;
            }

            Building_WorkTable table = SelTableGetter(__instance);
            if (table == null)
            {
                return;
            }

            BillAvailability availability = BillPresets.Availability(table);
            if (availability.matching == 0)
            {
                return;
            }

            Vector2 winSize = WinSizeRef();
            float pasteX = PasteXRef();
            float pasteY = PasteYRef();
            float pasteSize = PasteSizeRef();

            Rect rect = new Rect(winSize.x - pasteX - ButtonSlot * (pasteSize + 4f), pasteY, pasteSize, pasteSize);

            if (availability.importable == 0 || table.billStack.Count >= BillStack.MaxCount)
            {
                GUI.color = Color.gray;
                Widgets.DrawTextureFitted(rect, TexButton.Drop, 1f);
                GUI.color = Color.white;
                if (Mouse.IsOver(rect))
                {
                    TooltipHandler.TipRegion(rect, "NGP_ImportBillsUnavailable".Translate());
                }

                return;
            }

            if (Widgets.ButtonImageFitted(rect, TexButton.Drop, Color.white))
            {
                Find.WindowStack.Add(new Dialog_ImportBills(table));
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
            }

            if (Mouse.IsOver(rect))
            {
                TooltipHandler.TipRegion(rect, "NGP_ImportBillsTip".Translate(availability.importable));
            }
        }
    }
}
