using CsvHelper.Configuration;

namespace AsdaLoader.Ftdna
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

	public sealed class ChromosomeSegmentMap : CsvClassMap<ChromosomeSegment>
	{
        public ChromosomeSegmentMap()
		{
            Map(m => m.Name).Name("NAME");
			Map(m => m.MatchName).Name("MATCHNAME");
            Map(m => m.Chromosome).Name("CHROMOSOME");
            Map(m => m.StartLocation).Name("START LOCATION");
            Map(m => m.EndLocation).Name("END LOCATION");
            Map(m => m.Centimorgans).Name("CENTIMORGANS");
            Map(m => m.MatchingSnps).Name("MATCHING SNPS");
		}
	}
}