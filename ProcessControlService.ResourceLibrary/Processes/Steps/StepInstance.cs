using System;
using System.Collections.Generic;

namespace ProcessControlService.ResourceLibrary.Processes.Steps
{
    public class StepModel
    {

        //todo:增加StepCheckModel
        public List<StepCheck> StepChecks = new List<StepCheck>();

    }

    public class StepInstance:IDisposable
    {



        public void Dispose()
        {
        }
    }
}