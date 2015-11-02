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
				string folderPath = string.Empty;

				if (folder != null)
				{
					folderPath = folder.GetMetadata("FullPath");
				}

				if (string.IsNullOrEmpty(folderPath) || !System.IO.Directory.Exists(folderPath))
				{
					Log.LogError("Folder \'{0}\' does not exist", folderPath);
				}
			}

			return !Log.HasLoggedErrors;
		}
	}
}
