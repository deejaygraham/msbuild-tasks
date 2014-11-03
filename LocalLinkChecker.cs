using HtmlAgilityPack;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MsBuild.ThreeByTwo.Tasks
{
	public class LocalLinkChecker : Task
	{
		[Required]
		public ITaskItem Folder { get; set; }

		public string IgnoreLinks { get; set; }

        public bool WarningsAsErrors { get; set; }

		public override bool Execute()
		{
			string checkFolder = string.Empty;

			Log.LogMessage(MessageImportance.Low, "Starting Link Checking");

			if (this.Folder != null)
			{
				checkFolder = this.Folder.GetMetadata("FullPath");
				Log.LogMessage(MessageImportance.Low, "Checking files in folder {0}", checkFolder);
			}

			if (string.IsNullOrEmpty(checkFolder) || !Directory.Exists(checkFolder))
			{
				Log.LogError("Folder \'{0}\' does not exist", checkFolder);
				return false;
			}

			FileFinder fileFinder = new FileFinder();

			var htmlList = fileFinder.Find(checkFolder, "*.htm?");

			List<string> errors = new List<string>();

			LinkFinder linkFinder = new LinkFinder();

			// ignore email addresses and such
			if (!String.IsNullOrWhiteSpace(this.IgnoreLinks))
			{
				string[] ignoreList = this.IgnoreLinks.Split(new char[] { ';' });

				foreach (string ignoreItem in ignoreList)
					linkFinder.Ignore(ignoreItem);
			}

			linkFinder.LinkFound += (obj, e) =>
			{
				string link = e.Link;

				Log.LogMessage(MessageImportance.Low, "Found link: {0}", link);

				if (link.StartsWith("#"))
				{
					// internal link on the current page...
					var allInternalIds = e.Document.DocumentNode.SelectNodes("*[@id]");

					if (allInternalIds != null)
					{
						// find the link...
						var idItem = allInternalIds.FindFirst(link.Substring(1));

						if (idItem == null)
						{
                            LogWarningOrError("{0}({1},{2}): Link to \"{3}\" does not exist", e.FilePath, e.Line, e.Column, link);
						}
					}
				}
				else
				{
					// link to somewhere else...

					if (link.Contains("#"))
					{
						// internal link to an internal id in another document
						// not handled yet.
						Log.LogMessage(MessageImportance.Low, "Ignoring link to id within page: {0}", link);
					}
					else
					{
						if (link.StartsWith("http") || link.StartsWith("//"))
						{
							// absolute path...
							// ping it?
							Log.LogMessage(MessageImportance.Low, "Ignoring link to external page: {0}", link);
						}
						else
						{
							// local path
							try
							{
								string thisFilesFolder = Path.GetDirectoryName(e.FilePath) + "\\";
								Uri baseFolder = new Uri(thisFilesFolder);
								Uri u = new Uri(baseFolder, link);

								const string HtmlSpace = "%20";
								const string LocalSpace = " ";

								string fullPath = u.LocalPath.Replace(HtmlSpace, LocalSpace);

								if (!File.Exists(fullPath))
								{
                                    LogWarningOrError("{0}({1},{2}): Link to \"{3}\" does not exist", e.FilePath, e.Line, e.Column, link);
								}
							}
							catch(Exception ex)
							{
								Log.LogWarningFromException(ex);
							}
						}
					}
				}
			};

            ImageFinder imageFinder = new ImageFinder();

            imageFinder.ImageFound += (obj, e) =>
            {
                string link = e.Link;

                Log.LogMessage(MessageImportance.Low, "Found image: {0}", link);

                if (link.StartsWith("http"))
                {
                    // absolute path...
                    // ping it?
                    Log.LogMessage(MessageImportance.Low, "Ignoring link to external image: {0}", link);
                }
                else
                {
                    // local path
                    try
                    {
                        string thisFilesFolder = Path.GetDirectoryName(e.FilePath) + "\\";

                        Uri baseFolder = new Uri(thisFilesFolder);
                        Uri u = new Uri(baseFolder, link);
                        string fullPath = u.LocalPath.Replace("%20", " ");

                        if (!File.Exists(fullPath))
                        {
                            LogWarningOrError("{0}({1},{2}): Link to \"{3}\" does not exist", e.FilePath, e.Line, e.Column, link);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.LogWarningFromException(ex);
                    }
                }
            };

			foreach (string htmlDocument in htmlList)
			{
				try
				{
                    HtmlDocument html = new HtmlDocument
                    {
                        OptionCheckSyntax = true,
                        OptionExtractErrorSourceText = true
                    };

					html.Load(htmlDocument);

					if (html.DocumentNode == null)
						continue;

                    if (html.ParseErrors.Any())
                    {
                        string fileName = Path.GetFileName(htmlDocument);

                        Log.LogMessage(MessageImportance.High, "Parse errors found...");

                        foreach (var error in html.ParseErrors)
                        {
                            LogWarningOrError("{0}({1},{2}): {3}", fileName, error.Line, error.LinePosition, error.Reason);
                        }
                    }
                    else
                        Log.LogMessage(MessageImportance.High, "No parse errors found");

					linkFinder.Find(htmlDocument, html);
                    imageFinder.Find(htmlDocument, html);
				}
				catch (Exception ex)
				{
					Log.LogWarningFromException(ex);
				}
			}

			if (!Log.HasLoggedErrors)
			{
				Log.LogMessage(MessageImportance.Normal, "No link errors found");
			}

			return !Log.HasLoggedErrors;
		}

        private void LogWarningOrError(string message, params object[] arguments)
        {
            if (this.WarningsAsErrors)
                Log.LogError(message, arguments);
            else
                Log.LogWarning(message, arguments);
        }
	}
}
