using System;
namespace AsdaLoader.Ftdna
{
    public class Match
    {
        public string AboutMe { get; set; }

        public string BucketType { get; set; }

        public int ContactId { get; set; }

        public DateTime CreateDT { get; set; }

        public string Email { get; set; }

        public string FamilyTreeUrl { get; set; }

        public bool Female { get; set; }

        public string FirstName { get; set; }

        public bool HasFamilyTree { get; set; }

        public bool IsXMatch { get; set; }

        public string KitEncrypted { get; set; }

        public string LastName { get; set; }

        public double LongestCentimorgans { get; set; }

        public string MatchPersonName { get; set; }

        public string MiddleName { get; set; }

        public DateTime RbDate { get; set; }

        public Guid ResultGuid { get; set; }

        public string ResultId1 { get; set; }

        public string ResultId2 { get; set; }

        public double TotalCM { get; set; }
    }
}