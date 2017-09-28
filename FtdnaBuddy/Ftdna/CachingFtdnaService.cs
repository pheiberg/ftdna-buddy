using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FtdnaBuddy.Ftdna.Model;
using FtdnaBuddy.Ftdna.QueryModel;
using Newtonsoft.Json;

namespace FtdnaBuddy.Ftdna
{
	public class CachingDataService : IDnaDataService
	{
        private const string CacheDirectory = "__cache";
        readonly IDnaDataService _service;
		readonly JsonSerializer _serializer = new JsonSerializer();
        FtdnaUser _user;

		public CachingDataService(IDnaDataService service)
		{
			_service = service;
		}

		public async Task<MatchDetails> GetMatchDetails(IKitIdentity match)
        {
            if (TryGetStoredResult("GetMatchDetails", out MatchDetails cached, match.ResultId2))
                return cached;

            var result = await _service.GetMatchDetails(match);
            StoreResult("GetMatchDetails", result, match.ResultId2);

            return result;
        }

        public async Task<IEnumerable<Match>> ListAllMatches(int pageSize = FtdnaConnector.DefaultPageSize)
		{
			if (TryGetStoredResult("ListAllMatches", out IEnumerable<Match> cached))
				return cached;

            var result = await _service.ListAllMatches();
            StoreResult("ListAllMatches", result);
            return result;
		}

		public async Task<IEnumerable<ChromosomeSegment>> ListChromosomeSegmentsByMatchName()
		{
			if (TryGetStoredResult("ListChromosomeSegmentsByMatchName", out IEnumerable<ChromosomeSegment> cached))
				return cached;
            
            var result = await _service.ListChromosomeSegmentsByMatchName();
            StoreResult("ListChromosomeSegmentsByMatchName", result);
            return result;
		}

		public async Task<IEnumerable<Match>> ListInCommonWith(IKitIdentity match)
		{
			if (TryGetStoredResult("ListInCommonWith", out IEnumerable<Match> cached, match.ResultId2))
				return cached;

            var result = await _service.ListInCommonWith(match);
            StoreResult("ListInCommonWith", result, match.ResultId2);
            return result;
		}

		public async Task<IEnumerable<Match>> ListNewMatches(DateTime startDate)
		{
            string dateString = startDate.ToString("yyyyMMdd");
            if (TryGetStoredResult("ListNewMatches", out IEnumerable<Match> cached, dateString))
				return cached;

            var result = await _service.ListNewMatches(startDate);
            StoreResult("ListNewMatches", result, dateString);
            return result;
		}

		public LoginResult Login(string kitNumber, string password)
		{
            var result = _service.Login(kitNumber, password);
            _user = result.User;
			return result;
		}

		private void StoreResult<T>(string methodKey, T result, string parameterKeys = null)
        {
            EnsureDirectoryExists(CacheDirectory);

            string fileName = GetFileName(methodKey, parameterKeys);
            using (var file = File.OpenWrite(Path.Combine(CacheDirectory, fileName)))
            using (var writer = new StreamWriter(file))
            {
                _serializer.Serialize(writer, result);
            }
        }

        private void EnsureDirectoryExists(string directoryName)
        {
            if(!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
        }

        private bool TryGetStoredResult<T>(string methodKey, out T result, string parameterKeys = null)
        {
            string fileName = GetFileName(methodKey, parameterKeys);
            string filePath = Path.Combine(CacheDirectory, fileName);
            if (!File.Exists(filePath))
            {
                result = default(T);
                return false;
            }

			using (var reader = File.OpenText(filePath))
            using(var jsonReader = new JsonTextReader(reader))
			{
				result = _serializer.Deserialize<T>(jsonReader);
			}

            return true;
        }

		private string GetFileName(string methodKey, string parameterKeys)
		{
			return $"{_user.KitNumber}_{methodKey}_{parameterKeys ?? ""}.json";
		}
	}
}