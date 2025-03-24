using System;
using System.ComponentModel.DataAnnotations;

namespace Research_Assistant.Models
{
    public class Paper
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } // Extracted from PDF

        public string Author { get; set; } // Extracted from PDF

        public DateTime? PublicationDate { get; set; }  // Extracted from PDF

        public string Keywords { get; set; } // Extracted using NLP

        public string FilePath { get; set; } // Where the PDF is stored

        public DateTime UploadDate { get; set; } = DateTime.Now;
        
        public string ExtractedText { get; set; } 
    }
}
