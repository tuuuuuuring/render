using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace render.Services
{
    public class FileDialogService : IFileDialogService
    {
        public string OpenFileDialog(string title, string filter)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = title,
                Filter = filter
            };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public string OpenTextureDialog(string title, string filter)
        {
            // 实现与OpenFileDialog类似，可根据需要调整
            return OpenFileDialog(title, filter);
        }
    }
}
