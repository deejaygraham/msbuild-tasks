using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace MsBuild.ThreeByTwo.Tasks
{
	public class XmlMerge : Task
	{
		/// <summary>
		/// Xml file to use as the source
		/// </summary>
		[Required]
		public ITaskItem Source { get; set; }

		/// <summary>
		/// XPath query - where to append the merged data 
		/// </summary>
		[Required]
		public string SourceQuery { get; set; }

		/// <summary>
		/// Xml file containing the data to merge into the source
		/// </summary>
		[Required]
		public ITaskItem MergeSource { get; set; }

		/// <summary>
		/// XPath query - data to merge
		/// </summary>
		[Required]
		public string MergeQuery { get; set; }

		public string NamespacePrefix { get; set; }

		public string NamespaceUri { get; set; }

		/// <summary>
		/// Xml output file
		/// </summary>
		[Required]
		public ITaskItem Target { get; set; }
		
		public override bool Execute()
		{
			Log.LogMessage(MessageImportance.Low, "Starting XML Merge");

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

			string mergeSourcePath = string.Empty;

			if (this.MergeSource != null)
			{
				mergeSourcePath = this.MergeSource.GetMetadata("FullPath");
			}

			if (string.IsNullOrEmpty(mergeSourcePath) || !File.Exists(mergeSourcePath))
			{
				Log.LogError("File \'{0}\' does not exist", mergeSourcePath);
				return false;
			}

			if (string.IsNullOrEmpty(this.SourceQuery))
			{
				Log.LogError("Source XPath query is empty");
				return false;
			}

			if (string.IsNullOrEmpty(this.MergeQuery))
			{
				Log.LogError("Merge XPath query is empty");
				return false;
			}

			try
			{
				var sourceXml = new XmlDocument();
				sourceXml.Load(sourcePath);

				XPathNavigator sourceNavigator = sourceXml.CreateNavigator();
				XmlNamespaceManager sourceNamespaces = new XmlNamespaceManager(sourceNavigator.NameTable);

				if (!String.IsNullOrEmpty(this.NamespacePrefix) && !String.IsNullOrEmpty(this.NamespaceUri))
				{
					sourceNamespaces.AddNamespace(this.NamespacePrefix, this.NamespaceUri);
				}

				XPathNavigator insertAfterObject = sourceNavigator.SelectSingleNode(this.SourceQuery, sourceNamespaces);

				var mergeXml = new XmlDocument();
				mergeXml.Load(mergeSourcePath);

				XPathNavigator mergeNavigator = mergeXml.CreateNavigator();
				XmlNamespaceManager mergeNamespaces = new XmlNamespaceManager(mergeNavigator.NameTable);

				if (!String.IsNullOrEmpty(this.NamespacePrefix) && !String.IsNullOrEmpty(this.NamespaceUri))
				{
					mergeNamespaces.AddNamespace(this.NamespacePrefix, this.NamespaceUri);
				}

				foreach (XPathNavigator item in mergeNavigator.Select(this.MergeQuery, mergeNamespaces))
				{
					insertAfterObject.InsertAfter(item);
				}

				sourceXml.Save(this.Target.GetMetadata("FullPath"));
			}
			catch (Exception ex)
			{
				Log.LogWarningFromException(ex);
			}

			return !Log.HasLoggedErrors;
		}
	}
}
