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
				string filePath = string.Empty;

				if (file != null)
				{
					filePath = file.GetMetadata("FullPath");
				}

				if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
				{
					Log.LogError("File \'{0}\' does not exist", filePath);
				}
			}

			return !Log.HasLoggedErrors;
		}
	}
}
