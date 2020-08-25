namespace ProcessControlService.ResourceLibrary.Processes.ParameterBind
{
    public class ParameterBind
    {
        /*public Parameter ActionParameter { get; set; }

        public Parameter BindParameter { get; set; }*/

        public string ActionParameterName { get; set; }

        public string ProcessParameterName { get; set; }

        public string ConstValueString { get; set; }

        // public BaseAction StepAction { get; set; }

        public ParameterBindType ParameterBindType { get; set; }

        //public ProcessInstance ProcessInstance { get; set; }

    }
}