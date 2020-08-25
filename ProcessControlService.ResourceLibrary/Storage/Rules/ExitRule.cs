namespace ProcessControlService.ResourceLibrary.Storage.Rules
{
    public abstract class ExitRule<T> : RuleBase<T>, IExitRule where T : Storage
    {
        protected ExitRule(T storage) : base(storage)
        {
        }
        public abstract Coordinate GetNextExitPosition();
    }
}
