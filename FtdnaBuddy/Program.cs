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

            try
            {
                var user = service.Login(args[0], args[1]);
                TestFunctions(service);
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

        static void TestFunctions(FtdnaService service)
        {
            DateTime startDate = DateTime.Now.AddDays(-7);
            var newMatchData = service.ListNewMatches(startDate).Result;
            Console.WriteLine($"New matches since: {startDate.ToShortDateString()}");
			foreach (var match in newMatchData)
			{
				Console.WriteLine($"{match.MatchPersonName} - Tot {match.TotalCM} cM - LB {match.LongestCentimorgans} cM");
			}
            var matchData = service.ListAllMatches().Result;
            foreach (var match in matchData)
            {
                Console.WriteLine($"{match.MatchPersonName} - Tot {match.TotalCM} cM - LB {match.LongestCentimorgans} cM");
            }
            var segments = service.ListChromosomeSegmentsByMatchName().Result;
            foreach (var segment in segments)
            {
                Console.WriteLine($"{segment.MatchName} - Chr {segment.Chromosome} - {segment.Centimorgans} cM");
            }
            var aMatch = matchData.First();
            var inCommonWith = service.ListInCommonWith(aMatch).Result;
            foreach (var icw in inCommonWith)
            {
                Console.WriteLine($"ICW {aMatch.Name} - {icw.Name}");
            }
            var details = service.GetMatchDetails(aMatch).Result;
            Console.WriteLine($"{aMatch.Name} details: Segments:{details.Segments}, of which larger than 5cM: {details?.FFCMData.Count(s => s.Cm > 5)}");
        }
    }
}
