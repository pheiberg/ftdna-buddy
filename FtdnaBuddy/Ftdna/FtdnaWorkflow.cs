﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Regex = System.Text.RegularExpressions.Regex;
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
            var updatedKits = UpdateMatches(profile);
            UpdateSegments(updatedKits);
            AddInCommonWith(updatedKits);
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

        private IEnumerable<Kit> UpdateMatches(Profile profile)
        {
            if (DateTime.Now - profile.LastUpdated < TimeSpan.FromDays(1))
            {
                _logger.LogInfo("All matches are already up to date");
                return Enumerable.Empty<Kit>();
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
            return kits;
        }

        private void UpdateSegments(IEnumerable<Kit> kits)
        {
            _logger.LogInfo("Fetching segment information...");
            var segments = _service.ListChromosomeSegmentsByMatchName().Result.ToArray();
            var segmentMap = segments.ToLookup(s => NormalizeName(s.MatchName));

            foreach(var kit in kits)
            {
                var kitSegments = segmentMap[NormalizeName(kit.Name)];

                foreach (var kitSegment in kitSegments)
                {
                    kit.AddSegmentMatch(kitSegment.Chromosome, 
                                       kitSegment.StartLocation,
                                       kitSegment.EndLocation,
                                       kitSegment.MatchingSnps);
                }

                if(!kitSegments.Any())
                {
                    _logger.LogInfo($"Warning: No segments could be found for {kit.Name}");
                }
            }

            _logger.LogInfo($"Done fetching {segments.Length} pieces of segment information");
        }

        private void AddInCommonWith(IEnumerable<Kit> kits)
        {
            _logger.LogInfo("Fetching ICW information...");
			foreach (var kit in kits)
			{
				var icws = _service.ListInCommonWith(kit).Result;

				foreach (var icw in icws)
				{
					kit.AddInCommonWith(icw);
				}
			}
            _logger.LogInfo("Done fetching ICW");
        }

        private string NormalizeName(string name)
        {
            return Regex.Replace(name, "^[\\w]", "").ToLower();
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
