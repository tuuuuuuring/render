using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace render.Services
{
    public interface IMessageBoxService
    {
        bool ShowConfirmation(string message);
    }
}
