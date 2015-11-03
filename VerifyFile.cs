using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MsBuild.ThreeByTwo.Tasks
{
	public class VerifyFile : Task
	{
		[Required]
		public ITaskItem[] Files { get; set; }

		public override bool Execute()
		{
			Log.LogMessage(MessageImportance.Low, "Starting VerifyFile");

			foreach (ITaskItem file in this.Files)
			{
				string filePath = file.GetMetadata("FullPath");
				string metaCount = file.GetMetadata("FileSize");

				Log.LogMessage("Verifying {0}", filePath);

				long fileSize = 0;

				if (!String.IsNullOrEmpty(metaCount))
				{
					Int64.TryParse(metaCount, out fileSize);
				}

				if (!System.IO.File.Exists(filePath))
				{
					Log.LogError("File \'{0}\' does not exist", filePath);
				}
				else if (fileSize > 0)
				{
					Log.LogMessage("Verifying file size");

					var info = new System.IO.FileInfo(filePath);

					if (fileSize != info.Length)
					{
						Log.LogError("File \'{0}\' expected to be {1}, actually {2}", filePath, fileSize, info.Length);
					}
				}
			}

			return !Log.HasLoggedErrors;
		}
	}
}
