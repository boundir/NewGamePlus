using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Boundir.NewGamePlus
{
    public class Dialog_ImportBills : Window
    {
        private class Row
        {
            public bool isSection;
            public float height;
            public string text;
            public BillSource source;
            public BillRecord record;
            public bool importable;
            public List<KeyValuePair<string, string>> detailLines;
            public float detailHeight;
        }

        private readonly Building_WorkTable table;
        private readonly HashSet<BillRecord> selected = new HashSet<BillRecord>();
        private readonly HashSet<BillRecord> expanded = new HashSet<BillRecord>();

        private Vector2 scrollPosition;

        private List<BillSource> sources;
        private readonly HashSet<BillRecord> importable = new HashSet<BillRecord>();
        private readonly List<BillRecord> displayOrder = new List<BillRecord>();

        private readonly HashSet<BillRecord> displaySet = new HashSet<BillRecord>();

        private List<Row> rows;
        private float contentHeight;

        private int sourcesStamp = -1;
        private int sourcesBillCount = -1;
        private int sourcesVersion;

        private int rowsVersion = -1;
        private int rowsExpandedVersion = -1;
        private float rowsViewWidth = -1f;
        private int expandedVersion;

        public override Vector2 InitialSize => new Vector2(640f, 700f);

        public Dialog_ImportBills(Building_WorkTable table)
        {
            this.table = table;
            doCloseX = true;
            doCloseButton = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
            forcePause = true;
        }

        private void EnsureSources()
        {
            PersistentData data = PersistentStore.Data;

            if (sources != null
                && data.billStamp == sourcesStamp
                && table.billStack.Count == sourcesBillCount)
            {
                return;
            }

            sourcesStamp = data.billStamp;
            sourcesBillCount = table.billStack.Count;
            sourcesVersion++;

            sources = BillPresets.SourcesFor(table);

            importable.Clear();
            displayOrder.Clear();
            displaySet.Clear();

            foreach (BillSource source in sources)
            {
                foreach (BillRecord record in source.records)
                {
                    displayOrder.Add(record);
                    displaySet.Add(record);
                    if (BillPresets.CanImport(record, table))
                    {
                        importable.Add(record);
                    }
                }
            }

            selected.RemoveWhere(r => !importable.Contains(r));

            if (expanded.RemoveWhere(r => !displaySet.Contains(r)) > 0)
            {
                expandedVersion++;
            }
        }

        private void EnsureRows(float viewWidth)
        {
            if (rows != null
                && rowsVersion == sourcesVersion
                && rowsExpandedVersion == expandedVersion
                && rowsViewWidth == viewWidth)
            {
                return;
            }

            rowsVersion = sourcesVersion;
            rowsExpandedVersion = expandedVersion;
            rowsViewWidth = viewWidth;

            rows = BuildRows(sources, importable, viewWidth);

            contentHeight = 0f;
            for (int i = 0; i < rows.Count; i++)
            {
                contentHeight += rows[i].height;
            }
        }

        private void SetExpanded(BillRecord record, bool expand)
        {
            if (expand ? expanded.Add(record) : expanded.Remove(record))
            {
                expandedVersion++;
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            EnsureSources();

            Rect titleRect = new Rect(inRect.x, inRect.y, inRect.width, 32f);
            using (new TextBlock(GameFont.Medium))
            {
                Widgets.Label(titleRect, "NGP_ImportDialogTitle".Translate());
            }

            Rect subtitleRect = new Rect(inRect.x, titleRect.yMax, inRect.width, 24f);
            GUI.color = Color.gray;
            Widgets.Label(subtitleRect, table.def.LabelCap);
            GUI.color = Color.white;

            Rect selectRow = new Rect(inRect.x, subtitleRect.yMax + 4f, inRect.width, 26f);
            Rect selectAllRect = new Rect(selectRow.x, selectRow.y, 140f, 26f);
            Rect selectNoneRect = new Rect(selectAllRect.xMax + 10f, selectRow.y, 140f, 26f);
            Rect expandRect = new Rect(selectNoneRect.xMax + 10f, selectRow.y, 140f, 26f);

            if (Utils.DisableableButton(selectAllRect, "NGP_SelectAll".Translate(), importable.Count > 0))
            {
                selected.AddRange(importable);
            }

            if (Utils.DisableableButton(selectNoneRect, "NGP_SelectNone".Translate(), selected.Count > 0))
            {
                selected.Clear();
            }

            bool anyExpanded = expanded.Count > 0;
            if (Utils.DisableableButton(expandRect,
                    anyExpanded ? "NGP_CollapseAll".Translate() : "NGP_ExpandAll".Translate(),
                    displayOrder.Count > 0))
            {
                if (anyExpanded)
                {
                    expanded.Clear();
                }
                else
                {
                    expanded.AddRange(displayOrder);
                }

                expandedVersion++;
            }

            Rect footerRect = new Rect(inRect.x, inRect.yMax - Window.CloseButSize.y - 40f, inRect.width, 30f);
            Rect outRect = new Rect(inRect.x, selectRow.yMax + 6f, inRect.width, footerRect.y - selectRow.yMax - 12f);
            float viewWidth = outRect.width - 16f;

            EnsureRows(viewWidth);

            Rect viewRect = new Rect(0f, 0f, viewWidth, contentHeight);

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            float y = 0f;
            int stripe = 0;
            foreach (Row row in rows)
            {
                if (row.isSection)
                {
                    float sectionY = y;
                    Widgets.ListSeparator(ref sectionY, viewWidth, row.text);
                }
                else
                {
                    DrawRow(new Rect(0f, y, viewWidth, row.height), row, stripe);
                    stripe++;
                }

                y += row.height;
            }

            Widgets.EndScrollView();

            bool stackFull = table.billStack.Count >= BillStack.MaxCount;
            float buttonWidth = (footerRect.width - 10f) / 2f;
            Rect importSelectedRect = new Rect(footerRect.x, footerRect.y, buttonWidth, 30f);
            Rect importAllRect = new Rect(importSelectedRect.xMax + 10f, footerRect.y, buttonWidth, 30f);

            if (Utils.DisableableButton(importSelectedRect, "NGP_ImportSelected".Translate(selected.Count), selected.Count > 0 && !stackFull))
            {
                BillPresets.ImportMany(displayOrder.Where(r => selected.Contains(r)), table);
                Close();
            }

            if (Utils.DisableableButton(importAllRect, "NGP_ImportAllBills".Translate(importable.Count), importable.Count > 0 && !stackFull))
            {
                BillPresets.ImportMany(displayOrder.Where(r => importable.Contains(r)), table);
                Close();
            }

            if (stackFull)
            {
                TooltipHandler.TipRegionByKey(footerRect, "NGP_BillStackFull");
            }
        }

        private List<Row> BuildRows(List<BillSource> sources, HashSet<BillRecord> importable, float viewWidth)
        {
            List<Row> rows = new List<Row>();
            object lastOwner = null;
            bool firstSection = true;

            foreach (BillSource source in sources)
            {
                object owner = source.isLibrary ? (object)"library" : source.capture;
                if (firstSection || owner != lastOwner)
                {
                    rows.Add(new Row { isSection = true, height = BillsUI.SectionHeight, text = source.SectionLabel });
                    lastOwner = owner;
                    firstSection = false;
                }

                foreach (BillRecord record in source.records)
                {
                    Row row = new Row
                    {
                        height = BillsUI.RowHeight,
                        source = source,
                        record = record,
                        importable = importable.Contains(record)
                    };

                    if (expanded.Contains(record))
                    {
                        row.detailLines = BillDetails.Lines(record, source.originBench,
                            source.isLibrary ? null : source.capture.Label);
                        row.detailHeight = BillDetails.Height(row.detailLines, viewWidth - 64f);
                        row.height += row.detailHeight + BillsUI.DetailGap;
                    }

                    rows.Add(row);
                }
            }

            return rows;
        }

        private void DrawRow(Rect rect, Row row, int stripe)
        {
            BillRecord record = row.record;
            Rect headerRect = new Rect(rect.x, rect.y, rect.width, BillsUI.RowHeight);

            if (stripe % 2 == 1)
            {
                Widgets.DrawLightHighlight(headerRect);
            }

            if (Mouse.IsOver(headerRect))
            {
                Widgets.DrawHighlight(headerRect);
            }

            bool isSelected = selected.Contains(record);
            bool checkOn = isSelected;
            Widgets.Checkbox(new Vector2(headerRect.x + 4f, headerRect.y + 4f), ref checkOn, BillsUI.IconSize, disabled: !row.importable);
            if (checkOn != isSelected)
            {
                if (checkOn)
                {
                    selected.Add(record);
                }
                else
                {
                    selected.Remove(record);
                }
            }

            bool isExpanded = expanded.Contains(record);
            Rect expandRect = new Rect(headerRect.x + 32f, headerRect.y + 4f, BillsUI.IconSize, BillsUI.IconSize);
            if (BillsUI.Expander(expandRect, isExpanded, "NGP_ExpandDetailsTip"))
            {
                SetExpanded(record, !isExpanded);
            }

            Rect iconRect = new Rect(headerRect.x + 60f, headerRect.y + 4f, BillsUI.IconSize, BillsUI.IconSize);
            BillsUI.RecipeIcon(iconRect, record);

            Rect renameRect = new Rect(headerRect.xMax - 27f, headerRect.y + 4f, BillsUI.IconSize, BillsUI.IconSize);
            Rect repeatRect = new Rect(renameRect.x - 78f, headerRect.y, 74f, headerRect.height);
            Rect labelRect = new Rect(iconRect.xMax + 6f, headerRect.y, repeatRect.x - iconRect.xMax - 10f, headerRect.height);

            if (!row.importable)
            {
                GUI.color = Color.gray;
            }

            Text.Anchor = TextAnchor.MiddleLeft;
            string label = record.Label;
            if (!row.importable)
            {
                label += " (" + "NGP_ResearchLocked".Translate() + ")";
            }

            Widgets.LabelEllipses(labelRect, label);

            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(repeatRect, BillDetails.RepeatShort(record));
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;

            if (Widgets.ButtonInvisible(labelRect))
            {
                SetExpanded(record, !isExpanded);
            }

            if (Widgets.ButtonImage(renameRect, TexButton.Rename, Color.white, doMouseoverSound: true,
                    tooltip: "NGP_RenameBillTip".Translate().ToString()))
            {
                BillRecord toRename = record;
                Find.WindowStack.Add(new Dialog_RenameStored<BillRecord>(toRename, delegate { toRename.labelPinned = true; }));
            }

            if (row.detailLines != null)
            {
                Rect detailRect = new Rect(rect.x + 60f, headerRect.yMax, rect.width - 64f, row.detailHeight);
                Widgets.DrawBoxSolid(detailRect, new Color(1f, 1f, 1f, 0.05f));
                BillDetails.Draw(detailRect, row.detailLines);
            }
        }
    }
}
