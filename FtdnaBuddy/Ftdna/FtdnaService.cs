using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FtdnaBuddy.Ftdna.Model;
using FtdnaBuddy.Ftdna.QueryModel;

namespace FtdnaBuddy.Ftdna
{
    public class FtdnaService
    {
        const int PageSize = 1500;
        const int BatchDelay = 3000;

        readonly FtdnaConnector _connector;
        FtdnaUser _user;

        public FtdnaService() : this(new FtdnaConnector())
        {

        }

        public FtdnaService(FtdnaConnector connector)
        {
            _connector = connector;
        }

        public FtdnaUser Login(string kitNumber, string password)
        {
            var token = _connector.GetVerificationTokenAsync().Result;
            var loginResult = _connector.LoginAsync(kitNumber, password).Result;
            var ekitId = _connector.GetEkitIdAsync().Result;
            _user = new FtdnaUser(ekitId, token); 

            return _user;
        }

        public async Task<IEnumerable<Match>> ListAllMatches()
        {
            RequireLogin();

            var result = new List<Match>(PageSize);
            bool done = false;
            for (int page = 1; !done; page++)
            {
                if (page > 1)
                {
                    await Task.Delay(BatchDelay);
                }

                var matchResult = await _connector.ListAllMatches(PageSize, page);
                result.AddRange(matchResult.Data);
                done = matchResult.Count != PageSize;
            }
            return result;
        }

		public async Task<IEnumerable<Match>> ListNewMatches(DateTime startDate)
		{
			RequireLogin();

			var result = new List<Match>(PageSize);
			bool done = false;
			for (int page = 1; !done; page++)
			{
				if (page > 1)
				{
					await Task.Delay(BatchDelay);
				}

				var matchResult = await _connector.ListNewMatches(PageSize, page, startDate);
				result.AddRange(matchResult.Data);
				done = matchResult.Count != PageSize;
			}
			return result;
		}

        public async Task<IEnumerable<ChromosomeSegment>> ListChromosomeSegmentsByMatchName()
        {
            RequireLogin();
            return await _connector.ListChromosomeSegmentsAsync(_user.EncryptedKitId);
        }

		public async Task<IEnumerable<Match>> ListInCommonWith(IKitIdentity match)
		{
			RequireLogin();

			var result = new List<Match>(PageSize);
			bool done = false;
			for (int page = 1; !done; page++)
			{
				if (page > 1)
				{
					await Task.Delay(BatchDelay);
				}

				var matchResult = await _connector.ListInCommonWithAsync(match.ResultId2, PageSize, page);
				result.AddRange(matchResult.Data);
				done = matchResult.Count != PageSize;
			}
			return result;
		}

		public async Task<MatchDetails> GetMatchDetails(IKitIdentity match)
		{
			RequireLogin();
            var detailsResult = await _connector.GetMatchDetailsAsync(match.ResultId1, match.ResultId2);
            return detailsResult.Result;
		}

		private void RequireLogin()
		{
			if (_user == null)
				throw new InvalidOperationException("Must log in before performing this operation");
		}
	}
}