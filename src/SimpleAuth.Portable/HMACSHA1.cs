//	  This code is an adaptation of HMachSHA1 from https://github.com/JoeMayo/LinqToTwitter
//
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.



using System;
using System.Linq;

namespace System.Security.Cryptography
{
	public class HMACSHA1
	{
		Sha1 hash = new Sha1();

		byte[] key;
		public HMACSHA1(byte[] key)
		{
			this.key = InitializeKey(key);
		}

		public byte[] ComputeHash(byte[] input)
		{
			byte[] initializedKey = InitializeKey(key);

			byte[] oKeyPad = new byte[Blocksize];
			byte[] iKeyPad = new byte[Blocksize];

			for (int i = 0; i < Blocksize; i++)
			{
				oKeyPad[i] = (byte)(0x5c ^ initializedKey[i]);
				iKeyPad[i] = (byte)(0x36 ^ initializedKey[i]);
			}

			byte[] innerHash = hash.Compute(Concat(iKeyPad, input));
			byte[] outerHash = hash.Compute(Concat(oKeyPad, innerHash));

			return outerHash;
		}

		const int Blocksize = 64;


		byte[] InitializeKey(byte[] key)
		{
			byte[] initializedKey = null;

			if (key.Length > Blocksize)
			{
				byte[] hashedKey = hash.Compute(key);
				byte[] padding = Enumerable.Repeat<byte>(0x00, Blocksize - hashedKey.Length).ToArray();
				initializedKey = Concat(hashedKey, padding);
			}
			else if (key.Length < Blocksize)
			{
				byte[] padding = Enumerable.Repeat<byte>(0x00, Blocksize - key.Length).ToArray();
				initializedKey = Concat(key, padding);
			}
			else
			{
				initializedKey = key;
			}

			return initializedKey;
		}

		byte[] Concat(byte[] first, byte[] second)
		{
			byte[] combinedBytes = new byte[first.Length + second.Length];
			Buffer.BlockCopy(first, 0, combinedBytes, 0, first.Length);
			Buffer.BlockCopy(second, 0, combinedBytes, first.Length, second.Length);
			return combinedBytes;
		}

		public class Sha1
		{
			const int HashSize = 20;

			const uint K0 = 0x5A827999;
			const uint K1 = 0x6ED9EBA1;
			const uint K2 = 0x8F1BBCDC;
			const uint K3 = 0xCA62C1D6;

			class Context
			{
				public uint A, B, C, D, E;

				public int MessageBlockIndex;
				public readonly byte[] MessageBlock = new byte[64];

				public readonly uint[] IntermediateHash =
				{
				0x67452301,
				0xEFCDAB89,
				0x98BADCFE,
				0x10325476,
				0xC3D2E1F0
			};

				public uint LengthHigh;
				public uint LengthLow;
			}

			uint CircularShift(int bits, uint word)
			{
				return (((word) << (bits)) | ((word) >> (32 - (bits))));
			}

			public byte[] Compute(byte[] message)
			{
				var ctx = new Context();

				foreach (var msgByte in message)
				{
					ctx.MessageBlock[ctx.MessageBlockIndex++] = (byte)(msgByte & 0xFF);

					ctx.LengthLow += 8;
					if (ctx.LengthLow == 0)
						ctx.LengthHigh++;

					if (ctx.MessageBlockIndex == 64)
						ProcessMessageBlock(ctx);
				}

				PadMessage(ctx);

				var msgDigest = new byte[HashSize];

				for (int i = 0; i < HashSize; i++)
					msgDigest[i] = (byte)(ctx.IntermediateHash[i >> 2] >> 8 * (3 - (i & 0x03)));

				return msgDigest;
			}

			void ProcessMessageBlock(Context ctx)
			{
				uint[] w = new uint[80];

				for (int t = 0; t < 16; t++)
				{
					w[t] |= (uint)ctx.MessageBlock[t * 4 + 0] << 24;
					w[t] |= (uint)ctx.MessageBlock[t * 4 + 1] << 16;
					w[t] |= (uint)ctx.MessageBlock[t * 4 + 2] << 08;
					w[t] |= (uint)ctx.MessageBlock[t * 4 + 3];
				}

				for (int t = 16; t < 80; t++)
					w[t] = CircularShift(1, w[t - 3] ^ w[t - 8] ^ w[t - 14] ^ w[t - 16]);

				ctx.A = ctx.IntermediateHash[0];
				ctx.B = ctx.IntermediateHash[1];
				ctx.C = ctx.IntermediateHash[2];
				ctx.D = ctx.IntermediateHash[3];
				ctx.E = ctx.IntermediateHash[4];

				for (int t = 0; t < 20; t++)
					RotateWordBuffers(ctx, (b, c, d) => (b & c) | ((~b) & d), w[t], K0);

				for (int t = 20; t < 40; t++)
					RotateWordBuffers(ctx, (b, c, d) => b ^ c ^ d, w[t], K1);

				for (int t = 40; t < 60; t++)
					RotateWordBuffers(ctx, (b, c, d) => (b & c) | (b & d) | (c & d), w[t], K2);

				for (int t = 60; t < 80; t++)
					RotateWordBuffers(ctx, (b, c, d) => b ^ c ^ d, w[t], K3);

				ctx.IntermediateHash[0] += ctx.A;
				ctx.IntermediateHash[1] += ctx.B;
				ctx.IntermediateHash[2] += ctx.C;
				ctx.IntermediateHash[3] += ctx.D;
				ctx.IntermediateHash[4] += ctx.E;

				ctx.MessageBlockIndex = 0;
			}

			void RotateWordBuffers(Context ctx, Func<uint, uint, uint, uint> f, uint wt, uint kt)
			{
				uint temp = CircularShift(5, ctx.A) + (f(ctx.B, ctx.C, ctx.D)) + ctx.E + wt + kt;

				ctx.E = ctx.D;
				ctx.D = ctx.C;
				ctx.C = CircularShift(30, ctx.B);
				ctx.B = ctx.A;
				ctx.A = temp;
			}

			void PadMessage(Context ctx)
			{
				if (ctx.MessageBlockIndex > 55)
				{
					ctx.MessageBlock[ctx.MessageBlockIndex++] = 0x80;

					while (ctx.MessageBlockIndex < 64)
						ctx.MessageBlock[ctx.MessageBlockIndex++] = 0;

					ProcessMessageBlock(ctx);

					while (ctx.MessageBlockIndex < 56)
						ctx.MessageBlock[ctx.MessageBlockIndex++] = 0;
				}
				else
				{
					ctx.MessageBlock[ctx.MessageBlockIndex++] = 0x80;

					while (ctx.MessageBlockIndex < 56)
						ctx.MessageBlock[ctx.MessageBlockIndex++] = 0;
				}

				ctx.MessageBlock[56] = (byte)(ctx.LengthHigh >> 24);
				ctx.MessageBlock[57] = (byte)(ctx.LengthHigh >> 16);
				ctx.MessageBlock[58] = (byte)(ctx.LengthHigh >> 08);
				ctx.MessageBlock[59] = (byte)ctx.LengthHigh;
				ctx.MessageBlock[60] = (byte)(ctx.LengthLow >> 24);
				ctx.MessageBlock[61] = (byte)(ctx.LengthLow >> 16);
				ctx.MessageBlock[62] = (byte)(ctx.LengthLow >> 08);
				ctx.MessageBlock[63] = (byte)ctx.LengthLow;

				ProcessMessageBlock(ctx);
			}
		}
	}
}

