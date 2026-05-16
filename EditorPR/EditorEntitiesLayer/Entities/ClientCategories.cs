using System;
using System.Collections.Generic;
using System.Text;

namespace EditorEntitiesLayer.Entities
{
    public class ClientCategories : BaseEntity
    {
        public int ClientId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int? ParentCategory { get; set; }
        public string? CategoryType { get; set; }
        public string? Status { get; set; }
        public int Order { get; set; }

        // Navigation
        public Client? Client { get; set; }
        public ClientCategories? Parent { get; set; }
        public ICollection<ClientCategories> Children { get; set; } = new List<ClientCategories>();
       // public ICollection<Article> Articles { get; set; } = new List<Article>();
    }
}
