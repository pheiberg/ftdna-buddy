﻿using System;
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
            var ekitId = conn.GetEkitIdAsync().Result;
            Console.Write(ekitId);
            var matchData = conn.ListMatches(1500, 1).Result;
            foreach(var match in matchData.Data)
            {
                Console.WriteLine($"{match.MatchPersonName} - Tot {match.TotalCM} cM - LB {match.LongestCentimorgans} cM");
            }
        }
    }
}
