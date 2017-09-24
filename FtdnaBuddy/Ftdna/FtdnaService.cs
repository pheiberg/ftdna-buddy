using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FtdnaBuddy.Ftdna.Model;
using FtdnaBuddy.Ftdna.QueryModel;

namespace FtdnaBuddy.Ftdna
{
    public class FtdnaService : IDnaDataService
    {
        readonly FtdnaConnector _connector;
        FtdnaUser _user;

        public FtdnaService(int requestDelay) : this(new FtdnaConnector(requestDelay))
        {

        }

        public FtdnaService(FtdnaConnector connector)
        {
            _connector = connector;
        }

        public LoginResult Login(string kitNumber, string password)
        {
            var token = _connector.GetVerificationTokenAsync().Result;
            var loginResult = _connector.LoginAsync(kitNumber, password).Result;
            if(loginResult.ErrorMessage != null)
            {
                return loginResult;
            }


            var ekitId = _connector.GetEkitIdAsync().Result;
            _user = new FtdnaUser(kitNumber, ekitId, token);
            loginResult.User = _user;
            return loginResult;
        }

        public async Task<IEnumerable<Match>> ListAllMatches()
        {
            RequireLogin();

            var result = new List<Match>();
            bool done = false;
            for (int page = 1; !done; page++)
            {
                var matchResult = await _connector.ListAllMatches(page);
                result.AddRange(matchResult.Data);
                done = matchResult.Count != FtdnaConnector.DefaultPageSize;
            }
            return result;
        }

		public async Task<IEnumerable<Match>> ListNewMatches(DateTime startDate)
		{
			RequireLogin();

			var result = new List<Match>();
			bool done = false;
			for (int page = 1; !done; page++)
			{
				var matchResult = await _connector.ListNewMatches(startDate, page);
				result.AddRange(matchResult.Data);
				done = matchResult.Count != FtdnaConnector.DefaultPageSize;
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

			var result = new List<Match>();
			bool done = false;
			for (int page = 1; !done; page++)
			{
				var matchResult = await _connector.ListInCommonWithAsync(match.ResultId2, page);
				result.AddRange(matchResult.Data);
				done = matchResult.Count != FtdnaConnector.DefaultPageSize;
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