using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    internal class NativeMethods
    {
        public const int CHILDID_SELF = 0;
        public const int COLOR_BTNFACE = 15;
        public const int COLOR_HIGHLIGHT = 13;
        public const int COLOR_INACTIVECAPTION = 3;
        public const int COLOR_WINDOW = 5;
        public const int DCX_CACHE = 2;
        public const int DCX_CLIPCHILDREN = 8;
        public const int DCX_CLIPSIBLINGS = 0x10;
        public const int DCX_EXCLUDERGN = 0x40;
        public const int DCX_EXCLUDEUPDATE = 0x100;
        public const int DCX_INTERSECTRGN = 0x80;
        public const int DCX_INTERSECTUPDATE = 0x200;
        public const int DCX_LOCKWINDOWUPDATE = 0x400;
        public const int DCX_NODELETERGN = 0x40000;
        public const int DCX_NORESETATTRS = 4;
        public const int DCX_PARENTCLIP = 0x20;
        public const int DCX_USESTYLE = 0x10000;
        public const int DCX_WINDOW = 1;
        public const int DFC_BUTTON = 4;
        public const int DFCS_BUTTONPUSH = 0x10;
        public const int DISP_E_EXCEPTION = -2147352567;
        public const int DISP_E_MEMBERNOTFOUND = -2147352573;
        public const int DISP_E_PARAMNOTFOUND = -2147352572;
        public const int EC_LEFTMARGIN = 1;
        public const int EC_RIGHTMARGIN = 2;
        public const int EC_USEFONTINFO = 0xffff;
        public const int EM_SETMARGINS = 0xd3;
        public const int ES_AUTOHSCROLL = 0x80;
        public const int ES_AUTOVSCROLL = 0x40;
        public const int GW_CHILD = 5;
        public const int GWL_EXSTYLE = -20;
        public const int GWL_HWNDPARENT = -8;
        public const int GWL_STYLE = -16;
        public const int GWL_WNDPROC = -4;
        public static IntPtr InvalidIntPtr = ((IntPtr) (-1));
        public static IntPtr NullIntPtr = IntPtr.Zero;
        public const int OBJID_CLIENT = -4;
        public const int OBJID_HSCROLL = -6;
        public const int OBJID_VSCROLL = -5;
        public const int PS_SOLID = 0;
        public const int SB_BOTTOM = 7;
        public const int SB_CTL = 2;
        public const int SB_ENDSCROLL = 8;
        public const int SB_HORZ = 0;
        public const int SB_LEFT = 6;
        public const int SB_LINEDOWN = 1;
        public const int SB_LINELEFT = 0;
        public const int SB_LINERIGHT = 1;
        public const int SB_LINEUP = 0;
        public const int SB_PAGEDOWN = 3;
        public const int SB_PAGELEFT = 2;
        public const int SB_PAGERIGHT = 3;
        public const int SB_PAGEUP = 2;
        public const int SB_RIGHT = 7;
        public const int SB_THUMBPOSITION = 4;
        public const int SB_THUMBTRACK = 5;
        public const int SB_TOP = 6;
        public const int SB_VERT = 1;
        public const int SIF_ALL = 0x17;
        public const int SIF_PAGE = 2;
        public const int SIF_POS = 4;
        public const int SIF_RANGE = 1;
        public const int SIF_TRACKPOS = 0x10;
        public const int SWP_DRAWFRAME = 0x20;
        public const int SWP_HIDEWINDOW = 0x80;
        public const int SWP_NOACTIVATE = 0x10;
        public const int SWP_NOMOVE = 2;
        public const int SWP_NOSIZE = 1;
        public const int SWP_NOZORDER = 4;
        public const int SWP_SHOWWINDOW = 0x40;
        public const string uuid_IAccessible = "{618736E0-3C3D-11CF-810C-00AA00389B71}";
        public const int WHEEL_DELTA = 120;
        public const int WM_CHAR = 0x102;
        public const int WM_CONTEXTMENU = 0x7b;
        public const int WM_ERASEBKGND = 20;
        public const int WM_HSCROLL = 0x114;
        public const int WM_KEYDOWN = 0x100;
        public const int WM_KEYUP = 0x101;
        public const int WM_LBUTTONDOWN = 0x201;
        public const int WM_LBUTTONUP = 0x202;
        public const int WM_NCPAINT = 0x85;
        public const int WM_RBUTTONUP = 0x205;
        public const int WM_SETFONT = 0x30;
        public const int WM_SYSCOLORCHANGE = 0x15;
        public const int WM_VSCROLL = 0x115;
        public const int WS_BORDER = 0x800000;
        public const int WS_CHILD = 0x40000000;
        public const int WS_CLIPCHILDREN = 0x2000000;
        public const int WS_CLIPSIBLINGS = 0x4000000;
        public const int WS_EX_CLIENTEDGE = 0x200;
        public const int WS_EX_STATICEDGE = 0x20000;
        public const int WS_HSCROLL = 0x100000;
        public const int WS_VSCROLL = 0x200000;

        private NativeMethods()
        {
        }

        [DllImport("Gdi32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int CombineRgn(HandleRef hRgn, HandleRef hRgn1, HandleRef hRgn2, int nCombineMode);
        [DllImport("Gdi32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        internal static extern IntPtr CreatePen(int nStyle, int nWidth, int crColor);
        [DllImport("Gdi32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr CreateRectRgn(int left, int top, int right, int bottom);
        [DllImport("oleacc.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int CreateStdAccessibleObject(IntPtr hWnd, int objID, ref Guid refiid, [In, Out, MarshalAs(UnmanagedType.Interface)] ref object pAcc);
        [DllImport("Gdi32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        internal static extern IntPtr DeleteObject(HandleRef hObject);
        internal static void DrawLine(HandleRef h, int x1, int y1, int x2, int y2)
        {
            MoveToEx(h, x1, y1, null);
            LineTo(h, x2, y2);
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool GetClientRect(IntPtr hWnd, [In, Out] ref RECT rect);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr GetWindow(IntPtr hWnd, int uCmd);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool GetWindowRect(IntPtr hWnd, [In, Out] ref RECT rect);
        [DllImport("Gdi32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        internal static extern bool LineTo(HandleRef hdc, int x, int y);
        [DllImport("Gdi32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool MoveToEx(HandleRef hdc, int x, int y, POINT pt);
        [DllImport("Gdi32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        internal static extern IntPtr SelectObject(HandleRef hDC, HandleRef hObject);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int SetScrollInfo(IntPtr hWnd, int fnBar, SCROLLINFO si, bool redraw);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr CreateBitmap(int nWidth, int nHeight, int nPlanes, int nBitsPerPixel, ushort[] lpvBits);

        [StructLayout(LayoutKind.Sequential)]
        public class COMRECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
            public COMRECT()
            {
            }

            public COMRECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public class POINT
        {
            public int x;
            public int y;
            public POINT()
            {
            }

            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }

            public static NativeMethods.RECT FromXYWH(int x, int y, int width, int height)
            {
                return new NativeMethods.RECT(x, y, x + width, y + height);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public class SCROLLINFO
        {
            public int cbSize;
            public int fMask;
            public int nMin;
            public int nMax;
            public int nPage;
            public int nPos;
            public int nTrackPos;
            public SCROLLINFO()
            {
                this.cbSize = Marshal.SizeOf(typeof(NativeMethods.SCROLLINFO));
            }

            public SCROLLINFO(bool bInitWithAllMask) : this()
            {
                this.fMask = 0x17;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class TEXTMETRIC
        {
            public int tmHeight;
            public int tmAscent;
            public int tmDescent;
            public int tmInternalLeading;
            public int tmExternalLeading;
            public int tmAveCharWidth;
            public int tmMaxCharWidth;
            public int tmWeight;
            public int tmOverhang;
            public int tmDigitizedAspectX;
            public int tmDigitizedAspectY;
            public char tmFirstChar;
            public char tmLastChar;
            public char tmDefaultChar;
            public char tmBreakChar;
            public byte tmItalic;
            public byte tmUnderlined;
            public byte tmStruckOut;
            public byte tmPitchAndFamily;
            public byte tmCharSet;
        }

        internal class Util
        {
            public static int HIWORD(int n)
            {
                return ((n >> 0x10) & 0xffff);
            }

            public static int HIWORD(IntPtr n)
            {
                return HIWORD((int) n);
            }

            public static int LOWORD(int n)
            {
                return (n & 0xffff);
            }

            public static int LOWORD(IntPtr n)
            {
                return LOWORD((int) n);
            }

            public static int MAKELONG(int low, int high)
            {
                return ((high << 0x10) | (low & 0xffff));
            }

            public static IntPtr MAKELPARAM(int low, int high)
            {
                return (IntPtr) ((high << 0x10) | (low & 0xffff));
            }

            public static int RGB_GETBLUE(int color)
            {
                return ((color >> 0x10) & 0xff);
            }

            public static int RGB_GETGREEN(int color)
            {
                return ((color >> 8) & 0xff);
            }

            public static int RGB_GETRED(int color)
            {
                return (color & 0xff);
            }
        }

        public delegate IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    }

    [SuppressUnmanagedCodeSecurity]
    internal class SafeNativeMethods
    {
        private SafeNativeMethods()
        {
        }

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool DeleteObject(IntPtr hObject);
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool DrawFrameControl(IntPtr hDC, ref NativeMethods.RECT rect, int type, int state);
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool GetScrollInfo(IntPtr hWnd, int fnBar, [In, Out] NativeMethods.SCROLLINFO si);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetSysColor(int nIndex);
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool InvalidateRect(IntPtr hWnd, NativeMethods.COMRECT rect, bool erase);
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool ScrollWindow(IntPtr hWnd, int nXAmount, int nYAmount, ref NativeMethods.RECT rectScrollRegion, ref NativeMethods.RECT rectClip);
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);
    }

    internal class UnsafeNativeMethods
    {
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        [DllImport("User32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern IntPtr GetDCEx(IntPtr hWnd, IntPtr hrgnClip, int flags);
        [DllImport("User32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
    }
}

