using System;
using System.IO;
using System.Linq;
using FtdnaBuddy.Ftdna.QueryModel;
using Newtonsoft.Json;

namespace FtdnaBuddy.Ftdna.Serialization
{
    public class ProfileJsonSerializer
    {
        readonly JsonSerializer _serializer = new JsonSerializer();

        public string Serialize(Profile profile)
        {
            var flatProfile = new FlatProfile(profile);
            return JsonConvert.SerializeObject(flatProfile);
        }

		public void Serialize(Profile profile, StreamWriter stream)
		{
			var flatProfile = new FlatProfile(profile);
            var jsonWriter = new JsonTextWriter(stream);
            _serializer.Serialize(jsonWriter, flatProfile);
		}

		public Profile Deserialize(string json)
		{
            var flatProfile = JsonConvert.DeserializeObject<FlatProfile>(json);
            return flatProfile.ToProfile();
		}

		public Profile Deserialize(StreamReader stream)
		{
            var jsonReader = new JsonTextReader(stream);
            var flatProfile = _serializer.Deserialize<FlatProfile>(jsonReader);
			return flatProfile.ToProfile();
		}

        private class FlatProfile
        {
            public FlatProfile()
            {

            }

            public FlatProfile(Profile profile)
            {
                KitNumber = profile.KitNumber;
				Matches = profile.Matches.Select(m => new FlatKit(m)).ToArray();
                LastUpdated = profile.LastUpdated;
            }

            public string KitNumber { get; set; }

            public DateTime LastUpdated { get; set; }

            public FlatKit[] Matches { get; set; }

            public Profile ToProfile()
            {
                var profile = new Profile(KitNumber)
                {
                    LastUpdated = LastUpdated
                };
                var kits = Matches.Select(fp => fp.ToUnconnectedKit()).ToArray();
				var kitMap = kits.ToDictionary(k => k.ResultId2);

				foreach (var kit in kits)
				{
					profile.AddMatch(kit);
				}

				var icwIds = Matches.SelectMany(m => m.IcwIds.Select(i => new Tuple<string, string>(m.ResultId2, i)));
				foreach (var icw in icwIds)
				{
					var kit1 = kitMap[icw.Item1];
					var kit2 = kitMap[icw.Item2];
					kit1.AddInCommonWith(kit2);
				}

                return profile;
            }
        }

        private class FlatKit : IKitData
        {
            public FlatKit()
            {

            }

            public FlatKit(Kit kit)
            {
                ResultId1 = kit.ResultId1;
                ResultId2 = kit.ResultId2;
                Name = kit.Name;
                UserSurnames = kit.UserSurnames;
                Sex = kit.Sex;

                IcwIds = kit.InCommonWith.Select(k => k.ResultId2).ToArray();
                SegmentMatches = kit.SegmentMatches.ToArray();
            }

			public string AboutMe { get; set; }

			public string Email { get; set; }

			public string FamilyTreeUrl { get; set; }

			public string FirstName { get; set; }

			public bool HasFamilyTree { get; set; }

			public bool IsXMatch { get; set; }

			public string KitEncrypted { get; set; }

			public string LastName { get; set; }

			public DateTime LastUpdated { get; set; }

			public double LongestCentimorgans { get; set; }

			public bool MatchKitRelease { get; set; }

			public string MatchPersonName { get; set; }

			public string MaternalAncestorName { get; set; }

			public string MiddleName { get; set; }

			public string MtDNAMarkers { get; set; }

			public string MtHaplo { get; set; }

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

			public Sex Sex { get; set; }

			public string SuggestedRelationship { get; set; }

			public double TotalCM { get; set; }

			public bool ThirdParty { get; set; }

			public string UserSurnames { get; set; }

			public string YDNAMarkers { get; set; }

			public string YHaplo { get; set; }

            public string[] IcwIds { get; set; }

            public SegmentMatch[] SegmentMatches { get; set; }

            public Kit ToUnconnectedKit()
            {
                Kit kit = ModelBuilder.BuildKit(this);
                kit.LastUpdated = LastUpdated;
                foreach(var segment in SegmentMatches)
                {
                    kit.AddSegmentMatch(segment);
                }
                return kit;
            }

        }
    }
}
