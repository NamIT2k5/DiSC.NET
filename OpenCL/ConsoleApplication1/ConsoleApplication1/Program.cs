using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.IO;
using Cloo;
using System.Runtime.InteropServices;
//http://dhruba.name/2012/10/14/opencl-cookbook-hello-world-using-c-cloo-host-binding/
namespace test
{
    [StructLayout(LayoutKind.Sequential)]
    unsafe public struct KK
    {
        public int i;
        public char c;
    };

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // pick first platform
                ComputePlatform platform = ComputePlatform.Platforms[1];
                platform = null;
                //Select AMD OpenCL
                foreach (var p in ComputePlatform.Platforms)
                {
                    if (p.Name.Contains("AMD"))
                    {
                        platform = p;
                        break;
                    }
                }

                // create context with all gpu devices
                ComputeContext context = new ComputeContext(ComputeDeviceTypes.Gpu,
                    new ComputeContextPropertyList(platform), null, IntPtr.Zero);

                // create a command queue with first gpu found
                ComputeCommandQueue queue = new ComputeCommandQueue(context,
                    context.Devices[0], ComputeCommandQueueFlags.None);

                // load opencl source
                StreamReader streamReader = new StreamReader(@"..\..\kernels.cl");
                string clSource = streamReader.ReadToEnd();
                streamReader.Close();

                // create program with opencl source
                ComputeProgram program = new ComputeProgram(context, clSource);
                
                // compile opencl source
                program.Build(null, null, null, IntPtr.Zero);



                
                // load chosen kernel from program
                ComputeKernel kernel = program.CreateKernel("helloWorld");
                // create a ten integer array and its length
                int[] message = new int[] { 1, 2, 3, 4, 5 };
                int messageSize = message.Length;
                // allocate a memory buffer with the message (the int array)
                ComputeBuffer<int> messageBuffer = new ComputeBuffer<int>(context,
                    ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.UseHostPointer, message);
                
                
                kernel.SetMemoryArgument(0, messageBuffer); // set the integer array
                kernel.SetValueArgument(1, messageSize); // set the array size
                //http://stackoverflow.com/questions/3964003/cloo-opencl-c-sharp-problem
                
                

                
                ///----
                // execute kernel
                queue.ExecuteTask(kernel, null);
                long [] globalWorkOffset=new long[]{ 1, 2, 3, 4, 5 };
               
                

                // wait for completion
                queue.Finish();
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                System.Console.Write(ex.Message);
            }
        }
    }
}