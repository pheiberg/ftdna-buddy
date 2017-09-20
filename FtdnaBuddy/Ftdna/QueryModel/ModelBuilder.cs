using System;
using System.Collections.Generic;
using System.Linq;
using FtdnaBuddy.Ftdna.Model;

namespace FtdnaBuddy.Ftdna.QueryModel
{
    public class ModelBuilder
    {
        public static void BuildModel(string kitNumber, 
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
                var kit = BuildKit(g.Match);
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

        public static Kit BuildKit(IKitData match, DateTime? lastUpdated = null)
        {
            Kit kit = new Kit
            {
                AboutMe = match.AboutMe,
                Email = match.Email,
                FamilyTreeUrl = match.FamilyTreeUrl,
                FirstName = match.FirstName,
                HasFamilyTree = match.HasFamilyTree,
                IsXMatch = match.IsXMatch,
                KitEncrypted = match.KitEncrypted,
                LastName = match.LastName,
                LongestCentimorgans = match.LongestCentimorgans,
                MatchKitRelease = match.MatchKitRelease,
                MatchPersonName = match.MatchPersonName,
                MaternalAncestorName = match.MaternalAncestorName,
                MiddleName = match.MiddleName,
                MtDNAMarkers = match.MtDNAMarkers,
                MtHaplo = match.MtHaplo,
                Name = match.Name,
                Note = match.Note,
                PaternalAncestorName = match.PaternalAncestorName,
                Prefix = match.Prefix,
                RbDate = match.RbDate,
                RelationsGroupId = match.RelationsGroupId,
                Relationship = match.Relationship,
                RelationshipDistance = match.RelationshipDistance,
                RelationshipRange = match.RelationshipRange,
                ResultId1 = match.ResultId1,
                ResultId2 = match.ResultId2,
                Sex = match.Sex,
                SuggestedRelationship = match.SuggestedRelationship,
                TotalCM = match.TotalCM,
                ThirdParty = match.ThirdParty,
                UserSurnames = match.UserSurnames,
                YDNAMarkers = match.YDNAMarkers,
                YHaplo = match.YHaplo
            };

            if(lastUpdated.HasValue)
            {
                kit.LastUpdated = lastUpdated.Value;
            }
            return kit;
        }
    }
}