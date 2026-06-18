using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EditorEntitiesLayer.Entities
{
    public class UserLog
    {
        [Key]
        public int LogId { get; set; }
        public int UserId { get; set; } 
        
        public string UserName { get; set; } // The name of the user who performed the action

        public DateTime LogDate { get; set; } // The date and time when the action was performed
        public string Action { get; set; } = string.Empty; // e.g., "Create", "Update", "Delete"

        public string ControllerName { get; set; } = string.Empty; // The name of the controller where the action occurred
        public string EntityName { get; set; } = string.Empty;


        public int RecordId { get; set; } // The ID of the record that was affected by the action
    }
}
