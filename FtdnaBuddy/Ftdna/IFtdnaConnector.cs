using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FtdnaBuddy.Ftdna.Model;

namespace FtdnaBuddy.Ftdna
{
    public interface IFtdnaConnector
    {
        Task<string> GetVerificationTokenAsync();
        Task<LoginResult> LoginAsync(string kitNumber, string password);
        Task<string> GetEkitIdAsync();
        Task<MatchResults> ListAllMatches(int page = 1, int pageSize = FtdnaConnector.DefaultPageSize);
        Task<MatchResults> ListNewMatches(DateTime startDate, int page = 1, int pageSize = FtdnaConnector.DefaultPageSize);
        Task<IEnumerable<ChromosomeSegment>> ListChromosomeSegmentsAsync(string ekitId);
        Task<MatchResults> ListInCommonWithAsync(string resultId2, int page = 1, int pageSize = FtdnaConnector.DefaultPageSize);
        Task<MatchDetailsResult> GetMatchDetailsAsync(string resultId1, string resultId2);
    }
}