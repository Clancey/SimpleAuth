using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace SimpleAuth
{
	public class AuthStorage : IAuthStorage
	{
		private const int Keysize = 256;
		private const int DerivationIterations = 1000;

		public static string EncryptString(string plainText, string passPhrase)
		{
			var saltStringBytes = Generate256BitsOfRandomEntropy();
			var ivStringBytes = Generate256BitsOfRandomEntropy();
			var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
			using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
			{
				var keyBytes = password.GetBytes(Keysize / 8);
				using (var symmetricKey = new RijndaelManaged())
				{
					symmetricKey.BlockSize = 256;
					symmetricKey.Mode = CipherMode.CBC;
					symmetricKey.Padding = PaddingMode.PKCS7;
					using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
					{
						using (var memoryStream = new MemoryStream())
						{
							using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
							{
								cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
								cryptoStream.FlushFinalBlock();
								// Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
								var cipherTextBytes = saltStringBytes;
								cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
								cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
								memoryStream.Close();
								cryptoStream.Close();
								return Convert.ToBase64String(cipherTextBytes);
							}
						}
					}
				}
			}
		}

		public static string DecryptString(string cipherText, string passPhrase)
		{
			var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
			var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
			var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
			var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

			using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
			{
				var keyBytes = password.GetBytes(Keysize / 8);
				using (var symmetricKey = new RijndaelManaged())
				{
					symmetricKey.BlockSize = 256;
					symmetricKey.Mode = CipherMode.CBC;
					symmetricKey.Padding = PaddingMode.PKCS7;
					using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
					{
						using (var memoryStream = new MemoryStream(cipherTextBytes))
						{
							using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
							{
								var plainTextBytes = new byte[cipherTextBytes.Length];
								var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
								memoryStream.Close();
								cryptoStream.Close();
								return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
							}
						}
					}
				}
			}
		}

		private static byte[] Generate256BitsOfRandomEntropy()
		{
			var randomBytes = new byte[32];
			using (var rngCsp = new RNGCryptoServiceProvider())
			{
				rngCsp.GetBytes(randomBytes);
			}
			return randomBytes;
		}

		static string CalculateMD5Hash(string input)
		{
			var md5 = MD5.Create();

			var inputBytes = Encoding.ASCII.GetBytes(input);
			var hash = md5.ComputeHash(inputBytes);
			var sb = new StringBuilder();

			for (int i = 0; i < hash.Length; i++)
			{
				sb.Append(hash[i].ToString("X2"));
			}

			return sb.ToString();

		}

		public void SetSecured(string identifier, string value, string clientId, string clientSecret, string sharedGroup)
		{
			var key = $"{clientId}-{identifier}-{clientId}-{sharedGroup}";
			var newKey = CalculateMD5Hash(key);
			var encrypted = EncryptString(value, clientSecret);
			Plugin.Settings.CrossSettings.Current.AddOrUpdateValue(newKey, encrypted);
		}

		public string GetSecured(string identifier, string clientId, string clientSecret, string sharedGroup)
		{
			try
			{
				var key = $"{clientId}-{identifier}-{clientId}-{sharedGroup}";
				var newKey = CalculateMD5Hash(key);
				var cryptText = Plugin.Settings.CrossSettings.Current.GetValueOrDefault(newKey, "");
				return DecryptString(cryptText, clientSecret);
			}
			catch (Exception ex)
			{
				//Console.WriteLine(ex);
			}
			return null;
		}
	}
}
