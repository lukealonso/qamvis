using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QAMVis
{
	public class FFT
	{
		public struct Complex
		{
			public Complex(int f, float r, float i)
			{
				freq = f;
				real = r;
				imag = i;
			}

			public float Angle
			{
				get
				{
					return (float)Math.Atan2(imag, real);
				}
			}

			public float Magnitude
			{
				get
				{
					return (float)Math.Sqrt(real * real + imag * imag);
				}
			}

			public int freq;
			public float real;
			public float imag;
		}

		public static Complex[] DFT(float[] values, float nrm)
		{
			Complex[] coeffs = new Complex[values.Length / 2];
			for (int c = 0; c < coeffs.Length; c++)
			{
				coeffs[c].freq = c;
				for (int x = 0; x < values.Length; x++)
				{
					float t = (float)x / (float)(values.Length - 1);
					coeffs[c].real += (float)Math.Sin(t * Math.PI * 2.0f * (float)c) * values[x] * nrm;
					coeffs[c].imag += (float)Math.Cos(t * Math.PI * 2.0f * (float)c) * values[x] * nrm;
				}
			}
			return coeffs;
		}
	}
}
