using System;
using System.Linq;
using FtdnaBuddy.Ftdna;

namespace FtdnaBuddy
{
    class Program
    {
        static int Main(string[] args)
        {
            if(args.Length != 2)
            {
                Console.Error.WriteLine("Wrong number of arguments!");
                Console.Error.WriteLine("Usage: ./AsdaLoader.exe KitNumber Password");
                return -1;    
            }

			var service = new FtdnaService();
            var workflow = new FtdnaWorkflow(service, new ConsoleLogger());

            try
            {
                workflow.Execute(args[0], args[1]);
            }
            catch (Exception ex)
            {
                var error = ex;
                while (error.InnerException != null)
                    error = error.InnerException;
                Console.Error.WriteLine(ex);

                return -1;
            }
            return 0;
        }
    }
}
