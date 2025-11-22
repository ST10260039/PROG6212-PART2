using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonthlyClaimSystem.Models
{
    public class Document
    {
        [Key]
        public int DocumentId { get; set; }

        [ForeignKey(nameof(Claim))]
        public int ClaimId { get; set; }
        public Claim? Claim { get; set; }

        //Metadata
        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FileType { get; set; } = string.Empty; // e.g. "pdf", "jpg"

        public long FileSize { get; set; } // in bytes

        public DateTime UploadedOn { get; set; } = DateTime.UtcNow;

        //Content
        [Required]
        public byte[] Data { get; set; } = Array.Empty<byte>();

        
        public byte[]? EncryptedData { get; set; }
    }
}