using System.Collections.Generic;
using Verse;
using RimWorld;
using UnityEngine;

namespace Boundir.NewGamePlus
{
    public class NewGamePlus : Mod
    {
        private enum SettingsTab
        {
            General
        }

        /// <summary>
        /// Settings reference
        /// </summary>
        public static Settings settings;

        private static SettingsTab currentTab = SettingsTab.General;

        /// <summary>
        /// A mandatory constructor which resolves the reference to our settings
        /// </summary>
        /// <param name="content"></param>
        public NewGamePlus(ModContentPack content) : base(content)
        {
            settings = GetSettings<Settings>();
        }

        /// <summary>
        /// GUI part of settings: a tab bar, with the gameplay defaults under General.
        /// </summary>
        /// <param name="rect">Unity Rect with the size of the settings window.</param>
        public override void DoSettingsWindowContents(Rect rect)
        {
            PersistentStore.EnsureLoaded();

            rect.yMin += 32f;
            Widgets.DrawMenuSection(rect);
            TabDrawer.DrawTabs(rect, new List<TabRecord>
            {
                new TabRecord("NGP_TabGeneral".Translate(), () => currentTab = SettingsTab.General, currentTab == SettingsTab.General)
            });

            Rect inner = rect.ContractedBy(12f);
            settings.DoGeneralTab(inner);
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            PersistentStore.SaveIfLoaded();
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
