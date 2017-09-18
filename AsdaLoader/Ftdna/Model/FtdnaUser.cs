namespace AsdaLoader.Ftdna.Model
{
    public class FtdnaUser
    {
        public FtdnaUser(string encryptedKidId, string requestVerificationToken)
        {
            EncryptedKitId = encryptedKidId;
            RequestVerificationToken = requestVerificationToken;
        }

        public string EncryptedKitId { get; }

        public string RequestVerificationToken { get; }
    }
}