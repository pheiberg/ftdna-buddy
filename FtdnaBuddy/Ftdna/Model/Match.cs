using System;
using FtdnaBuddy.Ftdna.QueryModel;

namespace FtdnaBuddy.Ftdna.Model
{
    public class Match : IKitData, IKitIdentity
    {
        public string AboutMe { get; set; }

        public string BucketType { get; set; }

        public int ContactId { get; set; }

        public DateTime CreateDT { get; set; }

        public string Email { get; set; }

        public string FamilyTreeUrl { get; set; }

        public bool Female { get; set; }

        public int FF_XSharedSegments { get; set; }

        public int? FFDataTypeId { get; set; }

        public int? FFRelationshipGroupId { get; set; }

        public string FirstName { get; set; }

        public int? FtrRelationshipGroupId { get; set; }

        public string FtrRelationshipName { get; set; }

        public bool HasFamilyTree { get; set; }

        public bool IsXMatch { get; set; }

        public string KitEncrypted { get; set; }

        public string KitNum { get; set;}

        public string LastName { get; set; }

        public double LongestCentimorgans { get; set; }

        public bool MatchKitRelease { get; set; }

        public string MatchPersonName { get; set; }

        public bool MatchSummary_FFBack { get; set; }

        public string MaternalAncestorName { get; set; }

        public string MiddleName { get; set; }

        public string MtDNAMarkers { get; set; }

        public string MtHaplo { get; set; }

        public bool MyKitRelease { get; set; }

        public bool MySummary_FFBack { get; set; }

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

        public int Rownum { get; set; }

        public Sex Sex 
        { 
            get { return Female ? Sex.Female : Sex.Male; } 
            set { Female = value == Sex.Female; } 
        }

        public string Suffix { get; set; }

        public string SuggestedRelationship { get; set; }

        public bool ThirdParty { get; set; }

        public double TotalCM { get; set; }

        public string UserSurnames { get; set; }

        public string YDNAMarkers { get; set; }

        public string YHaplo { get; set; }
     }
}