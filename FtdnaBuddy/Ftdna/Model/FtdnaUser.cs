namespace FtdnaBuddy.Ftdna.Model
{
    public class FtdnaUser
    {
        public FtdnaUser(string kitNumber, string encryptedKidId, string requestVerificationToken)
        {
            KitNumber = kitNumber;
            EncryptedKitId = encryptedKidId;
            RequestVerificationToken = requestVerificationToken;
        }

		public string KitNumber { get; }

        public string EncryptedKitId { get; }

        public string RequestVerificationToken { get; }
    }
}