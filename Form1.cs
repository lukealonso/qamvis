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
        public Form1()
        {
            InitializeComponent();
        }

		private void DrawGraph(Graphics g, float x, float y, float[] values, float xScale, float yScale, Pen pen)
		{
			for (int i = 1; i < values.Length; i++)
			{
				g.DrawLine(pen, new PointF(x + (float)(i - 1) * xScale, y + (values[i - 1] * 0.5f + 0.5f) * yScale), new PointF(x + (float)i * xScale, y + (values[i] * 0.5f + 0.5f) * yScale));
			}
		}

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
			Graphics g = e.Graphics;
			g.Clear(Color.Black);

			QAM modulator = new QAM(2, 2);
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

			float[] noise = Noise.GenerateNoise(stream.Count, 0.5f);
			List<float> noisyStream = new List<float>();

			for (int i = 0; i < stream.Count; i++)
			{
				noisyStream.Add(noise[i] + stream[i]);
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
			float streamScale = 16.0f;
			float statesPerByte = (float)inStates.Length / (float)inBytes.Length;
			for (int i = 0; i < inString.Length; i++)
			{
				float state = (float)i * statesPerByte;
				g.DrawString(inString[i].ToString(), System.Drawing.SystemFonts.DefaultFont, Brushes.White, new PointF(20.0f + state * streamScale, curY));
			}
			curY += 16.0f;
			for (int i = 0; i < inStates.Length; i++)
			{
				g.DrawString(inStates[i].ToString(), System.Drawing.SystemFonts.DefaultFont, Brushes.White, new PointF(20.0f + (float)i * streamScale, curY));
			}
			curY += 16.0f;
			DrawGraph(g, 20, curY, stream.ToArray(), 0.5f, streamScale * 2.0f, Pens.Aqua);
			curY += 36.0f;
			DrawGraph(g, 20, curY, noise, 0.5f, streamScale, Pens.Red);
			curY += 20.0f;
			DrawGraph(g, 20, curY, noisyStream.ToArray(), 0.5f, streamScale * 2.0f, Pens.Yellow);
			curY += 38.0f;
			for (int i = 0; i < outStates.Count; i++)
			{
				Brush useBrush = outStates[i] != inStates[i] ? Brushes.Red : new SolidBrush(Color.FromArgb(0, 255, 0));
				g.DrawString(outStates[i].ToString(), System.Drawing.SystemFonts.DefaultFont, useBrush, new PointF(20.0f + (float)i * streamScale, curY));
			}
			curY += 16.0f;
			for (int i = 0; i < inString.Length; i++)
			{
				float state = (float)i * statesPerByte;
				Brush useBrush = outString[i] != inString[i] ? Brushes.Red : new SolidBrush(Color.FromArgb(0, 255, 0));
				g.DrawString(outString[i].ToString(), System.Drawing.SystemFonts.DefaultFont, useBrush, new PointF(20.0f + state * streamScale, curY));
			}
			curY += 16.0f;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
			timer1.Interval = 33;
			timer1.Enabled = true;
        }

		private void timer1_Tick(object sender, EventArgs e)
		{
			this.Invalidate();
		}
    }
}
