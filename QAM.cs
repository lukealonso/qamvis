using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace QAMVis
{
	public class QAM
	{
		public QAM(int amplitudes, int phases, int frequencies)
		{
			m_NumFrequencies = frequencies;
			m_NumAmplitudes = amplitudes;
			m_NumPhases = phases;
		}

		public float[] Modulate(uint state, int wordLength)
		{
			int subStateBits = (int)Math.Log(m_NumAmplitudes * m_NumPhases, 2);
			float baseAmplitude = 1.0f / (float)m_NumAmplitudes;
			float[] result = new float[wordLength];
			float nrm = 1.0f / (float)m_NumFrequencies;
			for (int f = 0; f < m_NumFrequencies; f++)
			{
				uint subState = (state >> (subStateBits * f)) & (uint)((1 << subStateBits) - 1);
				int amplitudeIndex = (int)subState / m_NumPhases;
				float remainingAmplitude = 1.0f - baseAmplitude;
				float amplitude = baseAmplitude + ((float)amplitudeIndex / (float)(m_NumAmplitudes - 1)) * remainingAmplitude;
				float phase = (float)(subState % m_NumPhases) / (float)(m_NumPhases - 1);
				for (int i = 0; i < wordLength; i++)
				{
					float t = (float)i / (float)(wordLength - 1);
					result[i] += (float)Math.Sin(t * Math.PI * 2.0f * ((float)f + 1.0f) + phase * Math.PI) * amplitude * nrm;
				}
			}
			return result;
		}

		public uint Demodulate(float[] data)
		{
			float nrm = (float)m_NumFrequencies;
			int subStateBits = (int)Math.Log(m_NumAmplitudes * m_NumPhases, 2);
			float magStep = (float)data.Length / (float)(m_NumAmplitudes * 2);
			List<FFT.Complex> coeffs = FFT.DFT(data, nrm).ToList();
			coeffs = coeffs.OrderBy(x => -x.Magnitude).ToList();
			coeffs = coeffs.GetRange(0, m_NumFrequencies);
			coeffs = coeffs.OrderBy(x => x.freq).ToList();
			uint finalState = 0; 
			for( int f = 0; f < m_NumFrequencies; f++ )
			{
				float phaseStep = (float)Math.PI / (float)(m_NumPhases - 1);
				float amplitude = (float)Math.Round((coeffs[f].Magnitude - magStep) / magStep);
				float phase = (float)Math.Round(Math.Abs(coeffs[f].Angle) / phaseStep);
				uint state = (uint)Math.Min((float)NumStates, Math.Max(0.0f, Math.Round(amplitude * (float)m_NumPhases + phase)));
				finalState |= state << (subStateBits * f);
			}

			return finalState;
		}

		public byte[] DecodeStates(uint[] states)
		{
			int bitsPerState = (int)Math.Log(NumStates, 2);
			int stateIndex = 0;
			int bitsRemaining = bitsPerState;
			uint currentState = states[0];
			List<byte> resultStream = new List<byte>();
			while (stateIndex < states.Length)
			{
				uint currentByte = 0;
				int currentBits = 0;
				while (currentBits < 8)
				{
					int needBits = 8 - currentBits;
					int useBits = Math.Min(bitsRemaining, needBits);
					uint useMask = (1U << useBits) - 1U;
					currentByte |= (currentState & useMask) << currentBits;
					currentBits += useBits;
					bitsRemaining -= useBits;
					currentState >>= useBits;
					if (bitsRemaining == 0)
					{
						stateIndex++;
						if (stateIndex < states.Length)
						{
							bitsRemaining = bitsPerState;
							currentState = states[stateIndex];
						}
						else
						{
							break;
						}
					}
				}
				resultStream.Add((byte)currentByte);
			}
			return resultStream.ToArray();
		}

		public uint[] EncodeStates(byte[] input)
		{
			int bitsPerState = (int)Math.Log(NumStates, 2);
			int byteIndex = 0;
			int bitsRemaining = 8;
			uint currentByte = input[0];
			List<uint> resultStream = new List<uint>();
			while (byteIndex < input.Length)
			{
				uint currentState = 0;
				int currentBits = 0;
				while (currentBits < bitsPerState)
				{
					int needBits = bitsPerState - currentBits;
					int useBits = Math.Min(bitsRemaining, needBits);
					uint useMask = (1U << useBits) - 1U;
					currentState |= (currentByte & useMask) << currentBits;
					currentBits += useBits;
					bitsRemaining -= useBits;
					currentByte >>= useBits;
					if (bitsRemaining == 0)
					{
						byteIndex++;
						if (byteIndex < input.Length)
						{
							bitsRemaining = 8;
							currentByte = input[byteIndex];
						}
						else
						{
							break;
						}
					}
				}
				resultStream.Add(currentState);
			}
			return resultStream.ToArray();
		}

		public int NumStates
		{
			get
			{
				return (int)Math.Pow(m_NumAmplitudes * m_NumPhases, m_NumFrequencies);
			}
		}
		private int m_NumFrequencies;
		private int m_NumAmplitudes;
		private int m_NumPhases;
	}
}
