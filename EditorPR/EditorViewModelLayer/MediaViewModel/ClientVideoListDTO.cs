using System;
using System.Collections.Generic;
using System.Text;

namespace EditorViewModelLayer.MediaViewModel
{
    public class ClientVideoListDTO
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public IEnumerable<ClientVideoDTO> Items { get; set; } = new List<ClientVideoDTO>();

        public List<MediaSelectOption> ChannelOptions { get; set; } = new();
        public List<MediaSelectOption> CategoryOptions { get; set; } = new();
        public List<MediaSelectOption> SubCategoryOptions { get; set; } = new();

        public int Total => Items.Count();
        public decimal TotalPR => Items.Sum(i => i.PRValue);
        public decimal TotalAD => Items.Sum(i => i.ADValue);
    }

}
