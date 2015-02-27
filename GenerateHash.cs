using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MsBuild.ThreeByTwo.Tasks
{
	public class GenerateHash : Task
	{
		[Required]
		public ITaskItem Source { get; set; }

		// defaults to MD5
		// Available Values:
		// SHA
		// SHA1
		// MD5
		// SHA256
		// SHA-256
		// SHA384
		// SHA-384
		// SHA512
		// SHA-512
		public string Algorithm { get; set; }

		[Output]
		public string HashValue { get; set; }
		
		public override bool Execute()
		{
			Log.LogMessage(MessageImportance.Low, "Starting GenerateHash");

			string sourcePath = string.Empty;

			if (this.Source != null)
			{
				sourcePath = this.Source.GetMetadata("FullPath");
			}

			if (string.IsNullOrEmpty(sourcePath) || !File.Exists(sourcePath))
			{
				Log.LogError("File \'{0}\' does not exist", sourcePath);
				return false;
			}

			HashAlgorithm algorithm = null;

			try
			{
				if (!String.IsNullOrEmpty(this.Algorithm))
				{
					algorithm = HashAlgorithm.Create(this.Algorithm);
				}
			}
			catch(Exception ex)
			{
				Log.LogErrorFromException(ex);
				return false;
			}

			try
			{
				if (algorithm == null)
				{
					// default
					this.HashValue = CalculateChecksum(sourcePath);
				}
				else
				{
					this.HashValue = CalculateChecksum(sourcePath, algorithm);
				}
			}
			catch (Exception ex)
			{
				Log.LogWarningFromException(ex);
			}

			return !Log.HasLoggedErrors;
		}

		private static string CalculateChecksum(string path)
		{
			using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
			{
				return CalculateChecksum(path, md5);
			}
		}

		private static string CalculateChecksum(string path, HashAlgorithm hashAlgorithm)
		{
			const string HexFormat = "X2";
			var builder = new StringBuilder();

			using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				byte[] hash = hashAlgorithm.ComputeHash(fs);

				for (int i = 0; i < hash.Length; ++i)
				{
					builder.Append(hash[i].ToString(HexFormat, CultureInfo.InvariantCulture));
				}
			}

			return builder.ToString();
		}
	}
}
