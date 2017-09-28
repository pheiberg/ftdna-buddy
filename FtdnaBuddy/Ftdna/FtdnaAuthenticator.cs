using FtdnaBuddy.Ftdna.Model;

namespace FtdnaBuddy.Ftdna
{
    public class FtdnaAuthenticator : IFtdnaAuthenticator
    {
        private readonly IFtdnaConnector _connector;
        private FtdnaUser _user;

        public FtdnaAuthenticator(IFtdnaConnector connector)
        {
            _connector = connector;
        }
		
        public LoginResult Authenticate(string kitNumber, string password)
        {
            var token = _connector.GetVerificationTokenAsync().Result;
            var loginResult = _connector.LoginAsync(kitNumber, password).Result;
            if(loginResult.ErrorMessage != null)
            {
                return loginResult;
            }

            var ekitId = _connector.GetEkitIdAsync().Result;
            loginResult.User = new FtdnaUser(kitNumber, ekitId, token);
            _user = loginResult.User;
            return loginResult;
        }

        public bool IsLoggedIn => _user != null;

        public FtdnaUser User => _user;
    }
}