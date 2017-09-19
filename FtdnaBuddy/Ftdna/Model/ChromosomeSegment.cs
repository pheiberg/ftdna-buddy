namespace FtdnaBuddy.Ftdna.Model
{
    public class ChromosomeSegment
    {
        public string Name { get; set; }

        public string MatchName { get; set; }

        public string Chromosome { get; set; }

        public long StartLocation { get; set; }

        public long EndLocation { get; set; }

        public double Centimorgans { get; set; }

        public int MatchingSnps { get; set; }
    }
}