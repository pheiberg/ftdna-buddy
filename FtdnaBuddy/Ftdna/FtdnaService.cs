using System.Collections.Generic;
using System.Threading;
using FtdnaBuddy.Ftdna.Model;

namespace FtdnaBuddy.Ftdna
{
    public class FtdnaService
    {
        const int PageSize = 1500;
        const int BatchDelay = 3000;

        readonly FtdnaConnector _connector;

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

            return new FtdnaUser(ekitId, token);
        }

        public IEnumerable<Match> ListMatches()
        {
            var result = new List<Match>(PageSize);
            bool done = false;
            for (int page = 1; !done; page++)
            {
                if(page > 1)
                {
                    Thread.Sleep(BatchDelay);    
                }

                var matchResult = _connector.ListMatches(PageSize, page).Result;
                result.AddRange(matchResult.Data);
                done = matchResult.Count != PageSize;
            }
            return result;
        }
    }
}