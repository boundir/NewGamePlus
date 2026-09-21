using UnityEngine;
using Verse;

namespace Boundir.NewGamePlus
{
    public class AreaEntry : IExposable
    {
        public string label;
        public Color color = Color.white;

        public void ExposeData()
        {
            Scribe_Values.Look(ref label, "label");
            Scribe_Values.Look(ref color, "color", Color.white);
        }
    }
}
