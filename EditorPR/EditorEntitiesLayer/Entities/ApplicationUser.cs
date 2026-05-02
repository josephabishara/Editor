using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace EditorEntitiesLayer.Entities
{
    public class ApplicationUser : IdentityUser<int>
    {
        [Required]
        [Display(Name = "Full Name")]
        public string? FullName { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
