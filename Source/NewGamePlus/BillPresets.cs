using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Boundir.NewGamePlus
{
    public class BillSource
    {
        public bool isLibrary;

        public ColonyBillCapture capture;

        public BenchBillPreset group;

        public ThingDef originBench;

        public List<BillRecord> records = new List<BillRecord>();

        public string SectionLabel => isLibrary ? "NGP_BillLibrary".Translate().ToString() : capture.Label;
    }

    public struct BillAvailability
    {
        public int matching;

        public int importable;
    }

    public static class BillPresets
    {
        private static bool IsExportable(Bill bill)
        {
            return bill is Bill_Production && !(bill is Bill_Autonomous) && bill.recipe != null;
        }

        public static ColonyBillCapture ExportFromGame(Game game)
        {
            if (game == null)
            {
                return null;
            }

            WorldInfo info = game.World?.info ?? Find.World?.info;
            if (info == null)
            {
                return null;
            }

            int colonyId = ColonyIdFor(info);

            Dictionary<ThingDef, Dictionary<string, BillRecord>> fresh =
                new Dictionary<ThingDef, Dictionary<string, BillRecord>>();

            foreach (Map map in game.Maps)
            {
                foreach (Building_WorkTable table in map.listerBuildings.AllBuildingsColonistOfClass<Building_WorkTable>())
                {
                    foreach (Bill bill in table.BillStack.Bills)
                    {
                        if (!IsExportable(bill))
                        {
                            continue;
                        }

                        BillRecord record = BillRecord.FromBill((Bill_Production)bill);
                        string key = BillRecordKey.For(record);

                        if (!fresh.TryGetValue(table.def, out Dictionary<string, BillRecord> byKey))
                        {
                            byKey = new Dictionary<string, BillRecord>();
                            fresh[table.def] = byKey;
                        }

                        if (!byKey.TryGetValue(key, out BillRecord twin))
                        {
                            byKey[key] = record;
                        }
                        else if (twin.customName.NullOrEmpty() && !record.customName.NullOrEmpty())
                        {
                            byKey[key] = record;
                        }
                    }
                }
            }

            if (fresh.Count == 0)
            {
                return null;
            }

            PersistentData data = PersistentStore.Data;
            ColonyBillCapture capture = data.billCaptures.FirstOrDefault(c => c.colonyId == colonyId);
            if (capture == null)
            {
                capture = new ColonyBillCapture { colonyId = colonyId };
                data.billCaptures.Add(capture);
            }

            capture.colonyName = Faction.OfPlayerSilentFail?.Name;
            capture.worldName = info.name;
            capture.capturedAtUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            foreach (KeyValuePair<ThingDef, Dictionary<string, BillRecord>> pair in fresh)
            {
                MergeBench(capture.BenchGroup(pair.Key, create: true), pair.Value);
            }

            data.billStamp++;
            return capture;
        }

        private static int ColonyIdFor(WorldInfo info)
        {
            return info.persistentRandomValue;
        }

        private static void MergeBench(BenchBillPreset group, Dictionary<string, BillRecord> fresh)
        {
            Dictionary<string, BillRecord> stored = new Dictionary<string, BillRecord>();
            foreach (BillRecord record in group.bills)
            {
                string key = BillRecordKey.For(record);
                if (!stored.ContainsKey(key))
                {
                    stored[key] = record;
                }
            }

            if (stored.Count != group.bills.Count)
            {
                group.bills.RemoveAll(r => !ReferenceEquals(r, stored[BillRecordKey.For(r)]));
            }

            foreach (KeyValuePair<string, BillRecord> item in fresh)
            {
                if (!stored.TryGetValue(item.Key, out BillRecord existing))
                {
                    group.bills.Add(item.Value);
                    continue;
                }

                if (!existing.labelPinned && existing.customName != item.Value.customName)
                {
                    existing.customName = item.Value.customName;
                }
            }
        }

        public static bool MatchesTable(BillRecord record, Building_WorkTable table)
        {
            return record?.recipe != null && table != null && table.def.AllRecipes.Contains(record.recipe);
        }

        public static bool CanImport(BillRecord record, Building_WorkTable table)
        {
            return MatchesTable(record, table)
                && record.recipe.AvailableNow
                && record.recipe.AvailableOnNow(table);
        }

        public static List<BillSource> SourcesFor(Building_WorkTable table)
        {
            List<BillSource> sources = new List<BillSource>();
            if (table == null)
            {
                return sources;
            }

            PersistentData data = PersistentStore.Data;

            foreach (BenchBillPreset group in OrderBenches(data.billLibrary, table))
            {
                AddSource(sources, null, group, table);
            }

            foreach (ColonyBillCapture capture in data.billCaptures.OrderByDescending(c => c.capturedAtUnix))
            {
                foreach (BenchBillPreset group in OrderBenches(capture.benches, table))
                {
                    AddSource(sources, capture, group, table);
                }
            }

            return sources;
        }

        private static IEnumerable<BenchBillPreset> OrderBenches(List<BenchBillPreset> groups, Building_WorkTable table)
        {
            return groups
                .OrderByDescending(g => g.benchDef == table.def)
                .ThenBy(g => g.benchDef?.label ?? "");
        }

        private static void AddSource(List<BillSource> sources, ColonyBillCapture capture, BenchBillPreset group, Building_WorkTable table)
        {
            List<BillRecord> matching = group.bills.Where(r => MatchesTable(r, table)).ToList();
            if (matching.Count == 0)
            {
                return;
            }

            sources.Add(new BillSource
            {
                isLibrary = capture == null,
                capture = capture,
                group = group,
                originBench = group.benchDef,
                records = matching
            });
        }

        private static Building_WorkTable cachedTable;
        private static int cachedStamp = -1;
        private static int cachedBillCount = -1;
        private static int cachedFrame = -1;
        private static BillAvailability cached;

        public static BillAvailability Availability(Building_WorkTable table)
        {
            PersistentData data = PersistentStore.Data;
            if (table == cachedTable
                && data.billStamp == cachedStamp
                && table.billStack.Count == cachedBillCount
                && Time.frameCount - cachedFrame < 30)
            {
                return cached;
            }

            BillAvailability result = default(BillAvailability);
            foreach (BillSource source in SourcesFor(table))
            {
                foreach (BillRecord record in source.records)
                {
                    result.matching++;
                    if (CanImport(record, table))
                    {
                        result.importable++;
                    }
                }
            }

            cachedTable = table;
            cachedStamp = data.billStamp;
            cachedBillCount = table.billStack.Count;
            cachedFrame = Time.frameCount;
            cached = result;
            return result;
        }

        public static void CopyToLibrary(BillRecord record, ThingDef benchDef, out bool alreadyPresent)
        {
            alreadyPresent = false;
            if (record?.recipe == null || benchDef == null)
            {
                return;
            }

            PersistentData data = PersistentStore.Data;
            BenchBillPreset group = data.billLibrary.FirstOrDefault(g => g.benchDef == benchDef);
            if (group == null)
            {
                group = new BenchBillPreset { benchDef = benchDef };
                data.billLibrary.Add(group);
            }

            if (group.Contains(BillRecordKey.For(record)))
            {
                alreadyPresent = true;
                return;
            }

            group.bills.Add(record.Clone());
            data.billStamp++;
        }

        private static bool Import(BillRecord record, Building_WorkTable table)
        {
            if (table.billStack.Count >= BillStack.MaxCount)
            {
                return false;
            }

            Bill_Production bill = record.MakeBill();
            if (bill == null)
            {
                return false;
            }

            table.billStack.AddBill(bill);
            return true;
        }

        public static void ImportMany(IEnumerable<BillRecord> records, Building_WorkTable table)
        {
            int added = 0;
            int noRoom = 0;
            List<string> skipped = new List<string>();

            foreach (BillRecord record in records)
            {
                if (record?.recipe == null)
                {
                    continue;
                }

                if (table.billStack.Count >= BillStack.MaxCount)
                {
                    noRoom++;
                    continue;
                }

                if (!CanImport(record, table))
                {
                    skipped.Add(record.Label);
                    continue;
                }

                if (Import(record, table))
                {
                    added++;
                }
                else
                {
                    skipped.Add(record.Label);
                }
            }

            if (added > 0)
            {
                Messages.Message("NGP_BillsImported".Translate(added), MessageTypeDefOf.TaskCompletion, historical: false);
            }

            if (skipped.Count > 0)
            {
                Messages.Message("NGP_BillsSkipped".Translate(skipped.Count, skipped.ToCommaList()), MessageTypeDefOf.RejectInput, historical: false);
            }

            if (noRoom > 0)
            {
                Messages.Message("NGP_BillsNoRoom".Translate(noRoom, BillStack.MaxCount), MessageTypeDefOf.RejectInput, historical: false);
            }

            PersistentStore.Data.billStamp++;
        }
    }
}
