namespace ProcessControlService.ResourceLibrary.Processes.Conditions
{
    public class ConditionModel
    {
        public enum ConditionStatus
        {
            Unknown = 0,
            NotStart = 1,
            Triged = 2
        }

        public string Name;

        public ConditionStatus Status;
    }
}