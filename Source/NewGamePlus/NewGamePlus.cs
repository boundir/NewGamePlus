using Verse;
using RimWorld;
using UnityEngine;

namespace Boundir.NewGamePlus
{
    public class NewGamePlus : Mod
    {
        /// <summary>
        /// Settings reference
        /// </summary>
        public static Settings settings;

        /// <summary>
        /// A mandatory constructor which resolves the reference to our settings
        /// </summary>
        /// <param name="content"></param>
        public NewGamePlus(ModContentPack content) : base(content)
        {
            settings = GetSettings<Settings>();
        }

        /// <summary>
        /// Optional GUI part of settings
        /// </summary>
        /// <param name="rect">Unity Rect with the size of the settings window.</param>
        public override void DoSettingsWindowContents(Rect rect)
        {
            base.DoSettingsWindowContents(rect);
            settings.DoWindowContents(rect);
        }

        /// <summary>
        /// Override SettingsCategory to show up in the list of settings
        /// </summary>
        /// <returns></returns>
        public override string SettingsCategory()
        {
            return "NewGamePlus".Translate();
        }
    }
}
