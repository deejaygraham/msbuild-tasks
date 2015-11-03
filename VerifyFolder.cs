using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MsBuild.ThreeByTwo.Tasks
{
	public class VerifyFolder : Task
	{
		[Required]
		public ITaskItem[] Folders { get; set; }

		public override bool Execute()
		{
			Log.LogMessage(MessageImportance.Low, "Starting VerifyFolder");

			foreach(ITaskItem folder in this.Folders)
			{
				string folderPath = folder.GetMetadata("FullPath");
				string metaCount = folder.GetMetadata("FileCount");

				Log.LogMessage("Verifying {0}", folderPath);

				int fileCount = 0;

				if (!String.IsNullOrEmpty(metaCount))
				{
					Int32.TryParse(metaCount, out fileCount);
				}
					
				if (!System.IO.Directory.Exists(folderPath))
				{
					Log.LogError("Folder \'{0}\' does not exist", folderPath);
				}
				else if (fileCount > 0)
				{
					var content = System.IO.Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly);

					Log.LogMessage("Checking folder content");

					if (content.Length != fileCount)
					{
						Log.LogError("Folder \'{0}\' expected {1} files, actually contains {2} files", folderPath, fileCount, content.Length);
					}
				}
			}

			return !Log.HasLoggedErrors;
		}
	}
}
