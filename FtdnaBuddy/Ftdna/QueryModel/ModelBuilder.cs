using System.Collections.Generic;
using System.Linq;
using FtdnaBuddy.Ftdna.Model;

namespace FtdnaBuddy.Ftdna.QueryModel
{
    public class ModelBuilder
    {
        public void BuildModel(string kitNumber, 
                               IEnumerable<Match> matches, 
                               IEnumerable<ChromosomeSegment> segments,
                               IDictionary<string ,IEnumerable<Match>> icw)
        {
            var groupedSegments = 
                from m in matches
                join s in segments
                on m.Name equals s.MatchName into segmentGroup
                select new { Match = m, Segments = segmentGroup.ToArray() };

            var profile = new Profile(kitNumber);

            foreach(var g in groupedSegments)
            {
                var kit = new Kit
                {
                    Name = g.Match.Name
                };
                foreach(var segment in g.Segments)
                {
                    kit.AddSegmentMatch(segment.Chromosome, 
                                        segment.StartLocation,
                                        segment.EndLocation, 
                                        segment.MatchingSnps);
                }
            }

            foreach(var kit in icw)
            {
                var kit1 = profile.Matches.Single(m => m.ResultId2 == kit.Key);
                foreach(var commonMatch in kit.Value)
                {
                    var kit2 = profile.Matches.Single(m => m.ResultId2 == commonMatch.ResultId2);
                    kit1.AddInCommonWith(kit2);
                }
            }
        }
    }
}