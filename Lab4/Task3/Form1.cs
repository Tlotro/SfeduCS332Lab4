using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp4
{
    public partial class Form1 : Form
    {
        private List<PointF> points = new List<PointF>();
        private int selectedPoint = -1;
        private bool isDragging = false;

        public Form1()
        {
            InitializeComponent();
            pictureBox1.MouseClick += PictureBox1_MouseClick;
            pictureBox1.MouseMove += PictureBox1_MouseMove;
            pictureBox1.MouseDown += PictureBox1_MouseDown;
            pictureBox1.MouseUp += PictureBox1_MouseUp;
            pictureBox1.Paint += PictureBox1_Paint;
        }

        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            DrawPoints(e.Graphics);
            DrawBezierCurve(e.Graphics);
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && selectedPoint != -1)
            {
                points[selectedPoint] = e.Location;
                pictureBox1.Invalidate();
            }
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                SelectPoint(e.Location);
                if (selectedPoint != -1)
                {
                    isDragging = true;
                }
            }
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
                selectedPoint = -1;
            }
        }

        private void PictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && !isDragging)
            {
                AddPoint(e.Location);
            }
            else if (e.Button == MouseButtons.Right)
            {
                RemovePoint(e.Location);
            }
        }

        private void AddPoint(PointF point)
        {
            points.Add(point);
            pictureBox1.Invalidate();
        }

        private void RemovePoint(PointF point)
        {
            for (int i = 0; i < points.Count; i++)
            {
                if (Distance(points[i], point) < 10)
                {
                    points.RemoveAt(i);
                    pictureBox1.Invalidate();
                    return;
                }
            }
        }

        private void SelectPoint(PointF point)
        {
            for (int i = 0; i < points.Count; i++)
            {
                if (Distance(points[i], point) < 10)
                {
                    selectedPoint = i;
                    return;
                }
            }
            selectedPoint = -1;
        }

        private void DrawPoints(Graphics g)
        {
            foreach (var p in points)
            {
                g.FillEllipse(Brushes.Red, p.X - 4, p.Y - 4, 8, 8);
            }
        }

        private void DrawBezierCurve(Graphics g)
        {
            if (points.Count < 4) return;

            for (int i = 0; i < points.Count - 3; i += 3)
            {
                DrawCubicBezier(g, points[i], points[i + 1], points[i + 2], points[i + 3]);
            }
        }

        private void DrawCubicBezier(Graphics g, PointF p0, PointF p1, PointF p2, PointF p3)
        {
            int steps = 30;
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                PointF point = CalculateBezierPoint(t, p0, p1, p2, p3);
                g.FillEllipse(Brushes.Green, point.X - 2, point.Y - 2, 4, 4);
            }
        }

        private PointF CalculateBezierPoint(float t, PointF p0, PointF p1, PointF p2, PointF p3)
        {
            float q = 1 - t;

            PointF p = new PointF(
                p0.X * q * q * q + 3 * p1.X * q * q * t + 3 * p2.X * q * t * t + p3.X * t * t * t,
                p0.Y * q * q * q + 3 * p1.Y * q * q * t + 3 * p2.Y * q * t * t + p3.Y * t * t * t
            );

            return p;
        }

        private float Distance(PointF p1, PointF p2)
        {
            return (float)Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }
    }
}
