using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        string Axiom;
        float angleShift;
        float startAngle;
        int colorSpread;
        int RandSeed;

        Dictionary<char, string> ruleMap = new Dictionary<char, string>();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            read();
            draw(trackBar1.Value);
        }

        void read()
        {
            RandSeed = Environment.TickCount;
            openFileDialog1.ShowDialog(this);
            if (File.Exists(openFileDialog1.FileName))
            {
                ruleMap.Clear();
                string[] rawFile = File.ReadAllLines(openFileDialog1.FileName);
                if (rawFile.Length == 0)
                {
                    MessageBox.Show("Bad file");
                }
                else
                {
                    string[] split = rawFile[0].Split(' ');
                    if (split.Length < 3 || !float.TryParse(split[1], out angleShift) || !float.TryParse(split[2], out startAngle))
                    {
                        MessageBox.Show("Bad 1st line");
                    }
                    if (split.Length == 4)
                    {
                        int.TryParse(split[3], out colorSpread);
                    }
                    Axiom = split[0];
                    StringBuilder errorBuilder = new StringBuilder();
                    for (int i = 1; i < rawFile.Length;i++)
                    {
                        if (rawFile[i][1] == '-' && rawFile[i][2] == '>' && !ruleMap.Keys.Contains(rawFile[i][0]))
                            ruleMap.Add(rawFile[i][0], rawFile[i].Substring(3));
                        else errorBuilder.Append($"Bad {i+1} line\n");
                    }
                    if (errorBuilder.Length > 0)
                    MessageBox.Show(errorBuilder.ToString());
                }
            }
        }

        void draw(int maxDepth)
        {

            StringBuilder LineBuilder = new StringBuilder();
            //just draw as if each line has a length of 1 and scale
            int Depth = maxDepth;
            UnicodeEncoding UnicodeEncoding = new UnicodeEncoding();
            Stack<PointF> pointStack = new Stack<PointF>();
            Stack<double> angleStack = new Stack<double>();
            Stack<MemoryStream> mainStack = new Stack<MemoryStream>();
            mainStack.Push(new MemoryStream(UnicodeEncoding.GetBytes(Axiom)));
            byte[] buffer = new byte[2];
            PointF currentPos = new PointF();
            double currentAngle = startAngle * Math.PI/180;
            int skipdepth = 0;
            PointF MaxPoint = new PointF(0,0);
            PointF MinPoint = new PointF(0,0);
            Random random1 = new Random(RandSeed);
            while (mainStack.Count > 0)
            {
                int currentcode = mainStack.Peek().Read(buffer, 0, 2);
                if (currentcode == 0)
                {
                    mainStack.Pop();
                    Depth++;
                    continue;
                }
                char current = BitConverter.ToChar(buffer, 0);
                if (skipdepth > 0)
                {
                    switch (current)
                    {
                        case '(':
                            skipdepth++;
                            break;
                        case ')':
                            skipdepth--;
                            break;
                    }
                }
                else
                {
                    switch (current)
                    {
                        case '+':
                            currentAngle += angleShift * Math.PI / 180;
                            break;
                        case '-':
                            currentAngle -= angleShift * Math.PI / 180;
                            break;
                        case '>':
                            currentAngle += (random1.Next() % angleShift + (float)random1.NextDouble()) * Math.PI / 180;
                            break;
                        case '<':
                            currentAngle -= (random1.Next() % angleShift + (float)random1.NextDouble()) * Math.PI / 180;
                            break;
                        case '[':
                            pointStack.Push(currentPos);
                            angleStack.Push(currentAngle);
                            break;
                        case ']':
                            currentPos = pointStack.Pop();
                            currentAngle = angleStack.Pop();
                            break;
                        case '(': break; case ')': break;
                        case '.':
                            StringBuilder builder = new StringBuilder();
                            builder.Append("0.");
                            currentcode = mainStack.Peek().Read(buffer, 0, 2);
                            if (currentcode == 0)
                                throw new Exception();
                            current = BitConverter.ToChar(buffer, 0);
                            while (char.IsDigit(current))
                            {
                                builder.Append(current);
                                currentcode = mainStack.Peek().Read(buffer, 0, 2);
                                if (currentcode != 2)
                                    throw new Exception();
                                current = BitConverter.ToChar(buffer, 0);
                            }
                            if (Double.Parse(builder.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture) > random1.NextDouble())
                            {
                                mainStack.Peek().Position -= 2;
                            }
                            else
                            {
                                if (current == '(')
                                    skipdepth++;
                            }
                            break;
                        case 'F':
                            float linescale = checkBox1.Checked ? (Depth + 1) : 1;
                            if (ruleMap.ContainsKey(current) && Depth > 0)
                            {
                                mainStack.Push(new MemoryStream(UnicodeEncoding.GetBytes(ruleMap[current])));
                                Depth--;
                                //currentPos = new PointF(currentPos.X + (linescale) * (float)Math.Cos(currentAngle), currentPos.Y + (linescale) * (float)Math.Sin(currentAngle));
                                MinPoint = new PointF(Math.Min(MinPoint.X, currentPos.X), Math.Min(MinPoint.Y, currentPos.Y));
                                MaxPoint = new PointF(Math.Max(MaxPoint.X, currentPos.X), Math.Max(MaxPoint.Y, currentPos.Y));
                            }
                            else
                            {
                                currentPos = new PointF(currentPos.X + (linescale) * (float)Math.Cos(currentAngle), currentPos.Y + (linescale) * (float)Math.Sin(currentAngle));
                                MinPoint = new PointF(Math.Min(MinPoint.X, currentPos.X), Math.Min(MinPoint.Y, currentPos.Y));
                                MaxPoint = new PointF(Math.Max(MaxPoint.X, currentPos.X), Math.Max(MaxPoint.Y, currentPos.Y));
                            }
                            break;
                        default:
                            if (ruleMap.ContainsKey(current) && Depth > 0)
                            {
                                mainStack.Push(new MemoryStream(UnicodeEncoding.GetBytes(ruleMap[current])));
                                Depth--;
                            }
                            break;
                    }
                }
            }
            double W = Math.Max(1, Math.Ceiling(MaxPoint.X) - Math.Floor(MinPoint.X));
            double H = Math.Max(1, Math.Ceiling(MaxPoint.Y) - Math.Floor(MinPoint.Y));
            float scale = (float)Math.Min(pictureBox1.Width/W,pictureBox1.Height/H);
            Bitmap image = new Bitmap((int)(W*scale) + 20, (int)(H*scale) + 20);
            currentPos = new PointF(-MinPoint.X*scale+10, -MinPoint.Y*scale+10); 
            currentAngle = startAngle * Math.PI / 180;
            random1 = new Random(RandSeed);
            Random random2 = new Random();
            mainStack = new Stack<MemoryStream>();
            mainStack.Push(new MemoryStream(UnicodeEncoding.GetBytes(Axiom)));
            pointStack = new Stack<PointF>();
            angleStack = new Stack<double>();
            Graphics g = Graphics.FromImage(image);
            Depth = maxDepth;
            while (mainStack.Count > 0)
            {
                int currentcode = mainStack.Peek().Read(buffer, 0, 2);
                if (currentcode == 0)
                {
                    mainStack.Pop();
                    Depth++;
                    continue;
                }
                char current = BitConverter.ToChar(buffer, 0);
                if (skipdepth > 0)
                {
                    switch (current)
                    {
                        case '(':
                            skipdepth++;
                            break;
                        case ')':
                            skipdepth--;
                            break;
                    }
                }
                else
                {
                    switch (current)
                    {
                        case '+':
                            currentAngle += angleShift * Math.PI / 180;
                            break;
                        case '-':
                            currentAngle -= angleShift * Math.PI / 180;
                            break;
                        case '>':
                            currentAngle += (random1.Next() % angleShift + (float)random1.NextDouble()) * Math.PI / 180;
                            break;
                        case '<':
                            currentAngle -= (random1.Next() % angleShift + (float)random1.NextDouble()) * Math.PI / 180;
                            break;
                        case '[':
                            pointStack.Push(currentPos);
                            angleStack.Push(currentAngle);
                            break;
                        case ']':
                            currentPos = pointStack.Pop();
                            currentAngle = angleStack.Pop();
                            break;
                        case '(': break;
                        case ')': break;
                        case '.':
                            StringBuilder builder = new StringBuilder();
                            builder.Append("0.");
                            currentcode = mainStack.Peek().Read(buffer, 0, 2);
                            if (currentcode == 0)
                                throw new Exception();
                            current = BitConverter.ToChar(buffer, 0);
                            while (char.IsDigit(current))
                            {
                                builder.Append(current);
                                currentcode = mainStack.Peek().Read(buffer, 0, 2);
                                if (currentcode != 2)
                                    throw new Exception();
                                current = BitConverter.ToChar(buffer, 0);
                            }
                            if (Double.Parse(builder.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture) > random1.NextDouble())
                            {
                                mainStack.Peek().Position -= 2;
                            }
                            else
                            {
                                if (current == '(')
                                    skipdepth++;
                            }
                            break;
                        case 'F':
                            float linescale = checkBox1.Checked ? (Depth + 1) : 1;
                            if (ruleMap.ContainsKey(current) && Depth > 0)
                            {
                                mainStack.Push(new MemoryStream(UnicodeEncoding.GetBytes(ruleMap[current])));
                                Depth--;
                                PointF oldpos = currentPos;
                                //currentPos = new PointF(currentPos.X + (linescale) * (float)scale * (float)Math.Cos(currentAngle), currentPos.Y + (linescale) * (float)scale * (float)Math.Sin(currentAngle));
                                int CS = random2.Next(-colorSpread, colorSpread + 1);
                                int R = Math.Min(Math.Max((colorDialog1.Color.R - colorDialog2.Color.R) / (maxDepth + 1) * (Depth + 1) + colorDialog2.Color.R + CS, 0), 255);
                                int G = Math.Min(Math.Max((colorDialog1.Color.G - colorDialog2.Color.G) / (maxDepth + 1) * (Depth + 1) + colorDialog2.Color.G + CS, 0), 255);
                                int B = Math.Min(Math.Max((colorDialog1.Color.B - colorDialog2.Color.B) / (maxDepth + 1) * (Depth + 1) + colorDialog2.Color.B + CS, 0), 255);
                                //g.DrawLine(new Pen(Color.FromArgb(R, G, B), linescale), oldpos, currentPos);
                            }
                            else
                            {
                                PointF oldpos = currentPos;
                                currentPos = new PointF(currentPos.X + (linescale) * (float)scale * (float)Math.Cos(currentAngle), currentPos.Y + (linescale) * (float)scale * (float)Math.Sin(currentAngle));
                                int CS = random2.Next(-colorSpread, colorSpread + 1);
                                int R = Math.Min(Math.Max((colorDialog1.Color.R - colorDialog2.Color.R) / (maxDepth + 1) * (Depth + 1) + colorDialog2.Color.R + CS, 0), 255);
                                int G = Math.Min(Math.Max((colorDialog1.Color.G - colorDialog2.Color.G) / (maxDepth + 1) * (Depth + 1) + colorDialog2.Color.G + CS, 0), 255);
                                int B = Math.Min(Math.Max((colorDialog1.Color.B - colorDialog2.Color.B) / (maxDepth + 1) * (Depth + 1) + colorDialog2.Color.B + CS, 0), 255);
                                g.DrawLine(new Pen(Color.FromArgb(R, G, B), linescale), oldpos, currentPos);
                            }
                            break;
                        default:
                            if (ruleMap.ContainsKey(current) && Depth > 0)
                            {
                                mainStack.Push(new MemoryStream(UnicodeEncoding.GetBytes(ruleMap[current])));
                                Depth--;
                            }
                            break;
                    }
                }
            }
            pictureBox1.Image = image;
        }

        private void Redraw(object sender, EventArgs e)
        {
            RandSeed = Environment.TickCount;
            draw(trackBar1.Value);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            button3.BackColor = colorDialog1.Color;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            colorDialog2.ShowDialog();
            button4.BackColor = colorDialog2.Color;
        }
    }
}
