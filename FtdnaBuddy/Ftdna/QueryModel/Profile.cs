using System.Collections.Generic;
using System.Linq;

namespace FtdnaBuddy.Ftdna.QueryModel
{
    public class Profile
    {
        private readonly IList<Kit> _matches = new List<Kit>();
        public IEnumerable<Kit> Matches => _matches;
        public string KitNumber { get; }

        public Profile(string kitNumber)
        {
            KitNumber = kitNumber;
        }

        public void AddMatch(Kit match)
        {
            bool exists = _matches.Any(m => m.ResultId2 == match.ResultId2);
            if (exists)
                return;

            _matches.Add(match);
        }

        public IEnumerable<Kit> ListMatchesWithoutSegments()
        {
            return _matches.Where(m => m.SegmentMatchCount == 0);
        }
    }

    public class Kit 
    {
        private readonly IList<Kit> _icw = new List<Kit>();

        private readonly IList<SegmentMatch> _segment = new List<SegmentMatch>();

		public string ResultId1 { get; set; }

		public string ResultId2 { get; set; }

        public string Name { get; set; }

        public string UserSurnames { get; set; }

        public bool IsMaternal { get; set; }

        public bool IsPaternal { get; set; }

        public Sex Sex { get; set; }

        public IEnumerable<Kit> InCommonWith => _icw;

        public IEnumerable<SegmentMatch> SegmentMatches => _segment;

        public int SegmentMatchCount => _segment.Count;

        internal void AddInCommonWith(Kit match)
        {
            bool exists = _icw.Any(i => i.ResultId2 == match.ResultId2);
            if (exists)
                return;
            
            _icw.Add(match);
            match._icw.Add(this);
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
}