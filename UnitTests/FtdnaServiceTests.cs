using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FtdnaBuddy.Ftdna;
using FtdnaBuddy.Ftdna.Model;
using NSubstitute;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace UnitTests
{
    public class FtdnaServiceTests
    {
        [Theory, AutoSubstituteData]
        public void ListAllMatches_ShouldFailIfNotLoggedIn(
            [Frozen]IFtdnaAuthenticator authenticator,
            FtdnaService sut)
        {
            authenticator.IsLoggedIn.Returns(false);

            Func<Task> f = async () => await sut.ListAllMatches();
            
            f.ShouldThrow<InvalidOperationException>();
        }

        [Theory, AutoSubstituteData]
        public async void ListAllMatchesWhenOnePage_ShouldReturnAllMatchesFromConnector(
            [Frozen]IFtdnaAuthenticator authenticator,
            [Frozen]IFtdnaConnector connector,
            [Greedy]FtdnaService sut,
            FtdnaUser user,
            MatchResults matchResult)
        {
            authenticator.IsLoggedIn.Returns(true);
            connector.ListAllMatches().ReturnsForAnyArgs(matchResult);
            
            var result = await sut.ListAllMatches();

            result.Should().BeEquivalentTo(matchResult.Data);
        }
        
        [Theory, AutoSubstituteData]
        public async void ListAllMatchesWhenTwoPages_ShouldReturnAllMatchesFromAllConnectorCalls(
            [Frozen]IFtdnaAuthenticator authenticator,
            [Frozen]IFtdnaConnector connector,
            [Greedy]FtdnaService sut,
            FtdnaUser user,
            MatchResults matchResult1,
            MatchResults matchResult2)
        {
            matchResult1.Count = matchResult1.Data.Count();
            matchResult2.Data = matchResult2.Data.Take(2);
            matchResult2.Count = matchResult2.Data.Count();
            authenticator.IsLoggedIn.Returns(true);
            connector.ListAllMatches(0, 0).ReturnsForAnyArgs(matchResult1, matchResult2);
            
            var result = await sut.ListAllMatches(3);

            result.Should().BeEquivalentTo(matchResult1.Data.Concat(matchResult2.Data));
        }
    }
}