using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FtdnaBuddy.Ftdna.Model;
using FtdnaBuddy.Ftdna.QueryModel;

namespace FtdnaBuddy.Ftdna
{
    public interface IDnaDataService
    {
        LoginResult Login(string kitNumber, string password);

        Task<IEnumerable<Match>> ListAllMatches();

        Task<IEnumerable<Match>> ListNewMatches(DateTime startDate);

        Task<IEnumerable<ChromosomeSegment>> ListChromosomeSegmentsByMatchName();

        Task<IEnumerable<Match>> ListInCommonWith(IKitIdentity match);

        Task<MatchDetails> GetMatchDetails(IKitIdentity match);
    }
}