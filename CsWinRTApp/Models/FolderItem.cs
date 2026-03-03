using System;
using System.IO;

namespace CsWinRTApp.Models
{
    public class FolderItem
    {
        public string FullPath { get; set; }
        public string DisplayName { get; set; }

        public FolderItem(string fullPath)
        {
            FullPath = fullPath;
            DisplayName = Path.GetFileName(fullPath);
            
            // 如果是根目录（如 C:\），使用完整路径作为显示名称
            if (string.IsNullOrEmpty(DisplayName))
            {
                DisplayName = fullPath;
            }
        }
    }
}
