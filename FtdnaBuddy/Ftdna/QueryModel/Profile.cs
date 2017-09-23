using System;
using System.Collections.Generic;
using System.Linq;

namespace FtdnaBuddy.Ftdna.QueryModel
{
    public class Profile
    {
        private readonly IList<Kit> _matches = new List<Kit>();

        public Profile(string kitNumber)
        {
            KitNumber = kitNumber;
        }

        public string KitNumber { get; }

        public DateTime LastUpdated { get; set; }

        public int MatchCount => _matches.Count;

        public IEnumerable<Kit> Matches => _matches;

        public void AddMatches(IList<Kit> matches)
        {
            foreach (var kit in matches)
            {
                AddMatch(kit);
            }
        }

        public void AddMatch(Kit match)
        {
            bool exists = _matches.Any(m => m.ResultId2 == match.ResultId2);
            if (exists)
                return;

            _matches.Add(match);
            match.Profile = this;
        }

        public IEnumerable<Kit> ListMatchesWithoutSegments()
        {
            return _matches.Where(m => m.SegmentMatchCount == 0);
        }
    }

    public class Kit : IKitData, IKitIdentity
    {
        private readonly IList<Kit> _icw = new List<Kit>();

        private readonly IList<SegmentMatch> _segment = new List<SegmentMatch>();

        public string AboutMe { get; set; }

        public string Email { get; set; }

        public string FamilyTreeUrl { get; set; }

        public string FirstName { get; set; }

        public bool HasFamilyTree { get; set; }

        public bool IsXMatch { get; set; }

        public string KitEncrypted { get; set; }

        public string LastName { get; set; }

        public DateTime LastUpdated { get; set; }

        public double LongestCentimorgans { get; set; }

        public bool MatchKitRelease { get; set; }

        public string MatchPersonName { get; set; }

        public string MaternalAncestorName { get; set; }

        public string MiddleName { get; set; }

        public string MtDNAMarkers { get; set; }

        public string MtHaplo { get; set; }

        public string Name { get; set; }

        public string Note { get; set; }

        public string PaternalAncestorName { get; set; }

        public string Prefix { get; set; }

        public DateTime RbDate { get; set; }

        public int RelationsGroupId { get; set; }

        public int Relationship { get; set; }

        public int RelationshipDistance { get; set; }

        public int? RelationshipId { get; set; }

        public string RelationshipRange { get; set; }

        public Guid ResultGuid { get; set; }

        public string ResultId1 { get; set; }

        public string ResultId2 { get; set; }

        public Sex Sex { get; set; }

        public string SuggestedRelationship { get; set; }

        public double TotalCM { get; set; }

        public bool ThirdParty { get; set; }

        public string UserSurnames { get; set; }

        public string YDNAMarkers { get; set; }

        public string YHaplo { get; set; }

        public IEnumerable<Kit> InCommonWith => _icw;

        public IEnumerable<SegmentMatch> SegmentMatches => _segment;

        public int SegmentMatchCount => _segment.Count;

        internal Profile Profile { get; set; }

        internal Kit AddInCommonWith(IKitIdentity match)
        {
            Kit existingMatch = Profile.Matches.SingleOrDefault(m =>
                                     m.ResultId2 == match.ResultId2);
            if (existingMatch == null)
                throw new ICWNotFoundAsAMatchException("ICW must be a profile match");

            Kit icwAlreadyAdded = _icw.SingleOrDefault(i => i.ResultId2 == match.ResultId2);
            if (icwAlreadyAdded != null)
                return icwAlreadyAdded;

            _icw.Add(existingMatch);
            existingMatch._icw.Add(this);
            return existingMatch;
        }

        internal void AddSegmentMatch(SegmentMatch segment)
        {
            AddSegmentMatch(segment.Chromosome, segment.Start, segment.End, segment.Snps);
        }

        internal void AddSegmentMatch(string chromosome, long start, long end,
                                      int snps)
        {
            bool exists = _segment.Any(s => s.Chromosome == chromosome
                                       && s.Start == start && s.End == end
                                       && s.Snps == snps);
            if (exists)
                return;

            var segment = new SegmentMatch
            {
                Chromosome = chromosome,
                Start = start,
                End = end,
                Snps = snps
            };
            _segment.Add(segment);
        }
    }

    public class SegmentMatch
    {
        public string Chromosome { get; set; }

        public long Start { get; set; }

        public long End { get; set; }

        public int Snps { get; set; }
    }

    public enum Sex
    {
        Female,
        Male
    }

    public interface IKitData
    {
        string AboutMe { get; set; }

        string Email { get; set; }

        string FamilyTreeUrl { get; set; }

        string FirstName { get; set; }

        bool HasFamilyTree { get; set; }

        bool IsXMatch { get; set; }

        string KitEncrypted { get; set; }

        string LastName { get; set; }

        double LongestCentimorgans { get; set; }

        bool MatchKitRelease { get; set; }

        string MatchPersonName { get; set; }

        string MaternalAncestorName { get; set; }

        string MiddleName { get; set; }

        string MtDNAMarkers { get; set; }

        string MtHaplo { get; set; }

        string Name { get; set; }

        string Note { get; set; }

        string PaternalAncestorName { get; set; }

        string Prefix { get; set; }

        DateTime RbDate { get; set; }

        int RelationsGroupId { get; set; }

        int Relationship { get; set; }

        int RelationshipDistance { get; set; }

        int? RelationshipId { get; set; }

        string RelationshipRange { get; set; }

        Guid ResultGuid { get; set; }

        string ResultId1 { get; set; }

        string ResultId2 { get; set; }

        Sex Sex { get; set; }

        string SuggestedRelationship { get; set; }

        double TotalCM { get; set; }

        bool ThirdParty { get; set; }

        string UserSurnames { get; set; }

        string YDNAMarkers { get; set; }

        string YHaplo { get; set; }
    }

    public class ICWNotFoundAsAMatchException : Exception
    {
        public ICWNotFoundAsAMatchException(string message) :
            base(message)
        {
            
        }
    }
}