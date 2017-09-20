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
                Console.Error.WriteLine("Usage: ./FtdnaBuddy.exe KitNumber Password");
                return -1;    
            }

			string kitNumber = args[0];
			string password = args[1];

			var service = new FtdnaService();
            var logger = new ConsoleLogger();
            var workflow = new FtdnaWorkflow(service, consoleLogger);

            try
            {
                workflow.Execute(kitNumber, password);
            }
            catch (Exception ex)
            {
                var error = ex;
                while (error.InnerException != null)
                    error = error.InnerException;
                logger.LogError(ex.ToString());

                return -1;
            }
            return 0;
        }
    }
}
