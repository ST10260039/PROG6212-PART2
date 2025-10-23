namespace MonthlyClaimSystem.Models
{

    public class Document
    {
        public int DocumentId { get; set; }
        public int ClaimId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public byte[] EncryptedData { get; set; }
    }
}