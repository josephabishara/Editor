using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EditorEntitiesLayer.Entities
{
    public class Writer : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        [Display(Name = "Writer Name")]
        public string WriterName { get; set; } = string.Empty;
    }
}
