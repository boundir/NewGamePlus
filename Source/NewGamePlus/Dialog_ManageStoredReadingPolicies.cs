using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Boundir.NewGamePlus
{
    public class Dialog_ManageStoredReadingPolicies : Dialog_ManageReadingPolicies
    {
        private static StoredPolicyCategory<ReadingPolicy> Store => PersistentStore.Data.reading;

        public Dialog_ManageStoredReadingPolicies(ReadingPolicy policy) : base(policy)
        {
        }

        protected override ReadingPolicy CreateNewPolicy()
        {
            int id = Store.NextId();
            ReadingPolicy policy = new ReadingPolicy(id, string.Format("{0} {1}", "ReadingPolicy".Translate(), id));
            Store.policies.Add(policy);
            return policy;
        }

        protected override ReadingPolicy GetDefaultPolicy()
        {
            return Store.Default();
        }

        protected override void SetDefaultPolicy(ReadingPolicy policy)
        {
            Store.SetDefault(policy);
        }

        protected override AcceptanceReport TryDeletePolicy(ReadingPolicy policy)
        {
            return Store.TryDelete(policy);
        }

        protected override List<ReadingPolicy> GetPolicies()
        {
            return Store.policies;
        }

        public override void PostClose()
        {
            base.PostClose();
            PersistentStore.Save();
        }
    }
}
