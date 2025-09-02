using System.Windows.Interactivity;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;

namespace LYTest.DataManager.SG.Controls
{
    public class MoveBehavior : Behavior<UIElement>
    {
        private Canvas canvas;

        private bool isDragging = false;

        private Point mouseOffset;

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.MouseLeftButtonDown += 
                
                new MouseButtonEventHandler(AssociatedObject_MouseLeftButtonDown);

            AssociatedObject.MouseMove += 
                
                new MouseEventHandler(AssociatedObject_MouseMove);

            AssociatedObject.MouseLeftButtonUp += 
                
                new MouseButtonEventHandler(AssociatedObject_MouseRightButtonUp);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.MouseLeftButtonDown -=
                
                new MouseButtonEventHandler(AssociatedObject_MouseLeftButtonDown);

            AssociatedObject.MouseMove -=

                new MouseEventHandler(AssociatedObject_MouseMove);

            AssociatedObject.MouseLeftButtonUp -=

                new MouseButtonEventHandler(AssociatedObject_MouseRightButtonUp);
        }

        void AssociatedObject_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging)
            {
                AssociatedObject.ReleaseMouseCapture();

                isDragging = false;
            }
        }

        void AssociatedObject_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point point = e.GetPosition(canvas);

                AssociatedObject.SetValue(Canvas.TopProperty, point.Y - mouseOffset.Y);

                AssociatedObject.SetValue(Canvas.LeftProperty, point.X - mouseOffset.X);
            }
        }

        void AssociatedObject_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (canvas == null)
            {
                canvas = VisualTreeHelper.GetParent(AssociatedObject) as Canvas;
            }

            isDragging = true;

            mouseOffset = e.GetPosition(AssociatedObject);

            AssociatedObject.CaptureMouse();
        }
    }
}
