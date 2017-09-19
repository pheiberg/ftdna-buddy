using System;
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

		public Profile Deserialize(string json)
		{
            var flatProfile = JsonConvert.DeserializeObject<FlatProfile>(json);
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
            }

            public string KitNumber { get; set; }

            public FlatKit[] Matches { get; set; }

            public Profile ToProfile()
            {
				var profile = new Profile(KitNumber);
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

        private class FlatKit
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
                IsMaternal = kit.IsMaternal;
                IsPaternal = kit.IsPaternal;
                Sex = kit.Sex;

                IcwIds = kit.InCommonWith.Select(k => k.ResultId2).ToArray();
                SegmentMatches = kit.SegmentMatches.ToArray();
            }

			public string ResultId1 { get; set; }

			public string ResultId2 { get; set; }

			public string Name { get; set; }

			public string UserSurnames { get; set; }

			public bool IsMaternal { get; set; }

			public bool IsPaternal { get; set; }

			public Sex Sex { get; set; }

            public string[] IcwIds { get; set; }

            public SegmentMatch[] SegmentMatches { get; set; }

            public Kit ToUnconnectedKit()
            {
                Kit kit = new Kit
                {
                    ResultId1 = ResultId1,
                    ResultId2 = ResultId2,
                    Name = Name,
                    UserSurnames = UserSurnames,
                    IsMaternal = IsMaternal,
                    IsPaternal = IsPaternal,
                    Sex = Sex
                };
                foreach(var segment in SegmentMatches)
                {
                    kit.AddSegmentMatch(segment);
                }
                return kit;
            }

        }
    }
}
