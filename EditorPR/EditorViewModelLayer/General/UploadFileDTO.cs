using System;
using System.Collections.Generic;
using System.Text;

namespace EditorViewModelLayer.General
{
    public class UploadFileDTO
    {
        public string FileName { get; set; } = string.Empty;
        public Stream FileStream { get; set; } = Stream.Null;
        public string ContentType { get; set; } = string.Empty;  
        public long Length { get; set; } 
    }
}
