using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


namespace ProcessControlService.ResourceFactory
{


    public interface IWork
    {

        bool WorkStarted
        { get; }

        void StartWork();

        void StopWork();

    }
}
