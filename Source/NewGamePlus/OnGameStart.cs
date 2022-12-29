using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using HarmonyLib;

namespace Boundir.NewGamePlus
{
    [StaticConstructorOnStartup]
    public static class OnGameStart
    {
        static OnGameStart()
        {
            Log.Message("New Game Plus loaded.");
            Harmony harmony = new Harmony("RimWorld.Boundir.NewGamePlus");
            harmony.PatchAll();
        }
    }
}
