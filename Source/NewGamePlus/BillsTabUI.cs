using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Boundir.NewGamePlus
{
    /// <summary>
    /// Settings tab for the stored bills: the hand-curated library, then one group per
    /// colony. Rows expand to show what a bill actually does, and can be renamed, copied
    /// into the library, or deleted.
    /// </summary>
    public static class BillsTabUI
    {
        private enum RowKind
        {
            Section,
            Empty,
            ColonyHeader,
            BenchGroup,
            Record
        }

        /// <summary>
        /// One line of the list, measured before anything is drawn. Building this first
        /// is what lets the scroll view know its height without guessing.
        /// </summary>
        private class Row
        {
            public RowKind kind;
            public float height;
            public string text;
            public ColonyBillCapture capture;
            public BenchBillPreset group;
            public BillRecord record;
            public List<KeyValuePair<string, string>> detailLines;
            public float detailHeight;
        }

        private static Vector2 scrollPosition;
        private static readonly HashSet<string> expandedGroups = new HashSet<string>();
        private static readonly HashSet<BillRecord> expandedRecords = new HashSet<BillRecord>();
        private static readonly HashSet<ColonyBillCapture> expandedColonies = new HashSet<ColonyBillCapture>();

        public static void Draw(Rect rect)
        {
            PersistentData data = PersistentStore.Data;

            Rect noteRect = new Rect(rect.x, rect.y, rect.width, 40f);
            Text.Font = GameFont.Tiny;
            GUI.color = Color.gray;
            Widgets.Label(noteRect, "NGP_BillsTabDesc".Translate());
            Text.Font = GameFont.Small;
            GUI.color = Color.white;

            Rect footer = new Rect(rect.x, rect.yMax - 34f, rect.width, 30f);
            Rect outRect = new Rect(rect.x, noteRect.yMax + 4f, rect.width, footer.y - noteRect.yMax - 12f);
            float viewWidth = outRect.width - 16f;

            List<Row> rows = BuildRows(data, viewWidth);
            PruneExpansionState(data);

            float contentHeight = 0f;
            for (int i = 0; i < rows.Count; i++)
            {
                contentHeight += rows[i].height;
            }

            Rect viewRect = new Rect(0f, 0f, viewWidth, contentHeight);

            // Every button below stores its effect here instead of running it inline:
            // mutating a list mid-draw both throws and invalidates the height above.
            Action pending = null;

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            float y = 0f;
            int stripe = 0;
            foreach (Row row in rows)
            {
                DrawRow(new Rect(0f, y, viewWidth, row.height), row, stripe, ref pending);
                if (row.kind == RowKind.Record)
                {
                    stripe++;
                }

                y += row.height;
            }

            Widgets.EndScrollView();

            if (pending != null)
            {
                pending();
            }

            DrawFooter(footer, data);
        }

        private static List<Row> BuildRows(PersistentData data, float viewWidth)
        {
            List<Row> rows = new List<Row>();

            rows.Add(new Row { kind = RowKind.Section, height = BillsUI.SectionHeight, text = "NGP_BillLibrary".Translate() });
            if (data.billLibrary.Count == 0)
            {
                rows.Add(new Row { kind = RowKind.Empty, height = 30f, text = "NGP_NoBillLibrary".Translate() });
            }
            else
            {
                foreach (BenchBillPreset group in data.billLibrary.OrderBy(g => g.benchDef?.label ?? ""))
                {
                    AddGroup(rows, null, group, viewWidth);
                }
            }

            rows.Add(new Row { kind = RowKind.Section, height = BillsUI.SectionHeight, text = "NGP_BillColonies".Translate() });
            if (data.billCaptures.Count == 0)
            {
                rows.Add(new Row { kind = RowKind.Empty, height = 30f, text = "NGP_NoBillCaptures".Translate() });
                return rows;
            }

            foreach (ColonyBillCapture capture in data.billCaptures.OrderByDescending(c => c.capturedAtUnix))
            {
                rows.Add(new Row
                {
                    kind = RowKind.ColonyHeader,
                    height = BillsUI.GroupHeight,
                    capture = capture
                });

                if (!expandedColonies.Contains(capture))
                {
                    continue;
                }

                foreach (BenchBillPreset group in capture.benches.OrderBy(g => g.benchDef?.label ?? ""))
                {
                    AddGroup(rows, capture, group, viewWidth);
                }
            }

            return rows;
        }

        private static void AddGroup(List<Row> rows, ColonyBillCapture capture, BenchBillPreset group, float viewWidth)
        {
            rows.Add(new Row
            {
                kind = RowKind.BenchGroup,
                height = BillsUI.GroupHeight,
                capture = capture,
                group = group
            });

            if (!expandedGroups.Contains(BillsUI.GroupKey(capture, group)))
            {
                return;
            }

            foreach (BillRecord record in group.bills)
            {
                Row row = new Row
                {
                    kind = RowKind.Record,
                    height = BillsUI.RowHeight,
                    capture = capture,
                    group = group,
                    record = record
                };

                if (expandedRecords.Contains(record))
                {
                    // Built once here and reused by the draw pass, so the measured and
                    // the drawn panel are literally the same list.
                    row.detailLines = BillDetails.Lines(record, group.benchDef, capture?.Label);
                    row.detailHeight = BillDetails.Height(row.detailLines, viewWidth - 64f);
                    row.height += row.detailHeight + BillsUI.DetailGap;
                }

                rows.Add(row);
            }
        }

        /// <summary>
        /// Drops expansion state for things that no longer exist, so deleted records do
        /// not keep being referenced.
        /// </summary>
        private static void PruneExpansionState(PersistentData data)
        {
            expandedColonies.RemoveWhere(c => !data.billCaptures.Contains(c));

            HashSet<BillRecord> live = new HashSet<BillRecord>();
            foreach (BenchBillPreset group in data.billLibrary)
            {
                live.AddRange(group.bills);
            }

            foreach (ColonyBillCapture capture in data.billCaptures)
            {
                foreach (BenchBillPreset group in capture.benches)
                {
                    live.AddRange(group.bills);
                }
            }

            expandedRecords.RemoveWhere(r => !live.Contains(r));
        }

        private static void DrawRow(Rect rect, Row row, int stripe, ref Action pending)
        {
            switch (row.kind)
            {
                case RowKind.Section:
                    float y = rect.y;
                    Widgets.ListSeparator(ref y, rect.width, row.text);
                    return;

                case RowKind.Empty:
                    Text.Anchor = TextAnchor.MiddleLeft;
                    GUI.color = Color.gray;
                    Widgets.Label(new Rect(rect.x + 4f, rect.y, rect.width - 8f, rect.height), row.text);
                    GUI.color = Color.white;
                    Text.Anchor = TextAnchor.UpperLeft;
                    return;

                case RowKind.ColonyHeader:
                    DrawColonyHeader(rect, row, ref pending);
                    return;

                case RowKind.BenchGroup:
                    DrawBenchGroup(rect, row, ref pending);
                    return;

                default:
                    DrawRecord(rect, row, stripe, ref pending);
                    return;
            }
        }

        private static void DrawColonyHeader(Rect rect, Row row, ref Action pending)
        {
            ColonyBillCapture capture = row.capture;
            Widgets.DrawLightHighlight(rect);
            Widgets.DrawHighlightIfMouseover(rect);

            Rect expandRect = new Rect(rect.x + 2f, rect.y + 2f, BillsUI.IconSize, BillsUI.IconSize);
            bool expanded = expandedColonies.Contains(capture);
            if (BillsUI.Expander(expandRect, expanded))
            {
                if (expanded)
                {
                    expandedColonies.Remove(capture);
                }
                else
                {
                    expandedColonies.Add(capture);
                }
            }

            float buttonX = rect.xMax;
            Rect deleteRect = new Rect(buttonX - 27f, rect.y + 2f, BillsUI.IconSize, BillsUI.IconSize);
            buttonX -= BillsUI.Stride;
            Rect copyRect = new Rect(buttonX - 27f, rect.y + 2f, BillsUI.IconSize, BillsUI.IconSize);
            buttonX -= BillsUI.Stride;
            Rect renameRect = new Rect(buttonX - 27f, rect.y + 2f, BillsUI.IconSize, BillsUI.IconSize);

            Rect labelRect = new Rect(expandRect.xMax + 6f, rect.y, renameRect.x - expandRect.xMax - 10f, rect.height);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.LabelEllipses(labelRect, "NGP_ColonyRow".Translate(capture.Label, capture.benches.Count, capture.BillCount));
            Text.Anchor = TextAnchor.UpperLeft;

            // Clicking the label is the same as using the expander.
            if (Widgets.ButtonInvisible(labelRect))
            {
                if (expanded)
                {
                    expandedColonies.Remove(capture);
                }
                else
                {
                    expandedColonies.Add(capture);
                }
            }

            if (capture.capturedAtUnix > 0L && Mouse.IsOver(labelRect))
            {
                TooltipHandler.TipRegion(labelRect, "NGP_ColonyUpdated".Translate(
                    DateTimeOffset.FromUnixTimeSeconds(capture.capturedAtUnix).ToLocalTime().ToString("g")));
            }

            if (Widgets.ButtonImage(renameRect, TexButton.Rename, Color.white, doMouseoverSound: true,
                    tooltip: "NGP_RenameColonyTip".Translate().ToString()))
            {
                Find.WindowStack.Add(new Dialog_RenameStored<ColonyBillCapture>(capture));
            }

            if (Widgets.ButtonImage(copyRect, TexButton.Copy, Color.white, doMouseoverSound: true,
                    tooltip: "NGP_CopyColonyToLibraryTip".Translate().ToString()))
            {
                ColonyBillCapture toCopy = capture;
                pending = delegate { CopyColonyToLibrary(toCopy); };
            }

            if (Widgets.ButtonImage(deleteRect, TexButton.Delete, Color.white, doMouseoverSound: true,
                    tooltip: "NGP_DeleteColonyTip".Translate().ToString()))
            {
                ColonyBillCapture toDelete = capture;
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                    "NGP_ConfirmDeleteColony".Translate(toDelete.BillCount, toDelete.Label),
                    delegate
                    {
                        PersistentStore.Data.billCaptures.Remove(toDelete);
                        PersistentStore.Data.billStamp++;
                        PersistentStore.Save();
                    },
                    destructive: true));
            }
        }

        private static void DrawBenchGroup(Rect rect, Row row, ref Action pending)
        {
            BenchBillPreset group = row.group;
            string key = BillsUI.GroupKey(row.capture, group);
            bool expanded = expandedGroups.Contains(key);

            // Bench groups sit one level in from their colony.
            float indent = row.capture == null ? 0f : 16f;
            Widgets.DrawHighlightIfMouseover(rect);

            Rect expandRect = new Rect(rect.x + indent + 2f, rect.y + 2f, BillsUI.IconSize, BillsUI.IconSize);
            if (BillsUI.Expander(expandRect, expanded))
            {
                if (expanded)
                {
                    expandedGroups.Remove(key);
                }
                else
                {
                    expandedGroups.Add(key);
                }
            }

            Rect deleteRect = new Rect(rect.xMax - 27f, rect.y + 2f, BillsUI.IconSize, BillsUI.IconSize);
            Rect labelRect = new Rect(expandRect.xMax + 6f, rect.y, deleteRect.x - expandRect.xMax - 10f, rect.height);

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.LabelEllipses(labelRect, "NGP_BenchGroupRow".Translate(
                group.benchDef != null ? group.benchDef.LabelCap : (TaggedString)"?", group.bills.Count));
            Text.Anchor = TextAnchor.UpperLeft;

            if (Widgets.ButtonInvisible(labelRect))
            {
                if (expanded)
                {
                    expandedGroups.Remove(key);
                }
                else
                {
                    expandedGroups.Add(key);
                }
            }

            if (Widgets.ButtonImage(deleteRect, TexButton.Delete, Color.white, doMouseoverSound: true,
                    tooltip: "NGP_DeleteBenchGroupTip".Translate().ToString()))
            {
                BenchBillPreset toDelete = group;
                ColonyBillCapture owner = row.capture;
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                    "NGP_ConfirmDeleteBenchGroup".Translate(toDelete.bills.Count,
                        toDelete.benchDef != null ? toDelete.benchDef.LabelCap : (TaggedString)"?"),
                    delegate
                    {
                        if (owner == null)
                        {
                            PersistentStore.Data.billLibrary.Remove(toDelete);
                        }
                        else
                        {
                            owner.benches.Remove(toDelete);
                            if (owner.benches.Count == 0)
                            {
                                PersistentStore.Data.billCaptures.Remove(owner);
                            }
                        }

                        PersistentStore.Data.billStamp++;
                        PersistentStore.Save();
                    },
                    destructive: true));
            }
        }

        private static void DrawRecord(Rect rect, Row row, int stripe, ref Action pending)
        {
            BillRecord record = row.record;
            Rect headerRect = new Rect(rect.x, rect.y, rect.width, BillsUI.RowHeight);

            if (stripe % 2 == 1)
            {
                Widgets.DrawLightHighlight(headerRect);
            }

            Widgets.DrawHighlightIfMouseover(headerRect);

            bool expanded = expandedRecords.Contains(record);
            Rect expandRect = new Rect(headerRect.x + 34f, headerRect.y + 4f, BillsUI.IconSize, BillsUI.IconSize);
            if (BillsUI.Expander(expandRect, expanded, "NGP_ExpandDetailsTip"))
            {
                if (expanded)
                {
                    expandedRecords.Remove(record);
                }
                else
                {
                    expandedRecords.Add(record);
                }
            }

            Rect iconRect = new Rect(expandRect.xMax + 4f, headerRect.y + 4f, BillsUI.IconSize, BillsUI.IconSize);
            BillsUI.RecipeIcon(iconRect, record);

            float buttonX = headerRect.xMax;
            Rect deleteRect = new Rect(buttonX - 27f, headerRect.y + 4f, BillsUI.IconSize, BillsUI.IconSize);
            buttonX -= BillsUI.Stride;

            // Copying into the library only makes sense from a capture.
            Rect copyRect = Rect.zero;
            if (row.capture != null)
            {
                copyRect = new Rect(buttonX - 27f, headerRect.y + 4f, BillsUI.IconSize, BillsUI.IconSize);
                buttonX -= BillsUI.Stride;
            }

            Rect renameRect = new Rect(buttonX - 27f, headerRect.y + 4f, BillsUI.IconSize, BillsUI.IconSize);
            Rect repeatRect = new Rect(renameRect.x - 78f, headerRect.y, 74f, headerRect.height);
            Rect labelRect = new Rect(iconRect.xMax + 6f, headerRect.y, repeatRect.x - iconRect.xMax - 10f, headerRect.height);

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.LabelEllipses(labelRect, record.Label);

            Text.Font = GameFont.Tiny;
            GUI.color = Color.gray;
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(repeatRect, BillDetails.RepeatShort(record));
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
            Text.Font = GameFont.Small;

            if (Widgets.ButtonInvisible(labelRect))
            {
                if (expanded)
                {
                    expandedRecords.Remove(record);
                }
                else
                {
                    expandedRecords.Add(record);
                }
            }

            if (Widgets.ButtonImage(renameRect, TexButton.Rename, Color.white, doMouseoverSound: true,
                    tooltip: "NGP_RenameBillTip".Translate().ToString()))
            {
                BillRecord toRename = record;
                Find.WindowStack.Add(new Dialog_RenameStored<BillRecord>(toRename, delegate { toRename.labelPinned = true; }));
            }

            if (row.capture != null && Widgets.ButtonImage(copyRect, TexButton.Copy, Color.white, doMouseoverSound: true,
                    tooltip: "NGP_CopyToLibraryTip".Translate().ToString()))
            {
                BillRecord toCopy = record;
                ThingDef benchDef = row.group.benchDef;
                pending = delegate
                {
                    BillPresets.CopyToLibrary(toCopy, benchDef, out bool alreadyPresent);
                    if (alreadyPresent)
                    {
                        Messages.Message("NGP_AlreadyInLibrary".Translate(toCopy.Label), MessageTypeDefOf.RejectInput, historical: false);
                    }
                    else
                    {
                        Messages.Message("NGP_CopiedToLibrary".Translate(toCopy.Label), MessageTypeDefOf.TaskCompletion, historical: false);
                        PersistentStore.Save();
                    }
                };
            }

            if (Widgets.ButtonImage(deleteRect, TexButton.Delete, Color.white, doMouseoverSound: true,
                    tooltip: "NGP_DeleteBillTip".Translate().ToString()))
            {
                BillRecord toDelete = record;
                BenchBillPreset group = row.group;
                ColonyBillCapture owner = row.capture;
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                    "NGP_ConfirmDeleteBill".Translate(toDelete.Label),
                    delegate
                    {
                        group.bills.Remove(toDelete);
                        if (group.bills.Count == 0)
                        {
                            if (owner == null)
                            {
                                PersistentStore.Data.billLibrary.Remove(group);
                            }
                            else
                            {
                                owner.benches.Remove(group);
                                if (owner.benches.Count == 0)
                                {
                                    PersistentStore.Data.billCaptures.Remove(owner);
                                }
                            }
                        }

                        PersistentStore.Data.billStamp++;
                        PersistentStore.Save();
                    },
                    destructive: true));
            }

            if (row.detailLines != null)
            {
                Rect detailRect = new Rect(rect.x + 60f, headerRect.yMax, rect.width - 64f, row.detailHeight);
                Widgets.DrawBoxSolid(detailRect, new Color(1f, 1f, 1f, 0.05f));
                BillDetails.Draw(detailRect, row.detailLines);
            }
        }

        private static void DrawFooter(Rect footer, PersistentData data)
        {
            float buttonWidth = (footer.width - 20f) / 3f;
            Rect captureRect = new Rect(footer.x, footer.y, buttonWidth, 30f);
            Rect libraryRect = new Rect(captureRect.xMax + 10f, footer.y, buttonWidth, 30f);
            Rect wipeRect = new Rect(libraryRect.xMax + 10f, footer.y, buttonWidth, 30f);

            // A world is needed, not just a game: capture keys the group on it.
            if (Utils.DisableableButton(captureRect, "NGP_CaptureBillsNow".Translate(), Current.Game?.World != null))
            {
                ColonyBillCapture capture = BillPresets.ExportFromGame(Current.Game);
                if (capture != null)
                {
                    PersistentStore.Save();
                    expandedColonies.Add(capture);
                    Messages.Message("NGP_BillsCaptured".Translate(capture.BillCount, capture.Label),
                        MessageTypeDefOf.TaskCompletion, historical: false);
                }
                else
                {
                    Messages.Message("NGP_NoBillsToCapture".Translate(), MessageTypeDefOf.RejectInput, historical: false);
                }
            }

            if (Utils.DisableableButton(libraryRect, "NGP_ClearLibrary".Translate(), data.billLibrary.Count > 0))
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("NGP_ConfirmClearLibrary".Translate(), delegate
                {
                    data.billLibrary.Clear();
                    data.billStamp++;
                    PersistentStore.Save();
                }, destructive: true));
            }

            if (Utils.DisableableButton(wipeRect, "NGP_DeleteAllCaptures".Translate(), data.billCaptures.Count > 0))
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("NGP_ConfirmDeleteAllCaptures".Translate(), delegate
                {
                    data.billCaptures.Clear();
                    data.billStamp++;
                    PersistentStore.Save();
                }, destructive: true));
            }
        }

        /// <summary>
        /// Copies every bill of a colony group into the library, under the bench type it
        /// was tuned on. Bills already in the library are counted, not duplicated.
        /// </summary>
        private static void CopyColonyToLibrary(ColonyBillCapture capture)
        {
            int copied = 0;
            int already = 0;

            // Snapshot the groups first: CopyToLibrary adds to billLibrary, which the
            // caller may be part-way through enumerating.
            foreach (BenchBillPreset group in capture.benches.ToList())
            {
                foreach (BillRecord record in group.bills.ToList())
                {
                    BillPresets.CopyToLibrary(record, group.benchDef, out bool alreadyPresent);
                    if (alreadyPresent)
                    {
                        already++;
                    }
                    else
                    {
                        copied++;
                    }
                }
            }

            if (copied > 0)
            {
                PersistentStore.Save();
            }

            Messages.Message("NGP_CopiedToLibraryMany".Translate(copied, already),
                copied > 0 ? MessageTypeDefOf.TaskCompletion : MessageTypeDefOf.RejectInput,
                historical: false);
        }

    }
}
