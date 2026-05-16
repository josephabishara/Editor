using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EditorViewModelLayer.ClientViewModel
{
    public class ClientDashboardDTO
    {
        public string? Photo { get; set; }
        public string? Name { get; set; }
        public int ClientId { get; set; }
    
        public string? Email { get; set; } 
        public string? Notes { get; set; }
        public int Publications { get; set; } = 0;
        public int Websites { get; set; }= 0;
        public int Videos { get; set; }= 0;
        public int TotalAD { get; set; }= 0;
        public int TotalPR { get; set; }= 0;
        public int TotalCirclation { get; set; } = 0;
    }
}
