namespace ProcessControlService.ResourceLibrary.Storage.Rules
{
    public interface IEntryRule:IRuleBase
    {
        Coordinate GetNextEntryPosition(object entryItem);
    }
}
