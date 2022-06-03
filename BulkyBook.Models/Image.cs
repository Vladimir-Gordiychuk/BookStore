using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Models
{
    public class Image
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Image content.
        /// </summary>
        [Required]
        public byte[] Content { get; set; }

        [Required]
        public string ContentType { get; set; }

        /// <summary>
        /// File extension (including starting dot '.' symbol).
        /// </summary>
        public string Extension { get; set; }
    }
}
