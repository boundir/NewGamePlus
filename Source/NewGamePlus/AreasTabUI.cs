using System.Linq;
using UnityEngine;
using Verse;

namespace Boundir.NewGamePlus
{
    public static class AreasTabUI
    {
        private static Vector2 scrollPosition;

        private static readonly Color[] Palette =
        {
            new Color(0.85f, 0.35f, 0.35f),
            new Color(0.90f, 0.60f, 0.25f),
            new Color(0.90f, 0.85f, 0.30f),
            new Color(0.55f, 0.85f, 0.35f),
            new Color(0.30f, 0.75f, 0.45f),
            new Color(0.30f, 0.80f, 0.80f),
            new Color(0.35f, 0.60f, 0.90f),
            new Color(0.45f, 0.40f, 0.90f),
            new Color(0.75f, 0.40f, 0.90f),
            new Color(0.90f, 0.40f, 0.70f),
            new Color(0.70f, 0.70f, 0.70f),
            new Color(0.55f, 0.45f, 0.30f)
        };

        public static void Draw(Rect rect)
        {
            PersistentData data = PersistentStore.Data;

            Rect noteRect = new Rect(rect.x, rect.y, rect.width, 40f);
            Text.Font = GameFont.Tiny;
            GUI.color = Color.gray;
            Widgets.Label(noteRect, "NGP_AreasTabDesc".Translate());
            Text.Font = GameFont.Small;
            GUI.color = Color.white;

            Rect footer = new Rect(rect.x, rect.yMax - 34f, rect.width, 30f);
            Rect outRect = new Rect(rect.x, noteRect.yMax + 4f, rect.width, footer.y - noteRect.yMax - 12f);

            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, data.areas.Count * 32f);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            float y = 0f;
            AreaEntry toRemove = null;
            foreach (AreaEntry entry in data.areas)
            {
                Rect row = new Rect(0f, y, viewRect.width, 30f);
                y += 32f;

                Rect colorRect = new Rect(row.x, row.y + 3f, 24f, 24f);
                Widgets.DrawBoxSolid(colorRect, entry.color);
                Widgets.DrawBox(colorRect);
                TooltipHandler.TipRegionByKey(colorRect, "NGP_AreaColorTip");
                if (Widgets.ButtonInvisible(colorRect))
                {
                    entry.color = NextColor(entry.color);
                }

                Rect nameRect = new Rect(colorRect.xMax + 6f, row.y + 3f, row.width - 24f - 6f - 30f, 24f);
                entry.label = Widgets.TextField(nameRect, entry.label);

                Rect deleteRect = new Rect(row.xMax - 27f, row.y + 3f, 24f, 24f);
                if (Widgets.ButtonImage(deleteRect, TexButton.Delete))
                {
                    toRemove = entry;
                }
            }

            Widgets.EndScrollView();

            if (toRemove != null)
            {
                data.areas.Remove(toRemove);
            }

            float buttonWidth = (footer.width - 20f) / 3f;
            Rect addRect = new Rect(footer.x, footer.y, buttonWidth, 30f);
            Rect captureRect = new Rect(addRect.xMax + 10f, footer.y, buttonWidth, 30f);
            Rect resetRect = new Rect(captureRect.xMax + 10f, footer.y, buttonWidth, 30f);

            if (Widgets.ButtonText(addRect, "NGP_AddArea".Translate()))
            {
                data.areas.Add(new AreaEntry
                {
                    label = "NGP_NewAreaName".Translate() + " " + (data.areas.Count + 1),
                    color = Palette[data.areas.Count % Palette.Length]
                });
            }

            if (Utils.DisableableButton(captureRect, "NGP_CaptureFromGame".Translate(), Current.Game != null))
            {
                if (data.areas.Count > 0)
                {
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("NGP_ConfirmCaptureAreas".Translate(), delegate
                    {
                        AreaTransfer.CaptureFromGame(Current.Game);
                        PersistentStore.Save();
                    }));
                }
                else
                {
                    AreaTransfer.CaptureFromGame(Current.Game);
                    PersistentStore.Save();
                }
            }

            if (Utils.DisableableButton(resetRect, "NGP_ResetToVanilla".Translate(), data.areas.Count > 0))
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("NGP_ConfirmResetAreas".Translate(), delegate
                {
                    data.areas.Clear();
                    PersistentStore.Save();
                }));
            }
        }

        private static Color NextColor(Color current)
        {
            int index = Palette.ToList().FindIndex(c => c.IndistinguishableFrom(current));
            return Palette[(index + 1 + Palette.Length) % Palette.Length];
        }
    }
}
