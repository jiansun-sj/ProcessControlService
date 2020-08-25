namespace ProcessControlService.ResourceLibrary.Storage.Rules
{
    public interface IExitRule : IRuleBase
    {
        Coordinate GetNextExitPosition();
    }
}
