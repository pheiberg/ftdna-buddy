using System.Threading.Tasks;
using FtdnaBuddy.Ftdna.Model;

namespace FtdnaBuddy.Ftdna
{
    public class FtdnaService
    {
        readonly FtdnaConnector _connector;

        public FtdnaService() : this(new FtdnaConnector())
        {

        }

        public FtdnaService(FtdnaConnector connector)
        {
            _connector = connector;
        }

        public async Task<FtdnaUser> Login(string kitNumber, string password)
        {
            var token = await _connector.GetVerificationTokenAsync();
            await _connector.LoginAsync(kitNumber, password);
            var ekitId = await _connector.GetEkitIdAsync();

            return new FtdnaUser(ekitId, token);
        }
    }
}