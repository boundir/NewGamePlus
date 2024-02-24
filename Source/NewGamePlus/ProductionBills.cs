using RimWorld;

namespace Boundir.NewGamePlus
{
    public static class ProductionBills
    {
        public static void OnNewBill(ref Bill __result)
        {
            Bill_Production bill = __result as Bill_Production;

            if (NewGamePlus.settings.dropOnFloor)
            {
                bill.SetStoreMode(BillStoreModeDefOf.DropOnFloor);
            }

            bill.ingredientSearchRadius = NewGamePlus.settings.billSearchRadius;
        }
    }
}
