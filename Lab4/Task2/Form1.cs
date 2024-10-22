using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Task2
{
    public partial class Form1 : Form
    {
        const int limit = 7;

        Bitmap drawboard;
        private List<Point> points;
        int y1, y2, coeff;
        float offset, falloff;

        int iters;

        public Form1()
        {
            InitializeComponent();
            drawboard = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = drawboard;
            points = new List<Point>();
            checkBox1.Checked = true;
            ClearBitmap();
        }

        void ClearBitmap()
        {
            using (var g = Graphics.FromImage(drawboard))
            {
                g.FillRectangle(new LinearGradientBrush(new Point(0,0),new Point(0,pictureBox1.Height),Color.FromArgb(0,136,151),Color.White),
                    0, 0, pictureBox1.Width, pictureBox1.Height);
                pictureBox1.Invalidate();
            }
        }

        void Iteration()
        {
            List<Point> newPoints = new List<Point>();
            Random r = new Random();
            for (int i = 0; i < points.Count - 1; i++)
            {
                int x = (int)Math.Round((points[i + 1].X - points[i].X) * offset) + points[i].X;
                int y = (points[i].Y + points[i + 1].Y) / 2 + r.Next(-coeff, coeff);
                newPoints.Add(new Point(x,y));
            }
            for (int i = 0; i < newPoints.Count; i++) { 
                points.Insert(i*2+1, newPoints[i]);
            }
            coeff = (int)Math.Round(coeff*falloff);
        }
        void DrawIter()
        {
            ClearBitmap();
            var completePoly = points.Append(new Point(pictureBox1.Width, pictureBox1.Height)).Append(new Point(0,pictureBox1.Height)).ToArray();
            using (var g = Graphics.FromImage(drawboard)) { 
                g.FillPolygon(new SolidBrush(Color.Black), completePoly);
                pictureBox1.Invalidate();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int a,b,c;
            float d,f;
            if (!int.TryParse(textBox1.Text,out a) ||
                !int.TryParse(textBox2.Text, out b) ||
                !int.TryParse(textBox3.Text,out c) ||
                !float.TryParse(textBox4.Text,out d) ||
                !float.TryParse(textBox5.Text, out f))
            {
                MessageBox.Show("Начальные параметры не введены или введены неправильно!");
                return;
            }
            y1 = int.Parse(textBox1.Text);
            y2 = int.Parse(textBox2.Text);
            coeff = int.Parse(textBox3.Text);
            offset = float.Parse(textBox4.Text);
            falloff = float.Parse(textBox5.Text);
            points = new List<Point> { new Point(0, y1), new Point(pictureBox1.Width, y2) };
            DrawIter();
            iters = 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (points.Count == 0) { MessageBox.Show("Сначала нажмите 'Начать генерацию'!"); return; }
            if (iters >= limit && checkBox1.Checked) { MessageBox.Show("Достигнут лимит генерации(можно отключить)!"); return; }
            Iteration();
            DrawIter();
            iters++;
        }
    }
}
