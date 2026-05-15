using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EditorViewModelLayer.WriterViewModel
{
    public class WriterDTO
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(200)]
        [Display(Name = "Writer Name")]
        public string WriterName { get; set; } = string.Empty;
    }
}
