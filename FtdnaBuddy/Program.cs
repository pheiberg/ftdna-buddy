using System;
using System.Linq;
using FtdnaBuddy.Ftdna;
using FtdnaBuddy.Ftdna.Model;

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

			var conn = new FtdnaConnector();
			var service = new FtdnaService(conn);

            try
            {
                var user = service.Login(args[0], args[1]);
                TestFunctions(conn, service, user);
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

        static void TestFunctions(FtdnaConnector conn, FtdnaService service, FtdnaUser user)
        {
            var matchData = service.ListMatches();
            foreach (var match in matchData)
            {
                Console.WriteLine($"{match.MatchPersonName} - Tot {match.TotalCM} cM - LB {match.LongestCentimorgans} cM");
            }
            var segments = conn.ListChromosomeSegmentsAsync(user.EncryptedKitId).Result;
            foreach (var segment in segments)
            {
                Console.WriteLine($"{segment.MatchName} - Chr {segment.Chromosome} - {segment.Centimorgans} cM");
            }
            var aMatch = matchData.Data.First();
            var inCommonWith = conn.ListInCommonWithAsync(aMatch.ResultId2).Result;
            foreach (var icw in inCommonWith.Data)
            {
                Console.WriteLine($"ICW {aMatch.Name} - {icw.Name}");
            }
            var details = conn.GetMatchDetailsAsync(aMatch.ResultId1, aMatch.ResultId2).Result.Result;
            Console.WriteLine($"{aMatch.Name} details: Segments:{details.Segments}, of which larger than 5cM: {details?.FFCMData.Count(s => s.Cm > 5)}");
        }
    }
}
