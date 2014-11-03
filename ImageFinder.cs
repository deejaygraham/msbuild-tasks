using HtmlAgilityPack;
using System;
using System.Collections.Generic;

namespace MsBuild.ThreeByTwo.Tasks
{
	internal class ImageFinder
	{
		public event EventHandler<HyperlinkEventArgs> ImageFound;

		const string xpathSelector = "//img[@src]";
		const string sourceAttribute = "src";

		public void Find(IEnumerable<string> documents)
		{
			foreach (string document in documents)
			{
				try
				{
					HtmlDocument html = new HtmlDocument();
					html.Load(document);

					if (html.DocumentNode == null)
						continue;

					Find(document, html);
				}
				catch (Exception)
				{
					// continue
				}
			}
		}

		public void Find(string documentPath, HtmlDocument html)
		{
			var allImages = html.DocumentNode.SelectNodes(xpathSelector);

			if (allImages == null)
				return;

			foreach (HtmlNode link in allImages)
			{
				HtmlAttribute src = link.Attributes[sourceAttribute];

				if (src != null && !String.IsNullOrEmpty(src.Value) && this.ImageFound != null)
				{
					var handler = this.ImageFound;

					handler(this, new HyperlinkEventArgs
					{
						Document = html,
						FilePath = documentPath,
						Link = src.Value,
						Line = src.Line,
						Column = src.LinePosition
					});
				}
			}
		}
	}
}
