using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using QRStudio.Engine.Codec;
using tf = QRStudio.Filter;
using TransformDirection = QRStudio.Filter.DirectionFilter.Direction;
using QRMode = QRStudio.Engine.Codec.QRCodeEncoder.ENCODE_MODE;
using QRErrorMode = QRStudio.Engine.Codec.QRCodeEncoder.ERROR_CORRECTION;

using QRStudio.Util;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Collections;

namespace QRStudio.Control
{
    [TemplatePart(Name = "PART_Canvas", Type = typeof(Canvas))]
    [TemplatePart(Name = "PART_Focus", Type = typeof(Rectangle))]
    [TemplatePart(Name = "PART_Range", Type = typeof(Rectangle))]
    [TemplatePart(Name = "PART_DotBoard", Type = typeof(DotBoard))]
    [TemplatePart(Name = "PART_Border", Type = typeof(Border))]
    class QRCanvas : System.Windows.Controls.Control
    {
        public enum CanvasMode
        {
            Element,
            Clipping
        }

        #region [ Geometry Data ]
        private static StreamGeometry
            geo_Blank = StreamGeometryEx.Create();

        private static StreamGeometry
            geo_Rect = StreamGeometryEx.Create(
                new GeoPoint(0, 0), new GeoPoint(1, 0),
                new GeoPoint(1, 1), new GeoPoint(0, 1));

        private static StreamGeometry
            geo_Left = StreamGeometryEx.Create(
                new GeoPoint(0, 0), new GeoPoint(1, 0),
                new GeoPoint(1, 1), new GeoPoint(0.65, 1)),

            geo_Top = StreamGeometryEx.Create(
                new GeoPoint(1, 0), new GeoPoint(1, 1),
                new GeoPoint(0, 1), new GeoPoint(0, 0.65)),

            geo_Right = StreamGeometryEx.Create(
                new GeoPoint(0, 0), new GeoPoint(0.35, 0),
                new GeoPoint(1, 1), new GeoPoint(0, 1)),

            geo_Bottom = StreamGeometryEx.Create(
                new GeoPoint(0, 0), new GeoPoint(1, 0),
                new GeoPoint(1, 0.35), new GeoPoint(0, 1));

        private static StreamGeometry
            geo_LT = StreamGeometryEx.Create(
                new GeoPoint(1, 0), new GeoPoint(1, 1),
                new GeoPoint(0, 1), new GeoPoint(0, 0.44771526, true),
                new GeoPoint(0.44771526, 0), new GeoPoint(1, 0)),

            geo_LB = StreamGeometryEx.Create(
                new GeoPoint(0, 0), new GeoPoint(1, 0),
                new GeoPoint(1, 1), new GeoPoint(0.44771528, 1, true),
                new GeoPoint(0, 0.55228466), new GeoPoint(0, 0)),

            geo_RT = StreamGeometryEx.Create(
                new GeoPoint(0, 0), new GeoPoint(0.55228472, 0, true),
                new GeoPoint(1, 0.44771528), new GeoPoint(1, 1),
                new GeoPoint(0, 1)),

            geo_RB = StreamGeometryEx.Create(
                new GeoPoint(0, 0), new GeoPoint(1, 0),
                new GeoPoint(1, 0.55228478, true), new GeoPoint(0.55228472, 1),
                new GeoPoint(0, 1));

        private Dictionary<TransformDirection, StreamGeometry> geoPreset = new Dictionary<TransformDirection, StreamGeometry>()
        {
            { 0, geo_Blank },
            { TransformDirection.Left, geo_Left },
            { TransformDirection.Top, geo_Top },
            { TransformDirection.Right, geo_Right },
            { TransformDirection.Bottom, geo_Bottom },

            { TransformDirection.Left | TransformDirection.Top, geo_LT },
            { TransformDirection.Left | TransformDirection.Bottom, geo_LB },
            { TransformDirection.Right | TransformDirection.Top, geo_RT },
            { TransformDirection.Right | TransformDirection.Bottom, geo_RB },
        };
        #endregion

        public static readonly DependencyProperty DotSizeProperty =
            DependencyProperty.Register(nameof(DotSize), typeof(double), typeof(QRCanvas), new PropertyMetadata(12.0, new PropertyChangedCallback(DotSizeChanged)));

