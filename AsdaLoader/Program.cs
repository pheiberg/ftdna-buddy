using System;
using System.Linq;
using AsdaLoader.Ftdna;

namespace AsdaLoader
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

            try
            {
                var conn = new FtdnaConnector();
                Login(conn, args[0], args[1]);
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

        private static void Login(FtdnaConnector conn, string kitNumber, string password)
        {
            string token = conn.GetVerificationTokenAsync().Result;
            Console.WriteLine(token);
            var result = conn.LoginAsync(kitNumber, password).Result;
            Console.WriteLine(result);
            var ekitId = conn.GetEkitIdAsync().Result;
            Console.Write(ekitId);
            var matchData = conn.ListMatches(1500, 1).Result;
            foreach(var match in matchData.Data)
            {
                Console.WriteLine($"{match.MatchPersonName} - Tot {match.TotalCM} cM - LB {match.LongestCentimorgans} cM");
            }
            var segments = conn.ListChromosomeSegmentsAsync(ekitId).Result;
            foreach(var segment in segments)
            {
                Console.WriteLine($"{segment.MatchName} - Chr {segment.Chromosome} - {segment.Centimorgans} cM");
            }
            var aMatch = matchData.Data.First();
            var inCommonWith = conn.ListInCommonWithAsync(aMatch.ResultId2).Result;
            foreach(var icw in inCommonWith.Data)
            {
                Console.WriteLine($"ICW {aMatch.Name} - {icw.Name}");
            }
            var details = conn.GetMatchDetailsAsync(aMatch.ResultId1, aMatch.ResultId2).Result.Result;
            Console.WriteLine($"{aMatch.Name} details: Segments:{details.Segments}, of which larger than 5cM: {details?.FFCMData.Count(s => s.Cm > 5)}");
        }
    }
}
