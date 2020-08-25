using System;

namespace ProcessControlService.ResourceLibrary.Storage.Rules
{
    public abstract class EntryRule<T> : RuleBase<T>,IEntryRule where T : Storage
    {
        protected EntryRule(T storage) : base(storage) { }


        public abstract Coordinate GetNextEntryPosition(object entryItem);

    }
}
