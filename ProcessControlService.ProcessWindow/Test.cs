using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceLibrary.DataBinding;

namespace ProcessControlService.ProcessWindow
{
    public class Test
    {

        public static void TestMethod()
        {
            var resourceCollection = ResourceManager.GetAllResources();

            foreach (var resource in resourceCollection)
            {
                if (resource is DataBinding dataBinding)
                {
                    dataBinding.ExecuteBinding();
                }
            }



        }

    }
}
