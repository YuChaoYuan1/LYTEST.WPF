using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace LYTest.WPF.Model
{
    [ContentProperty("Target")]
    public class GridLineDecorator : FrameworkElement
    {
        private TreeView _target;
        private DrawingVisual _gridLinesVisual = new DrawingVisual();
        private GridViewHeaderRowPresenter _headerRowPresenter = null;

        public GridLineDecorator()
        {
            AddVisualChild(_gridLinesVisual);
            AddHandler(ScrollViewer.ScrollChangedEvent, new RoutedEventHandler(OnScrollChanged));
        }

        #region GridLineBrush

        /// <summary>
        /// GridLineBrush Dependency Property
        /// </summary>
        public static readonly DependencyProperty GridLineBrushProperty =
            DependencyProperty.Register("GridLineBrush", typeof(Brush), typeof(GridLineDecorator),
                new FrameworkPropertyMetadata(Brushes.LightGray,
                    new PropertyChangedCallback(OnGridLineBrushChanged)));

        /// <summary>
        /// Gets or sets the GridLineBrush property.  This dependency property 
        /// indicates ....
        /// </summary>
        public Brush GridLineBrush
        {
            get { return (Brush)GetValue(GridLineBrushProperty); }
            set { SetValue(GridLineBrushProperty, value); }
        }

        /// <summary>
        /// Handles changes to the GridLineBrush property.
        /// </summary>
        private static void OnGridLineBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GridLineDecorator)d).OnGridLineBrushChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the GridLineBrush property.
        /// </summary>
        protected virtual void OnGridLineBrushChanged(DependencyPropertyChangedEventArgs e)
        {
            DrawGridLines();
        }

        #endregion

        #region Target

        public TreeView Target
        {
            get { return _target; }
            set
            {
                if (_target != value)
                {
                    if (_target != null) Detach();
                    RemoveVisualChild(_target);
                    RemoveLogicalChild(_target);

                    _target = value;

                    AddVisualChild(_target);
                    AddLogicalChild(_target);
                    if (_target != null) Attach();

                    InvalidateMeasure();
                }
            }
        }

        private void GetGridViewHeaderPresenter()
        {
            if (Target == null)
            {
                _headerRowPresenter = null;
                return;
            }
            _headerRowPresenter = Target.FindVisualChild<GridViewHeaderRowPresenter>();
        }

        #endregion

        #region DrawGridLines

        private void DrawGridLines()
        {
            if (Target == null)
            {
                return;
            }
            if (_headerRowPresenter == null)
            {
                return;
            }
            var itemCount = Target.Items.Count;
            if (itemCount == 0)
            {
                return;
            }

            // 获取drawingContext
            var drawingContext = _gridLinesVisual.RenderOpen();
            var startPoint = new Point(0, 0);

            // 为了对齐到像素的计算参数，否则就会看到有些线是模糊的
            var dpiFactor = this.GetDpiFactor();
            var pen = new Pen(GridLineBrush, 1 * dpiFactor);
            var halfPenWidth = pen.Thickness / 2;
            var guidelines = new GuidelineSet();

            // 计算表头的偏移量和大小
            Point headerOffset = _headerRowPresenter.TranslatePoint(startPoint, this);
            Size headerSize = _headerRowPresenter.RenderSize;
            double headerBottomY = headerOffset.Y + headerSize.Height;

            // 计算ScrollViewer的可视区域大小
            var item0 = _target.ItemContainerGenerator.ContainerFromIndex(0);
            if (item0 == null)
            { return; }

            var scrollViewer = item0.FindVisualParent<ScrollViewer>();
            if (scrollViewer == null)
            { return; }

            UIElement contentElement = scrollViewer.Content as UIElement;
            double maxLineX = scrollViewer.ViewportWidth;
            double maxLineY = scrollViewer.ViewportHeight; //headerBottomY +  contentElement.RenderSize.Height;
            double vLineY = 0.0;
            #region 画横线
            for (int i = 0; i < itemCount; i++)
            {
                TreeViewItem treeItem = Target.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
                if (treeItem != null)
                {
                    DrawHorizonLine(treeItem, i, drawingContext, pen, halfPenWidth, guidelines, headerBottomY, maxLineX, maxLineY, 1, out vLineY);
                }
            }
            #endregion
            // 画竖线
            var columns = _headerRowPresenter.Columns;
            var vLineX = headerOffset.X;
            if (vLineY > maxLineY) vLineY = maxLineY;

            foreach (var column in columns)
            {
                var columnWidth = column.GetColumnWidth();
                vLineX += columnWidth;

                if (vLineX > maxLineX) break;

                // 加入参考线，对齐到像素
                guidelines.GuidelinesX.Add(vLineX + halfPenWidth);
                drawingContext.PushGuidelineSet(guidelines);
                drawingContext.DrawLine(pen, new Point(vLineX, headerBottomY), new Point(vLineX, vLineY));
                drawingContext.Pop();
            }

            drawingContext.Close();
        }

        private void DrawHorizonLine(TreeViewItem item, int itemIndex, DrawingContext drawingContext, Pen pen, double halfPenWidth, GuidelineSet guidelines, double headerBottomY, double maxLineX, double maxLineY, int level, out double vLineY)
        {
            vLineY = 0.0;
            Point startPoint = new Point(0, 0);
            #region 先画当前节点
            if (item == null)
            {
                return;
            }
            var renderSize = item.RenderSize;
            var offset = item.TranslatePoint(startPoint, this);

            var hLineX1 = offset.X + 0 * level;//15
            var hLineX2 = offset.X + renderSize.Width;
            var hLineY = offset.Y;
            vLineY = hLineY + renderSize.Height;

            // 小于视图起始位置的不绘制
            if (hLineY > headerBottomY && hLineY <= maxLineY)
            {
                // 如果大于横向宽度，取横向宽度
                if (hLineX2 > maxLineX)
                {
                    hLineX2 = maxLineX;
                }

                // 加入参考线，对齐到像素
                guidelines.GuidelinesY.Add(hLineY + halfPenWidth);
                drawingContext.PushGuidelineSet(guidelines);
                //如果是第一个点,要画顶层的线
                if (itemIndex == 0)
                {
                    drawingContext.DrawLine(pen, new Point(hLineX1, hLineY - renderSize.Height), new Point(hLineX2, hLineY - renderSize.Height));
                }
                drawingContext.DrawLine(pen, new Point(hLineX1, hLineY), new Point(hLineX2, hLineY));
                drawingContext.Pop();
            }
            #endregion
            #region 如果展开了并且有子节点
            if (item.IsExpanded && item.Items != null && item.Items.Count > 0)
            {
                for (int j = 0; j < item.Items.Count; j++)
                {
                    TreeViewItem childItem = item.ItemContainerGenerator.ContainerFromIndex(j) as TreeViewItem;
                    if (item != null)
                    {
                        DrawHorizonLine(childItem, j, drawingContext, pen, halfPenWidth, guidelines, headerBottomY, maxLineX, maxLineY, level + 1, out vLineY);
                    }
                }
            }
            #endregion
        }

        #endregion

        #region Overrides to show Target and grid lines

        protected override int VisualChildrenCount
        {
            get { return Target == null ? 1 : 2; }
        }

        protected override System.Collections.IEnumerator LogicalChildren
        {
            get { yield return Target; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index == 0) return _target;
            if (index == 1) return _gridLinesVisual;
            throw new IndexOutOfRangeException(string.Format("Index of visual child '{0}' is out of range", index));
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (Target != null)
            {
                Target.Measure(availableSize);
                return Target.DesiredSize;
            }

            return base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Target != null)
                Target.Arrange(new Rect(new Point(0, 0), finalSize));

            return base.ArrangeOverride(finalSize);
        }

        #endregion

        #region Handle Events

        private void Attach()
        {
            _target.Loaded += OnTargetLoaded;
            _target.Unloaded += OnTargetUnloaded;
            _target.SizeChanged += OnTargetSizeChanged;
        }

        private void Detach()
        {
            _target.Loaded -= OnTargetLoaded;
            _target.Unloaded -= OnTargetUnloaded;
            _target.SizeChanged -= OnTargetSizeChanged;
        }

        private void OnTargetLoaded(object sender, RoutedEventArgs e)
        {
            if (_headerRowPresenter == null)
                GetGridViewHeaderPresenter();
            DrawGridLines();
        }

        private void OnTargetUnloaded(object sender, RoutedEventArgs e)
        {
            DrawGridLines();
        }

        private void OnTargetSizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawGridLines();
        }

        private void OnScrollChanged(object sender, RoutedEventArgs e)
        {
            DrawGridLines();
        }

        #endregion
    }
}
