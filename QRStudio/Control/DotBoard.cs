using QRStudio.Engine.Geom;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace QRStudio.Control
{
    class DotBoard : Canvas
    {
        private bool isDrawing = false;
        private List<Tuple<Rect, Brush>> brushes;

        private double dotSize = 12;
        public double DotSize
        {
            get { return dotSize; }
            set
            {
                dotSize = value;

                Invalidate();
            }
        }

        public DotBoard()
        {
            brushes = new List<Tuple<Rect, Brush>>();
        }

        private void Invalidate()
        {
            if (!isDrawing)
                InvalidateVisual();
        }

        public void SetDot(Rect rect, Brush brush)
        {
            SetDot((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height, brush);
        }

        public void SetDot(int x, int y, int width, int height, Brush brush)
        {
            foreach (Tuple<Rect, Brush> t in new ArrayList(brushes))
            {
                Rect r = t.Item1;

                if (x <= r.X && x + width >= r.Right &&
                    y <= r.Y && y + height >= r.Bottom)
                {
                    brushes.Remove(t);
                }
            }

            brushes.Add(new Tuple<Rect, Brush>(new Rect(x, y, width, height), brush));

            Invalidate();
        }

        public void BeginDraw()
        {
            isDrawing = true;
        }

        public void EndDraw()
        {
            isDrawing = false;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            
            foreach (var tp in brushes)
            {
                dc.DrawRectangle(tp.Item2, null, 
                    new Rect()
                    {
                        X = tp.Item1.X * DotSize,
                        Y = tp.Item1.Y * DotSize,
                        Width = tp.Item1.Width * DotSize,
                        Height = tp.Item1.Height * DotSize
                    });
            }
        }

        public void Clear()
        {
            brushes.Clear();
        }
    }
}