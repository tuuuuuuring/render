using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace render.Services
{
    public class MessageBoxService : IMessageBoxService
    {
        public bool ShowConfirmation(string message)
        {
            return MessageBox.Show(message, "确认", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
        }
    }
}
