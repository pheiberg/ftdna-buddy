using FtdnaBuddy.Ftdna.Model;

namespace FtdnaBuddy.Ftdna
{
    public interface IFtdnaAuthenticator
    {
        LoginResult Authenticate(string kitNumber, string password);
		
        bool IsLoggedIn { get; }
		
        FtdnaUser User { get; }
    }
}