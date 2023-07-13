using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    internal sealed class GridDragImageList : IDisposable
    {
        private bool bOwnHandle;
        private IntPtr handle;
        private const int ILC_COLOR = 0;
        private const int ILC_COLOR16 = 0x10;
        private const int ILC_COLOR24 = 0x18;
        private const int ILC_COLOR4 = 4;
        private const int ILC_COLOR8 = 8;
        private const int ILC_COLORDDB = 0xfe;
        private const int ILC_MASK = 1;

        public GridDragImageList()
        {
            this.handle = IntPtr.Zero;
        }

        public GridDragImageList(IntPtr handleIL)
        {
            this.handle = IntPtr.Zero;
            this.handle = handleIL;
            this.bOwnHandle = false;
        }

        public GridDragImageList(int imageWidth, int imageHeigh)
        {
            this.handle = IntPtr.Zero;
            this.Handle = ImageList_Create(imageWidth, imageHeigh, 0x19, 1, 4);
            this.bOwnHandle = true;
        }

        public GridDragImageList(int imageWidth, int imageHeigh, int flags, int initialCount, int growCount)
        {
            this.handle = IntPtr.Zero;
            this.Handle = ImageList_Create(imageWidth, imageHeigh, flags, initialCount, growCount);
            this.bOwnHandle = true;
        }

        public void Add(Bitmap bitmapImage, Color colorTransparent)
        {
            bitmapImage = (Bitmap) bitmapImage.Clone();
            try
            {
                bitmapImage.MakeTransparent(colorTransparent);
                IntPtr monochromeMask = ControlPaint.CreateHBitmapTransparencyMask(bitmapImage);
                IntPtr hbmImage = ControlPaint.CreateHBitmapColorMask(bitmapImage, monochromeMask);
                ImageList_Add(this.Handle, hbmImage, monochromeMask);
                SafeNativeMethods.DeleteObject(hbmImage);
                SafeNativeMethods.DeleteObject(monochromeMask);
                GC.KeepAlive(this);
            }
            finally
            {
                bitmapImage.Dispose();
            }
        }

        public bool BeginDrag(int iImage, int dxHotspot, int dyHotspot)
        {
            bool flag = ImageList_BeginDrag(this.handle, iImage, dxHotspot, dyHotspot);
            GC.KeepAlive(this);
            return flag;
        }

        private void DetachHandle()
        {
            IntPtr handle = this.handle;
            bool bOwnHandle = this.bOwnHandle;
            this.handle = IntPtr.Zero;
            this.bOwnHandle = false;
            if (bOwnHandle && (handle != IntPtr.Zero))
            {
                ImageList_Destroy(handle);
            }
        }

        public void Dispose()
        {
            this.DetachHandle();
            GC.SuppressFinalize(this);
        }

        public static bool DragEnter(IntPtr hwndLock, Point pt)
        {
            return ImageList_DragEnter(hwndLock, pt.X, pt.Y);
        }

        public static bool DragLeave(IntPtr hwndLock)
        {
            return ImageList_DragLeave(hwndLock);
        }

        public static bool DragMove(Point pt)
        {
            return ImageList_DragMove(pt.X, pt.Y);
        }

        public static bool DragShowNolock(bool bShow)
        {
            return ImageList_DragShowNolock(bShow);
        }

        public static void EndDrag()
        {
            ImageList_EndDrag();
        }

        ~GridDragImageList()
        {
            this.DetachHandle();
        }

        [DllImport("comctl32.dll", CharSet=CharSet.Auto)]
        private static extern int ImageList_Add(IntPtr himl, IntPtr hbmImage, IntPtr hbmMask);
        [DllImport("comctl32.dll", CharSet=CharSet.Auto)]
        private static extern bool ImageList_BeginDrag(IntPtr himlTrack, int iTrack, int dxHotspot, int dyHotspot);
        [DllImport("comctl32.dll", CharSet=CharSet.Auto)]
        private static extern IntPtr ImageList_Create(int cx, int cy, int flags, int cInitial, int cGrow);
        [DllImport("comctl32.dll", CharSet=CharSet.Auto)]
        private static extern bool ImageList_Destroy(IntPtr himl);
        [DllImport("comctl32.dll", CharSet=CharSet.Auto)]
        private static extern bool ImageList_DragEnter(IntPtr hwndLock, int x, int y);
        [DllImport("comctl32.dll", CharSet=CharSet.Auto)]
        private static extern bool ImageList_DragLeave(IntPtr hwndLock);
        [DllImport("comctl32.dll", CharSet=CharSet.Auto)]
        private static extern bool ImageList_DragMove(int x, int y);
        [DllImport("comctl32.dll", CharSet=CharSet.Auto)]
        private static extern bool ImageList_DragShowNolock(bool bShow);
        [DllImport("comctl32.dll", CharSet=CharSet.Auto)]
        private static extern void ImageList_EndDrag();

        public IntPtr Handle
        {
            get
            {
                return this.handle;
            }
            set
            {
                this.DetachHandle();
                this.handle = value;
            }
        }
    }

    internal sealed class GridDragImageListOperation : IDisposable
    {
        private IntPtr handleWnd;
        private GridDragImageList ownedDIL;

        public GridDragImageListOperation(GridDragImageList dil, Point ptGrab, Point ptStart)
        {
            this.handleWnd = (IntPtr)(-1);
            this.CommonConstruct(dil, ptGrab, IntPtr.Zero, ptStart);
        }

        public GridDragImageListOperation(GridDragImageList dil, Point ptGrab, IntPtr hwnd, Point ptStart)
        {
            this.handleWnd = (IntPtr)(-1);
            this.CommonConstruct(dil, ptGrab, hwnd, ptStart);
        }

        public GridDragImageListOperation(GridDragImageList dil, Point ptGrab, IntPtr hwnd, Point ptStart, bool bOwnDIL)
        {
            this.handleWnd = (IntPtr)(-1);
            this.CommonConstruct(dil, ptGrab, hwnd, ptStart);
            if (bOwnDIL)
            {
                this.ownedDIL = dil;
            }
        }

        private void CommonConstruct(GridDragImageList dil, Point ptGrab, IntPtr hwnd, Point ptStart)
        {
            this.handleWnd = hwnd;
            dil.BeginDrag(0, ptGrab.X, ptGrab.Y);
            GridDragImageList.DragEnter(hwnd, ptStart);
        }

        public void Dispose()
        {
            if (this.handleWnd != ((IntPtr)(-1)))
            {
                GridDragImageList.DragLeave(this.handleWnd);
                GridDragImageList.EndDrag();
            }
            if (this.ownedDIL != null)
            {
                this.ownedDIL.Dispose();
                this.ownedDIL = null;
            }
        }
    }
}

