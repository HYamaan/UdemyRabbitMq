using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UdemyRabbitMq.ExcelCreate.Models
{
    public enum FileStatus
    {
        Creating,
        Completed
    }

    public class UserFile
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime? CreatedAt { get; set; }
        public FileStatus Status { get; set; }

        [NotMapped]
        public string GetCreatedDate => CreatedAt.HasValue ? CreatedAt.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty;

    }
}
