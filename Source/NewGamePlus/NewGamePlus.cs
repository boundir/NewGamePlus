using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace Boundir.NewGamePlus
{
    public class NewGamePlus : Mod
    {
        public static Settings settings;

        public NewGamePlus(ModContentPack content) : base(content)
        {
            settings = GetSettings<Settings>();
        }

        public override void DoSettingsWindowContents(Rect rect)
        {
            base.DoSettingsWindowContents(rect);
            settings.DoWindowContents(rect);
        }

        public override string SettingsCategory()
        {
            return "NewGamePlus".Translate();
        }
    }
}
