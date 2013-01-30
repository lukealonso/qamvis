using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace QAMVis
{
    public partial class Form1 : Form
    {
		private Random m_Random = new Random();

        public Form1()
        {
            InitializeComponent();
        }

		private void DrawGraph(Graphics g, float x, float y, float[] values, float xScale, float yScale, Pen pen, out int height)
		{
			int minPos = 100000;
			int maxPos = 0;
			for (int i = 1; i < values.Length; i++)
			{
				int yPos = (int)(values[i] * yScale);
				minPos = Math.Min(minPos, yPos);
				maxPos = Math.Max(maxPos, yPos);
			}
			for (int i = 1; i < values.Length; i++)
			{
				g.DrawLine(pen, new PointF(x + (float)(i - 1) * xScale, y + values[i - 1] * yScale - minPos), new PointF(x + (float)i * xScale, y + values[i] * yScale - minPos));
			}
			height = maxPos - minPos;
		}

        private void Form1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
			timer1.Interval = 33;
			timer1.Enabled = true;
        }

		private void timer1_Tick(object sender, EventArgs e)
		{
			paintControl1.Invalidate();
		}

		private void paintControl1_Paint(object sender, PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			g.Clear(Color.Black);

			QAM modulator = new QAM((int)Math.Pow(2.0f, (float)numericUpDown1.Value), (int)Math.Pow(2.0f, (float)numericUpDown2.Value), (int)numericUpDown4.Value);
			/*
			for (var i = 0; i < modulator.NumStates; i++)
			{
				float[] state = modulator.Modulate(i, 64);
				DrawGraph(g, 100.0f, 100 + i * 74.0f, state, 1.0f, 32.0f);
			}
			*/
			string inString = "Hello World!";
			byte[] inBytes = Encoding.ASCII.GetBytes(inString);
			uint[] inStates = modulator.EncodeStates(inBytes);
			List<float> stream = new List<float>();
			int wordLength = 32;
			foreach (uint state in inStates)
			{
				float[] fragment = modulator.Modulate(state, wordLength);
				stream.AddRange(fragment);
			}

			float[] noise = Noise.GenerateNoise(stream.Count, (float)numericUpDown3.Value / 20.0f);
			List<float> noisyStream = new List<float>();
			int shifter = (int)((m_Random.NextDouble() * 2.0f - 1.0f) * 0.0f);
			for (int i = 0; i < stream.Count; i++)
			{
				noisyStream.Add(noise[i] + stream[Math.Max(0, Math.Min(stream.Count - 1, i + shifter))]);
			}

			List<uint> outStates = new List<uint>();
			for (int i = 0; i < noisyStream.Count; i += wordLength)
			{
				float[] fragment = noisyStream.GetRange(i, wordLength).ToArray();
				uint stateOut = modulator.Demodulate(fragment);
				outStates.Add(stateOut);
			}

			byte[] outBytes = modulator.DecodeStates(outStates.ToArray());
			string outString = Encoding.ASCII.GetString(outBytes);

			float curY = 100.0f;
			float streamScale = 0.5f * (float)numericUpDown4.Value;
			float streamSize = wordLength * streamScale;
			float statesPerByte = (float)inStates.Length / (float)inBytes.Length;
			for (int i = 0; i < inString.Length; i++)
			{
				float state = (float)i * statesPerByte;
				g.DrawString(inString[i].ToString(), System.Drawing.SystemFonts.DefaultFont, Brushes.White, new PointF(20.0f + state * streamSize, curY));
			}
			curY += 16.0f;
			for (int i = 0; i < inStates.Length; i++)
			{
				g.DrawString(inStates[i].ToString(), System.Drawing.SystemFonts.DefaultFont, Brushes.White, new PointF(20.0f + (float)i * streamSize, curY));
			}
			int graphMax = 0;
			curY += 26.0f;
			DrawGraph(g, 20, curY, stream.ToArray(), streamScale, 32.0f, Pens.Aqua, out graphMax);
			curY += graphMax + 10.0f;
			DrawGraph(g, 20, curY, noise, streamScale, 32.0f, Pens.Red, out graphMax);
			curY += graphMax + 10.0f;
			DrawGraph(g, 20, curY, noisyStream.ToArray(), streamScale, 32.0f, Pens.Yellow, out graphMax);
			curY += graphMax + 10.0f;
			for (int i = 0; i < outStates.Count; i++)
			{
				Brush useBrush = outStates[i] != inStates[i] ? Brushes.Red : new SolidBrush(Color.FromArgb(0, 255, 0));
				g.DrawString(outStates[i].ToString(), System.Drawing.SystemFonts.DefaultFont, useBrush, new PointF(20.0f + (float)i * streamSize, curY));
			}
			curY += 16.0f;
			for (int i = 0; i < inString.Length; i++)
			{
				float state = (float)i * statesPerByte;
				Brush useBrush = outString[i] != inString[i] ? Brushes.Red : new SolidBrush(Color.FromArgb(0, 255, 0));
				g.DrawString(outString[i].ToString(), System.Drawing.SystemFonts.DefaultFont, useBrush, new PointF(20.0f + state * streamSize, curY));
			}
			curY += 16.0f;

			g.DrawString("amps", System.Drawing.SystemFonts.DefaultFont, Brushes.Yellow, new PointF(5, 25));
			g.DrawString("phases", System.Drawing.SystemFonts.DefaultFont, Brushes.Yellow, new PointF(65, 25));
			g.DrawString("freqs", System.Drawing.SystemFonts.DefaultFont, Brushes.Yellow, new PointF(125, 25));
			g.DrawString("noise", System.Drawing.SystemFonts.DefaultFont, Brushes.Yellow, new PointF(185, 25));
			g.DrawString(modulator.NumStates.ToString() + " states", System.Drawing.SystemFonts.DefaultFont, Brushes.Yellow, new PointF(250, 25));

		}
    }
}
