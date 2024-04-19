using Verse;
using HarmonyLib;
using RimWorld;

namespace Boundir.NewGamePlus
{
    [StaticConstructorOnStartup]
    public static class OnGameStart
    {
        static OnGameStart()
        {
            Log.Message("New Game Plus loaded.");
            Harmony harmony = new Harmony("RimWorld.Boundir.NewGamePlus");

            harmony.Patch(
                original: AccessTools.Constructor(type: typeof(PlaySettings)),
                postfix: new HarmonyMethod(methodType: typeof(GameSettings), methodName: nameof(GameSettings.DefaultPlaySettings))
            );
            harmony.Patch(
                original: AccessTools.Constructor(type: typeof(Faction)),
                postfix: new HarmonyMethod(methodType: typeof(FactionRewards), methodName: nameof(FactionRewards.SetFactionPreferences))
            );
            harmony.Patch(
                original: AccessTools.Constructor(type: typeof(OutfitDatabase)),
                postfix: new HarmonyMethod(methodType: typeof(Outfits), methodName: nameof(Outfits.OutfitHitPoints))
            );
            harmony.Patch(
                original: AccessTools.Method(type: typeof(OutfitDatabase), name: nameof(OutfitDatabase.MakeNewOutfit)),
                postfix: new HarmonyMethod(methodType: typeof(Outfits), methodName: nameof(Outfits.OnNewOutfit))
            );
            harmony.Patch(
                original: AccessTools.Method(type: typeof(BillUtility), name: nameof(BillUtility.MakeNewBill)),
                postfix: new HarmonyMethod(methodType: typeof(ProductionBills), methodName: nameof(ProductionBills.OnNewBill))
            );
            harmony.Patch(
                original: AccessTools.Method(type: typeof(StoryWatcher_PopAdaptation), name: nameof(StoryWatcher_PopAdaptation.Notify_PawnEvent)),
                postfix: new HarmonyMethod(methodType: typeof(OnPawnJoin), nameof(OnPawnJoin.HostilityResponse))
            );

            harmony.PatchAll();
        }
    }
}