        public static readonly DependencyProperty SpaceProperty =
            DependencyProperty.Register(nameof(Space), typeof(double), typeof(QRCanvas), new PropertyMetadata(1.0));

        public static readonly DependencyProperty TransParentProperty =
            DependencyProperty.Register(nameof(TransParent), typeof(bool), typeof(QRCanvas), new PropertyMetadata(false));

        public double DotSize
        {
            get { return (double)GetValue(DotSizeProperty); }
            set { SetValue(DotSizeProperty, value); }
        }

        public double Space
        {
            get { return (double)GetValue(SpaceProperty); }
            set { SetValue(SpaceProperty, value); }
        }

        public bool TransParent
        {
            get { return (bool)GetValue(TransParentProperty); }
            set { SetValue(TransParentProperty, value); }
        }

        public Size QRSize { get; private set; } = new Size(0, 0);

        private CanvasMode renderMode = CanvasMode.Clipping;
        public CanvasMode RenderMode
        {
            get { return renderMode; }
            set
            {
                renderMode = value;

                if (pCanvas == null)
                    return;

                if (value == CanvasMode.Clipping)
                {
                    gGroup = gGroup ?? new GeometryGroup();

                    pCanvas.Clip = gGroup;
                }
                else
                {
                    pCanvas.Clip = null;
                }
            }
        }

        public int Version { get; set; } = 3;

        public Rect? SelectedRange { get; private set; } = null;

        private GeometryGroup gGroup = null;
        private Canvas pCanvas = null;
        private Rectangle pFocus = null;
        private Rectangle pRange = null;
        private DotBoard pBoard = null;
        private Border pBorder = null;

        private byte[][] lastMatrix = null;
        private byte[][] matrixBuffer = null;  // 가변용 매트릭스
        private Geometry[][] geoBuffer = null; // 가변용 지오메트리

        private bool isMouseDown = false;
        private Point rangeStart;

        private ScaleTransform scaleTransform;

        public QRCanvas()
        {
            this.PreviewMouseDown += QRCanvas_BeginDrag;
            this.PreviewMouseUp += QRCanvas_EndDrag;
            this.PreviewMouseMove += QRCanvas_PreviewMouseMove;
            this.MouseLeave += QRCanvas_MouseLeave;

            scaleTransform = new ScaleTransform();
            var binding = new Binding("DotSize")
            {
                Source = this
            };

            BindingOperations.SetBinding(scaleTransform, ScaleTransform.ScaleXProperty, binding);
            BindingOperations.SetBinding(scaleTransform, ScaleTransform.ScaleYProperty, binding);
        }

