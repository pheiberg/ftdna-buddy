using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FtdnaBuddy.Ftdna.Model;
using FtdnaBuddy.Ftdna.QueryModel;
using FtdnaBuddy.Ftdna.Serialization;

namespace FtdnaBuddy.Ftdna
{
    public class FtdnaWorkflow
    {
        readonly FtdnaService _service;
        readonly ILogger _logger;

        public FtdnaWorkflow(FtdnaService service, ILogger logger)
        {
            _service = service;
            _logger = logger;
        }

        public void Execute(string kitNumber, string password)
        {
            var user = StartSession(kitNumber, password);
            var profile = CreateProfile(kitNumber);
            UpdateMatches(profile);
            StoreProfile(profile);
        }

        private FtdnaUser StartSession(string kitNumber, string password)
        {
            _logger.LogInfo($"Logging in as {kitNumber}");
            FtdnaUser ftdnaUser = _service.Login(kitNumber, password);

            if (ftdnaUser != null)
            {
                _logger.LogInfo($"Login successful");
            }
            else
            {
                _logger.LogError($"Login failed");
                throw new Exception("Login failed");
            }
            return ftdnaUser;
        }

        private Profile CreateProfile(string kitNumber)
        {
            string fileName = CreateFileName(kitNumber);
            if (!File.Exists(fileName))
            {
                return new Profile(kitNumber);
            }

            _logger.LogInfo("Loading existing profile...");

			var serializer = new ProfileJsonSerializer();
            using (var file = File.OpenText(fileName))
            {
				return serializer.Deserialize(file);
            }
        }

        private void UpdateMatches(Profile profile)
        {
            if (DateTime.Now - profile.LastUpdated < TimeSpan.FromDays(1))
            {
                _logger.LogInfo("All matches are already up to date");
                return;
            }

            Task<IEnumerable<Match>> matchTask;
            if (profile.MatchCount == 0)
            {
                _logger.LogInfo("Fetching all matches...");
                matchTask = _service.ListAllMatches();
            }
            else
            {
                _logger.LogInfo($"Fetching new matches since {profile.LastUpdated.ToShortDateString()}");
                matchTask = _service.ListNewMatches(profile.LastUpdated);
            }

            var matches = matchTask.Result;
            var kits = matches.Select(m => ModelBuilder.BuildKit(m, DateTime.Now));
            profile.AddMatches(kits);
            profile.LastUpdated = DateTime.Now;

            _logger.LogInfo($"Done fetching {matches.Count()} matches");
        }

        private void StoreProfile(Profile profile)
        {
            string fileName = CreateFileName(profile.KitNumber);
            _logger.LogInfo("Saving profile...");
            var serializer = new ProfileJsonSerializer();
            using (var file = File.OpenWrite(fileName))
            using (var writer = new StreamWriter(file))
            {
                serializer.Serialize(profile, writer);
            }

            _logger.LogInfo("Profile was saved");
        }

		private static string CreateFileName(string kitNumber)
		{
			return $"{kitNumber}.json";
		}
    }
}
