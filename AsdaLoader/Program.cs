using System;
using AsdaLoader.Ftdna;

namespace AsdaLoader
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var conn = new FtdnaConnector();
                Login(conn, "Kit", "Pass");
            }
            catch (Exception ex)
            {
                var error = ex;
                while (error.InnerException != null)
                    error = error.InnerException;
                Console.Error.WriteLine(ex);
            }
        }

        private static void Login(FtdnaConnector conn, string kitNumber, string password)
        {
        	string token = conn.GetVerificationTokenAsync().Result;
			Console.WriteLine(token);
			var result = conn.LoginAsync(token, kitNumber, password).Result;
			Console.WriteLine(result);
            Console.Write(conn.VerifyLoginAsync().Result);
        }
    }
}
