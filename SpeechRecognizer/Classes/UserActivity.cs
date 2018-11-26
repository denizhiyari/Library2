using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpeechRecognizer.Classes
{
    public class UserActivity
    {
        public KeyEventHandler KeyDown;
        public KeyEventHandler KeyuP;

        public void Inıtialize()
        {
            GlobalHook.HookManager.KeyDown += KeyDown;
            GlobalHook.HookManager.KeyUp += KeyuP;
        }

    }
}
