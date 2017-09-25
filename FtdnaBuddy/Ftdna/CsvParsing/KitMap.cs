using CsvHelper.Configuration;
using FtdnaBuddy.Ftdna.QueryModel;

namespace FtdnaBuddy.Ftdna.CsvParsing
{
    public sealed class KitMap : ClassMap<Kit>
    {
        public KitMap()
        {
            Map(m => m.Name).Name("Full Name");
            Map(m => m.RbDate).Name("Match Date");
            Map(m => m.RelationshipRange).Name("Relationship Range");
            Map(m => m.SuggestedRelationship).Name("Suggested Relationship");
            Map(m => m.TotalCM).Name("Shared cM");
            Map(m => m.LongestCentimorgans).Name("Longest Block");
            Map(m => m.Relationship).Name("Known Relationship");
            Map(m => m.Email).Name("E - mail");
            Map(m => m.UserSurnames).Name("Ancestral");
            Map(m => m.YHaplo).Name("YDNA Haplogroup");
            Map(m => m.MtHaplo).Name("mtDNA Haplogroup");
            Map(m => m.ResultId2).Name("ResultID2");
            Map(m => m.Note).Name("Notes");
            Map(m => m.MatchPersonName).Name("Name");
        }
    }
}
