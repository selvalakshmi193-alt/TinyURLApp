using System.ComponentModel.DataAnnotations;

namespace TinyUrlApi.Models
{
    public class UrlEntry
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string OriginalUrl { get; set; } = null!;
        [Required, MaxLength(6)]
        public string ShortCode { get; set; } = null!;
        public bool IsPrivate { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int Clicks { get; set; } = 0;
    }
}
