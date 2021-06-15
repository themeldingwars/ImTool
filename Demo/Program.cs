using System;
using System.Threading.Tasks;
using ImTool;

namespace Demo
{
    class Program
    {
        static async Task Main()
        {
            try
            {
                await DemoTool.Run();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }
    }
}