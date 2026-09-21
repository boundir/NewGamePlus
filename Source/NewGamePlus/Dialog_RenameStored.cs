using System;
using Verse;

namespace Boundir.NewGamePlus
{
    public class Dialog_RenameStored<T> : Dialog_Rename<T> where T : class, IRenameable
    {
        private readonly Action afterRename;

        public Dialog_RenameStored(T renaming, Action afterRename = null)
            : base(renaming)
        {
            this.afterRename = afterRename;
        }

        protected override void OnRenamed(string name)
        {
            afterRename?.Invoke();
            PersistentStore.Data.billStamp++;
            PersistentStore.Save();
        }
    }
}
