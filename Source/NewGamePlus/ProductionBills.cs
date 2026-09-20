using RimWorld;

namespace Boundir.NewGamePlus
{
    public static class ProductionBills
    {
        public static void OnNewBill(ref Bill __result)
        {
            if (__result == null)
            {
                return;
            }

            if (NewGamePlus.settings.dropOnFloor && __result is Bill_Production bill)
            {
                bill.SetStoreMode(BillStoreModeDefOf.DropOnFloor);
            }

            __result.ingredientSearchRadius = NewGamePlus.settings.billSearchRadius;
        }
    }
}
