using FtdnaBuddy.Ftdna.Model;
using CsvHelper.Configuration;

namespace FtdnaBuddy.Ftdna.CsvParsing
{
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
