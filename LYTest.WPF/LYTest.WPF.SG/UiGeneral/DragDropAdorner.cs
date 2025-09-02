using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace LYTest.WPF.SG.UiGeneral
{
    public class DragDropAdorner : Adorner
    {
        readonly FrameworkElement mDraggedElement;
        public DragDropAdorner(UIElement parent)
            : base(parent)
        {
            IsHitTestVisible = false; // Seems Adorner is hit test visible?
            mDraggedElement = parent as FrameworkElement;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (mDraggedElement != null)
            {
                Win32.POINT screenPos = new Win32.POINT();
                if (Win32.GetCursorPos(ref screenPos))
                {
                    Point pos = PointFromScreen(new Point(screenPos.X, screenPos.Y));
                    Rect rect = new Rect(pos.X, pos.Y, mDraggedElement.ActualWidth, mDraggedElement.ActualHeight);
                    drawingContext.PushOpacity(1.0);
                    if (mDraggedElement.TryFindResource(SystemColors.HighlightBrushKey) is Brush highlight)
                        drawingContext.DrawRectangle(highlight, new Pen(Brushes.Transparent, 0), rect);
                    drawingContext.DrawRectangle(new VisualBrush(mDraggedElement),new Pen(Brushes.Transparent, 0), rect);
                    drawingContext.Pop();
                }
            }
        }

    }
}
