namespace FtdnaBuddy.Ftdna.QueryModel
{
    public class User
    {
        public User(string kitNumber, string encryptedKitId)
        {
            KitNumber = kitNumber;
            EncryptedKitId = encryptedKitId;
        }

        public string KitNumber { get; }

        public string EncryptedKitId { get; }
    }
}
