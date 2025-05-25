using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace render.Services
{
    public interface IFileDialogService
    {
        string OpenFileDialog(string title, string filter);
        string OpenTextureDialog(string title, string filter);
    }
}
