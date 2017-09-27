using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Regex = System.Text.RegularExpressions.Regex;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using FtdnaBuddy.Ftdna.Model;
using FtdnaBuddy.Ftdna.QueryModel;
using FtdnaBuddy.Ftdna.Serialization;

namespace FtdnaBuddy.Ftdna
{
    public class FtdnaWorkflow
    {
	    private readonly IDnaDataService _service;
	    private readonly ILogger _logger;

        public FtdnaWorkflow(IDnaDataService service, ILogger logger)
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
            GenerateCsv(profile);
        }

        private LoginResult StartSession(string kitNumber, string password)
        {
            _logger.LogInfo($"Logging in as {kitNumber}");
            var loginResult = _service.Login(kitNumber, password);

            if (loginResult.ErrorMessage == null)
            {
                _logger.LogInfo("Login successful");
				return loginResult;
            }

            _logger.LogError(loginResult.ErrorMessage);
            if(loginResult.IsLockedOut)
            {
                _logger.LogError("Account has been locked out");
            }

            throw new Exception("Login failed");
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

            var matches = matchTask.Result.ToArray();
            var kits = matches.Select(m => ModelBuilder.BuildKit(m, DateTime.Now)).ToArray();
            profile.AddMatches(kits);
            profile.LastUpdated = DateTime.Now;

            _logger.LogInfo($"Done fetching {matches.Length} matches");
            return kits;
        }

        private void UpdateSegments(IEnumerable<Kit> kits)
        {
            _logger.LogInfo("Fetching segment information...");
            var segments = _service.ListChromosomeSegmentsByMatchName().Result.ToArray();
            var segmentMap = segments.ToLookup(s => NormalizeName(s.MatchName));

            foreach(var kit in kits)
            {
                UpdateSegment(segmentMap, kit);
            }

            _logger.LogInfo("Done processing segment information");
        }

        private void UpdateSegment(ILookup<string, ChromosomeSegment> segmentMap, Kit kit)
        {
			var kitSegments = segmentMap[NormalizeName(kit.Name)];

			if (!kitSegments.Any())
			{
				_logger.LogInfo($"Warning: No segments could be found for {kit.Name}, falling back to fetching details...");
				var details = _service.GetMatchDetails(kit).Result;
				kitSegments = details.FFCMData.Select(s => new ChromosomeSegment
				{
					Chromosome = s.Chromosome,
					StartLocation = s.P1,
					EndLocation = s.P2,
					Centimorgans = s.Cm,
					MatchingSnps = s.Snps
				});
				_logger.LogInfo($"Fetching {kitSegments.Count()} segments from details");
			}

			foreach (var kitSegment in kitSegments)
			{
				kit.AddSegmentMatch(kitSegment.Chromosome,
								   kitSegment.StartLocation,
								   kitSegment.EndLocation,
								   kitSegment.MatchingSnps);
			}
        }

		private string NormalizeName(string name)
		{
			return Regex.Replace(name, "[^\\w]", "").ToLower();
		}

        private void AddInCommonWith(IEnumerable<Kit> kits)
        {
            int kitCount = kits.Count();
            _logger.LogInfo("Fetching ICW information...");
            ConsoleProgressBar progress = _logger.ShowProgress(kitCount);
            int kitIndex = 1;
			foreach (var kit in kits)
			{
				var icws = _service.ListInCommonWith(kit).Result;

				foreach (var icw in icws)
				{
					kit.AddInCommonWith(icw);
				}
                progress.Draw(kitIndex++);
			}
            _logger.LogInfo("Done fetching ICW");
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

        private void GenerateCsv(Profile profile)
        {
            GenerateFamilyFinderCsv(profile);
			_logger.LogInfo("Created Family Finder Matches CSV");   
        }

        private void GenerateFamilyFinderCsv(Profile profile)
        {
	        const string header = "Full Name,Match Date,Relationship Range,Suggested Relationship,Shared cM," 
	                              + "Longest Block,Known Relationship,E - mail,Ancestral,YDNA Haplogroup,mtDNA " 
	                              + "Haplogroup,ResultID2,Notes,Name";
	        
	        var fileName = $"{profile.KitNumber}_Family_Finder_Matches.csv";
            using(var file = File.OpenWrite(fileName))
	        using(var writer = new StreamWriter(file))
			using(var csvWriter = new CsvWriter(writer))
			{
				writer.WriteLine(header);
				
				foreach (var kit in profile.Matches)
				{
					WriteKitCsvLine(csvWriter, kit);
				}
            }
        }

	    private static void WriteKitCsvLine(IWriter csvWriter, IKitData kit)
	    {
		    csvWriter.WriteField(kit.Name);
		    csvWriter.WriteField(kit.RbDate.ToString("MM/dd/yyyy"));
		    csvWriter.WriteField(kit.RelationshipRange);
		    csvWriter.WriteField(kit.SuggestedRelationship);
		    csvWriter.WriteField(kit.TotalCM);
		    csvWriter.WriteField(kit.LongestCentimorgans);
		    csvWriter.WriteField(kit.Relationship);
		    csvWriter.WriteField(kit.Email);
		    csvWriter.WriteField(kit.UserSurnames);
		    csvWriter.WriteField(kit.YHaplo);
		    csvWriter.WriteField(kit.MtHaplo);
		    csvWriter.WriteField(kit.ResultId2);
		    csvWriter.WriteField(kit.Note);
		    csvWriter.WriteField(kit.Name);
		    csvWriter.NextRecord();
	    }

	    private static string CreateFileName(string kitNumber)
		{
			return $"{kitNumber}.json";
		}
    }
}
