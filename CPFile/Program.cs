using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace CPF
{
    public class CPFClipboard
    {
        /// <summary>The GetForegroundWindow function returns a handle to the foreground window.</summary>
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private static Shell32.FolderItem ReadFileInfo()
        {
            IntPtr handle = GetForegroundWindow();

            List<string> selected = new List<string>();
            SHDocVw.ShellWindows shellWindows = new SHDocVw.ShellWindowsClass();

            foreach (SHDocVw.ShellBrowserWindow window in shellWindows)
            {
                if (window.HWND == (int)handle)
                {
                    Shell32.FolderItems items = ((Shell32.IShellFolderViewDual2)window.Document).SelectedItems();
                    if (items.Count == 1)
                        return items.Item(0);
                }
            }
            return null;
        }

        public void CopyNamePathToClipboard()
        {
            string strContent = ReadFileInfo().Path;
            Clipboard.SetText(strContent);
        }

        public void CopyNameToClipboard()
        {
            string strContent = ReadFileInfo().Name;
            Clipboard.SetText(strContent);
        }

        public void CopyPathToClipboard()
        {
            string strContent = Path.GetDirectoryName(ReadFileInfo().Path);
            Clipboard.SetText(strContent);
        }
    }

    class CPFile
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern short GetKeyState(int key);

        // Constants
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int KeyPressed = 0x8000;
        
        private static LowLevelKeyboardProc m_keyboardProc = HookCallback;
        private static IntPtr m_pHookID = IntPtr.Zero;

        public static bool IsKeyDown(int key)
        {
            return (GetKeyState((int)key) & KeyPressed) != 0;
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            const int VK_SHIFT = 0x10;
            const int VK_CONTROL = 0x11;
            const int nKeyC = 67; // file name plus path
            const int nKeyF = 70; // file name only
            const int nKeyP = 80; // path only

            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (IsKeyDown(VK_SHIFT) && IsKeyDown(VK_CONTROL))
                {
                    switch (vkCode)
                    {
                        case nKeyC:
                        {
                            CPFClipboard clipboard = new CPFClipboard();
                            Thread workerThread = new Thread(clipboard.CopyNamePathToClipboard);
                            workerThread.SetApartmentState(ApartmentState.STA);
                            workerThread.Start();
                            break;
                        }
                        case nKeyF:
                        {
                            CPFClipboard clipboard = new CPFClipboard();
                            Thread workerThread = new Thread(clipboard.CopyNameToClipboard);
                            workerThread.SetApartmentState(ApartmentState.STA);
                            workerThread.Start();
                            break;
                        }
                        case nKeyP:
                        {
                            CPFClipboard clipboard = new CPFClipboard();
                            Thread workerThread = new Thread(clipboard.CopyPathToClipboard);
                            workerThread.SetApartmentState(ApartmentState.STA);
                            workerThread.Start();
                            break;
                        }
                        default:
                            break;
                    }
                }
            }
            return CallNextHookEx(m_pHookID, nCode, wParam, lParam);
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            m_pHookID = SetHook(m_keyboardProc);
            Application.Run(new CPFForm());
            UnhookWindowsHookEx(m_pHookID);
        }
    }
}