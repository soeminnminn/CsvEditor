using System;
using System.Windows.Media;
using System.Windows;

namespace CsvEditor.Controls
{
    internal enum InterestPoint
    {
        TopLeft = 0,
        TopRight = 1,
        BottomLeft = 2,
        BottomRight = 3,
        Center = 4,
    }

    internal class Helper
    {
        internal static bool TryGetTransformToDevice(Visual visual, out Matrix value)
        {
            var presentationSource = PresentationSource.FromVisual(visual);
            if (presentationSource != null)
            {
                value = presentationSource.CompositionTarget.TransformToDevice;
                return true;
            }

            value = default;
            return false;
        }

        internal static Vector GetOffset(UIElement element1, InterestPoint interestPoint1, UIElement element2, InterestPoint interestPoint2, Rect element2Bounds)
        {
            Point point = element1.TranslatePoint(GetPoint(element1, interestPoint1), element2);
            if (element2Bounds.IsEmpty)
            {
                return point - GetPoint(element2, interestPoint2);
            }
            else
            {
                return point - GetPoint(element2Bounds, interestPoint2);
            }
        }

        private static Point GetPoint(UIElement element, InterestPoint interestPoint)
        {
            return GetPoint(new Rect(element.RenderSize), interestPoint);
        }

        private static Point GetPoint(Rect rect, InterestPoint interestPoint)
        {
            switch (interestPoint)
            {
                case InterestPoint.TopLeft:
                    return rect.TopLeft;
                case InterestPoint.TopRight:
                    return rect.TopRight;
                case InterestPoint.BottomLeft:
                    return rect.BottomLeft;
                case InterestPoint.BottomRight:
                    return rect.BottomRight;
                case InterestPoint.Center:
                    return new Point(rect.Left + rect.Width / 2,
                                     rect.Top + rect.Height / 2);
                default:
                    throw new ArgumentOutOfRangeException(nameof(interestPoint));
            }
        }

        internal static int DoubleToInt(double val)
        {
            return (0 < val) ? (int)(val + 0.5) : (int)(val - 0.5);
        }

        internal static Rect ToRect(NativeMethods.RECT rc)
        {
            Rect rect = new Rect();

            rect.X = rc.left;
            rect.Y = rc.top;
            rect.Width = rc.right - rc.left;
            rect.Height = rc.bottom - rc.top;

            return rect;
        }
    }
}
