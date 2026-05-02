using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorEntitiesLayer.Entities
{
    public class ApplicationRole : IdentityRole<int>
    {
        public string? Description { get; set; }

    }
}
