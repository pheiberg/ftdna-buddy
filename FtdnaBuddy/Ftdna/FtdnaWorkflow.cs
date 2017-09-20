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
            if (!File.Exists($"{kitNumber}.json"))
            {
                return new Profile(kitNumber);
            }

            _logger.LogInfo("Loading existing profile...");
            var json = File.ReadAllText($"{kitNumber}.json");
            var serializer = new ProfileJsonSerializer();
            return serializer.Deserialize(json);
        }

        private void StoreProfile(Profile profile)
        {
            _logger.LogInfo("Saving profile...");
            var serializer = new ProfileJsonSerializer();
            string serialized = serializer.Serialize(profile);
            File.WriteAllText($"{profile.KitNumber}.json", serialized);
            _logger.LogInfo("Profile was saved");
        }
    }
}
