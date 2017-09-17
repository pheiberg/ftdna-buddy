using System;
using System.Collections.Generic;

namespace AsdaLoader.Ftdna
{
    public class MatchDetailsResult
    {
        public MatchDetails Result { get; set; }

        public bool Success { get; set; }

        public string Error { get; set; }
    }

    public class MatchDetails
    {
        public string ImgUrl { get; set; }

        public string IconImgURL { get; set; }

        public bool Selected { get; set; }

        public string Resultid2 { get; set; }

        public double Cm { get; set; }

        public int Segments { get; set; }

        public string Notes { get; set; }

        public IEnumerable<MatchDetailsSurname> FFSurnames { get; set; }

        public IEnumerable<MatchDetailsCMData> FFCMData { get; set; }

        public bool Female { get; set; }

        public string Predicted { get; set; }

        public string Range { get; set; }
    }

    public class MatchDetailsSurname
    {
        public string Name { get; set; }

        public string Country { get; set; }

        public bool Match { get; set; }
    }

	public class MatchDetailsCMData
	{
		public string Chromosome { get; set; }

		public double Cm { get; set; }

		public int Snps { get; set; }

        public long P1 { get; set; }

        public long P2 { get; set; }
	}
}
