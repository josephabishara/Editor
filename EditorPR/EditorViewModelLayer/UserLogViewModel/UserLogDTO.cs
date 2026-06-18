using System;
using System.Collections.Generic;
using System.Text;

namespace EditorViewModelLayer.UserLogViewModel
{
    public class UserLogDTO
    {
        public int LogId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime LogDate { get; set; }
        public string Action { get; set; } = string.Empty;
        public string ControllerName { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public int RecordId { get; set; }
    }
}