        #region [ Focus ]
        private void QRCanvas_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!isMouseDown && pFocus != null)
            {
                Canvas.SetLeft(pFocus, -DotSize);
                Canvas.SetTop(pFocus, -DotSize);

                pFocus.Visibility = Visibility.Collapsed;
            }
        }

        private void QRCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (pFocus != null && lastMatrix != null)
            {
                Point pt = SnapToDot(e.GetPosition(pCanvas));

                if (!isMouseDown)
                {
                    pFocus.SetSize(DotSize, DotSize);

                    pFocus.Margin = new Thickness(pt.X, pt.Y, 0, 0);
                    pFocus.Visibility = Visibility.Visible;
                }
                else
                {
                    pt.Offset(DotSize, DotSize);

                    Point delta = pt - (Vector)rangeStart;
                    var margin = new Thickness();

                    if (delta.X > 0)
                    {
                        margin.Left = rangeStart.X;
                    }
                    else
                    {
                        delta.X -= DotSize * 2;
                        margin.Left = pt.X - DotSize;
                    }

                    if (delta.Y > 0)
                    {
                        margin.Top = rangeStart.Y;
                    }
                    else
                    {
                        delta.Y -= DotSize * 2;
                        margin.Top = pt.Y - DotSize;
                    }

                    pFocus.SetSize(Math.Abs(delta.X), Math.Abs(delta.Y));
                    pFocus.Margin = margin;
                }
            }

            e.Handled = true;
        }
        #endregion

        #region [ Drag ]
        private void QRCanvas_BeginDrag(object sender, MouseButtonEventArgs e)
        {
            if (!isMouseDown && e.LeftButton == MouseButtonState.Pressed)
            {
                this.CaptureMouse();

                isMouseDown = true;

                rangeStart = SnapToDot(e.GetPosition(pCanvas));
                pRange.Visibility = Visibility.Collapsed;
            }
        }

        private void QRCanvas_EndDrag(object sender, MouseButtonEventArgs e)
        {
            if (isMouseDown)
            {
                isMouseDown = false;

                // Selection Swap
                EndSelectRange(pFocus);

                pRange.Margin = pFocus.Margin;
                pRange.SetSize(pFocus);
                pRange.Visibility = Visibility.Visible;

                pFocus.Visibility = Visibility.Collapsed;
                pFocus.SetSize(DotSize, DotSize);

                this.ReleaseMouseCapture();
            }
        }

        private void EndSelectRange(FrameworkElement target)
        {
            Point pt1 = new Point(target.Margin.Left / DotSize, target.Margin.Top / DotSize);
            Point pt2 = pt1 + new Vector(target.Width / DotSize, target.Height / DotSize);

            SelectedRange = new Rect(pt1, pt2);
        }
        #endregion

        public BitmapSource ToBitmap(double dpiX = 96, double dpiY = 96)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(pCanvas);
            var spaceSize = new Vector(DotSize * 2 * Space, DotSize * 2 * Space);

            RenderTargetBitmap rtb = new RenderTargetBitmap(
                (int)((pCanvas.ActualWidth + spaceSize.X) * dpiX / 96.0),
                (int)((pCanvas.ActualHeight + spaceSize.Y) * dpiY / 96.0),
                dpiX, dpiY, PixelFormats.Pbgra32);

            var dv = new DrawingVisual();

            using (var ctx = dv.RenderOpen())
            {
                if (!TransParent)
                {
                    ctx.DrawRectangle(Brushes.White, null,
                                        new Rect(new Point(),
                                        new Size(bounds.Width + spaceSize.X, bounds.Height + spaceSize.Y)));
                }

                var vb = new VisualBrush(pCanvas);
                ctx.DrawRectangle(vb, null, new Rect((Point)(spaceSize / 2), bounds.Size));
            }
            
            rtb.Render(dv);

            return rtb;
        }

        public void Update()
        {
            var buffer = new ArrayList(gGroup.Children);
            pCanvas.Children.Clear();
            gGroup.Children.Clear();

            foreach (Geometry g in buffer)
            {
                gGroup.Children.Add(g);
            }

            UpdateDotSize(DotSize + 1);
            UpdateDotSize(DotSize);
        }

        private Point SnapToDot(Point pos)
        {
            Size qrSz = QRSize;

            return new Point(
                (int)(Math.Max(Math.Min(pos.X / DotSize, qrSz.Width - 1), 0)) * DotSize,
                (int)(Math.Max(Math.Min(pos.Y / DotSize, qrSz.Height - 1), 0)) * DotSize);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            pCanvas = GetPart<Canvas>("PART_Canvas");
            pFocus = GetPart<Rectangle>("PART_Focus");
            pRange = GetPart<Rectangle>("PART_Range");
            pBoard = GetPart<DotBoard>("PART_DotBoard");
            pBorder = GetPart<Border>("PART_Border");

            RenderMode = CanvasMode.Clipping;
        }

        public void SetBrush(Brush brush, bool isOverride)
        {
            if (SelectedRange.HasValue)
            {
                Rect range = SelectedRange.Value;

                pBoard.SetDot(range, brush);

                for (int x = (int)range.X; x < range.Right; x++)
                {
                    for (int y = (int)range.Y; y < range.Bottom; y++)
                    {
                        if (isOverride)
                        {
                            // 1로 오버라이드
                            if (matrixBuffer[x][y] != 1)
                            {
                                if (matrixBuffer[x][y] > 0)
                                    gGroup.Children.Remove(geoBuffer[x][y]);

                                matrixBuffer[x][y] = 1;

                                geoBuffer[x][y] = CreateDotClip(x, y, 1);
                                gGroup.Children.Add(geoBuffer[x][y]);
                            }
                        }
                        else
                        {
                            // 복원
                            if (matrixBuffer[x][y] != lastMatrix[x][y])
                            {
                                matrixBuffer[x][y] = lastMatrix[x][y];

                                gGroup.Children.Remove(geoBuffer[x][y]);

                                if (matrixBuffer[x][y] > 0)
                                {
                                    geoBuffer[x][y] = CreateDotClip(x, y, matrixBuffer[x][y]);
                                    gGroup.Children.Add(geoBuffer[x][y]);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void Apply(string content)
        {
            var encoder = new QRCodeEncoder()
            {
                EncodeMode = QRMode.BYTE,
                ErrorMode = QRErrorMode.M,
                Version = this.Version
            };

            byte[][] matrix = encoder.Generate(content, Encoding.UTF8)
                .Cast2D(b => (byte)(b ? 1 : 0));

            var et = new tf.EdgeFilter(matrix);
            et.Filters.Add(new tf.DirectionFilter());

            lastMatrix = et.Translate();
            QRSize = new Size(lastMatrix.Length, lastMatrix[0].Length);

            matrixBuffer = lastMatrix.Copy2D();

            ApplyQR(lastMatrix);

            pBoard.Clear();
            pBoard.SetDot(new Rect(0, 0, QRSize.Width, QRSize.Height), Background);

            GC.Collect();
        }

        private void UIInvoke(Action action)
        {
            Dispatcher.Invoke(action);
        }

        public void UpdateDotSize(double? fakeSize = null)
        {
            double ds = (fakeSize.HasValue ? fakeSize.Value : DotSize);

            if (lastMatrix != null)
            {
                int width = lastMatrix.Length;
                int height = lastMatrix[0].Length;

                pFocus.SetSize(ds, ds);
                pCanvas.SetSize(width * ds, height * ds);
                pBoard.SetSize(pCanvas);

                pBoard.DotSize = ds;

                if (SelectedRange.HasValue)
                {
                    pRange.Margin = new Thickness()
                    {
                        Left = SelectedRange.Value.Left * ds,
                        Top = SelectedRange.Value.Top * ds
                    };
                    pRange.SetSize(SelectedRange.Value.Width * ds, SelectedRange.Value.Height * ds);
                }
            }
        }

        private static void DotSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as QRCanvas).UpdateDotSize();
        }

        #region [ Rendering ]
        private void ApplyQR(byte[][] matrix)
        {
            pCanvas.Children.Clear();
            gGroup.Children.Clear();

            int width = matrix.Length;
            int height = matrix[0].Length;

            geoBuffer = new Geometry[width][];

            for (int x = 0; x < width; x++)
            {
                geoBuffer[x] = new Geometry[height];

                for (int y = 0; y < height; y++)
                {
                    if (RenderMode == CanvasMode.Element)
                    {
                        // # element
                        var dot = CreateDot(x, y, matrix[x][y]);
                        pCanvas.Children.Add(dot);
                    }
                    else
                    {
                        // # clip
                        var dotClip = CreateDotClip(x, y, matrix[x][y]);

                        if (dotClip != null)
                        {
                            gGroup.Children.Add(dotClip);
                            geoBuffer[x][y] = dotClip;
                        }
                    }
                }
            }

            pFocus.SetSize(DotSize, DotSize);
            pCanvas.SetSize(width * DotSize, height * DotSize);
            pBoard.SetSize(pCanvas);
        }

        private Path CreateDot(int x, int y, byte v, Brush brush = null)
        {
            var p = new Path()
            {
                Data = CreateDotGeometry(v),
                Fill = (v > 0 ? (brush ?? Brushes.Black) : Brushes.Transparent),
                Width = DotSize,
                Height = DotSize,
                Stretch = Stretch.Fill
            };

            Canvas.SetTop(p, y * DotSize);
            Canvas.SetLeft(p, x * DotSize);

            return p;
        }

        private Geometry CreateDotClip(int x, int y, byte v)
        {
            if (v > 0)
            {
                var g = CreateDotGeometry(v)?.Clone();

                g.Transform = new TransformGroup()
                {
                    Children =
                    {
                        new TranslateTransform(x, y),
                        scaleTransform
                    }
                };

                return g;
            }
            return null;
        }

        private Geometry CreateDotGeometry(byte v)
        {
            var td = (TransformDirection)v;

            if (geoPreset.ContainsKey(td))
            {
                return geoPreset[td];
            }
            else
            {
                return geo_Rect;
            }
        }
        #endregion

        T GetPart<T>(string partName) where T : DependencyObject, new()
        {
            var f = GetTemplateChild(partName);

            if (f != null)
                return f as T;

            return default(T);
        }

        public void UnSelect()
        {
            SelectedRange = null;

            pRange.Visibility = Visibility.Collapsed;
        }
    }
}