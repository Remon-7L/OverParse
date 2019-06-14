using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

//sourcecode from
//https://gist.github.com/EmissaryFromSunday/1308531
//いつか書き直す

namespace HotKeyFrame
{
    class HotKey  // HACK: BAD Copy & Paste
    {
        private const int WM_HOTKEY = 0x0312;
        private readonly IntPtr _windowHandle;
        private Dictionary<int, EventHandler> _hotkeyEvents;

        public HotKey(Window window)
        {
            WindowInteropHelper _host = new WindowInteropHelper(window);
            _windowHandle = _host.Handle;
            ComponentDispatcher.ThreadPreprocessMessage += ComponentDispatcher_ThreadPreprocessMessage;
            _hotkeyEvents = new Dictionary<int, EventHandler>();
        }

        public void ComponentDispatcher_ThreadPreprocessMessage(ref MSG msg, ref bool handled)
        {
            if (msg.message != WM_HOTKEY) { return; }

            var hotkeyID = msg.wParam.ToInt32();
            if (!_hotkeyEvents.Any((x) => x.Key == hotkeyID)) { return; }

            new ThreadStart(
                () => _hotkeyEvents[hotkeyID](this, EventArgs.Empty)
            ).Invoke();
        }

        public void Regist(ModifierKeys modkey, Key trigger, EventHandler eh,int i)
        {
            var imod = modkey.ToInt32();
            var itrg = KeyInterop.VirtualKeyFromKey(trigger);

            while ((++i < 0xc000) && Ov3rD4r53.NativeMethods.RegisterHotKey(_windowHandle, i, imod, itrg) == 0) { ; }

            if (i < 0xc000)
            {
                _hotkeyEvents.Add(i, eh);
            }
        }

        public void Unregist()
        {
            foreach (var hotkeyid in _hotkeyEvents.Keys)
            {
                Ov3rD4r53.NativeMethods.UnregisterHotKey(_windowHandle, hotkeyid);
            }
        }
    }

    static class Extention
    {
        public static int ToInt32(this ModifierKeys m) => (int)m;
    }
}
