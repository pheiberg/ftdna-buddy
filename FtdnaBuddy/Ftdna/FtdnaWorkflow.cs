using System;
using System.IO;
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
