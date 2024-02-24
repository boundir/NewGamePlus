using RimWorld;

namespace Boundir.NewGamePlus
{
    public static class FactionRewards
    {
        public static void SetFactionPreferences(Faction __instance)
        {
            __instance.allowGoodwillRewards = NewGamePlus.settings.allowGoodwillRewards;
            __instance.allowRoyalFavorRewards = NewGamePlus.settings.allowRoyalFavorRewards;
        }
    }
}
