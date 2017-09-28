using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FtdnaBuddy.Ftdna.Model;
using FtdnaBuddy.Ftdna.QueryModel;

namespace FtdnaBuddy.Ftdna
{
    public class FtdnaService : IDnaDataService
    {
	    private readonly IFtdnaConnector _connector;
	    private readonly IFtdnaAuthenticator _autenticator;
	    
        public FtdnaService(int requestDelay) : this(new FtdnaConnector(requestDelay))
        {

        }

        public FtdnaService(IFtdnaConnector connector) : this(connector, new FtdnaAuthenticator(connector))
        {
        }
	    
	    public FtdnaService(IFtdnaConnector connector, IFtdnaAuthenticator autenticator)
	    {
		    _connector = connector;
		    _autenticator = autenticator;
	    }

        public LoginResult Login(string kitNumber, string password)
        {
	        return _autenticator.Authenticate(kitNumber, password);
        }

        public async Task<IEnumerable<Match>> ListAllMatches(int pageSize = FtdnaConnector.DefaultPageSize)
        {
            RequireLogin();

            var result = new List<Match>();
            bool done = false;
            for (int page = 1; !done; page++)
            {
                var matchResult = await _connector.ListAllMatches(page, pageSize);
                result.AddRange(matchResult.Data);
                done = matchResult.Count != pageSize;
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
            return await _connector.ListChromosomeSegmentsAsync(_autenticator.User.EncryptedKitId);
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
			if (!_autenticator.IsLoggedIn)
				throw new InvalidOperationException("Must log in before performing this operation");
		}
	}
}