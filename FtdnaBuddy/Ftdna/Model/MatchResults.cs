using System.Collections.Generic;

namespace AsdaLoader.Ftdna.Model
{
    public class MatchResults
    {
        public int Count { get; set; }

        public int CountBoth { get; set; }

        public int CountMaternal { get; set; }

        public int CountPaternal { get; set; }

        public IEnumerable<Match> Data { get; set; }

        public bool IsFreeTransfer { get; set; }
    }
}