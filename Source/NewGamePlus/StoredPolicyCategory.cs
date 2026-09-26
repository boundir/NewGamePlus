using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Boundir.NewGamePlus
{
    public class StoredPolicyCategory<T> : IExposable where T : Policy
    {
        public List<T> policies = new List<T>();

        /// <summary>
        /// Ids of policies that allowed every item of their kind when they were stored.
        /// On import they re-allow everything, so items from mods installed since then stay allowed, like in a vanilla-generated policy.
        /// </summary>
        public List<int> allowAllIds = new List<int>();

        public bool Active => policies.Count > 0;

        public int NextId()
        {
            return policies.Any() ? policies.Max(p => p.id) + 1 : 1;
        }

        public T Default()
        {
            return policies.Count > 0 ? policies[0] : null;
        }

        public void SetDefault(T policy)
        {
            int index = policies.IndexOf(policy);
            if (index > 0)
            {
                T previousDefault = policies[0];
                policies[0] = policy;
                policies[index] = previousDefault;
            }
        }

        public AcceptanceReport TryDelete(T policy)
        {
            if (policies.Count <= 1)
            {
                return new AcceptanceReport("NGP_CannotDeleteLastPolicy".Translate());
            }

            if (Default() == policy)
            {
                return new AcceptanceReport("NGP_CannotDeleteDefaultPolicy".Translate());
            }

            policies.Remove(policy);
            return AcceptanceReport.WasAccepted;
        }

        public void Clear()
        {
            policies.Clear();
            allowAllIds.Clear();
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref policies, "policies", LookMode.Deep);
            Scribe_Collections.Look(ref allowAllIds, "allowAllIds", LookMode.Value);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (policies == null)
                {
                    policies = new List<T>();
                }

                if (allowAllIds == null)
                {
                    allowAllIds = new List<int>();
                }

                if (policies.RemoveAll(p => p == null) > 0)
                {
                    PersistentStore.NotifyPrunedOnLoad();
                }
            }
        }
    }
}
