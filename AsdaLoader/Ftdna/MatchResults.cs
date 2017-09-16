using System;
using System.Collections.Generic;

namespace AsdaLoader.Ftdna
{
    public class MatchResults
    {
        public int Count { get; set; }
        public IEnumerable<Match> Data { get; set; }
    }
}
