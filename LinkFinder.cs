using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeByTwoTasks
{
	internal class LinkFinder
	{
		private List<string> ignoredPrefixes = new List<string>();

		public event EventHandler<HyperlinkEventArgs> LinkFound;

		const string xpathSelector = "//a[@href]";
		const string linkAttribute = "href";
		
		public void Ignore(string prefix)
		{
			this.ignoredPrefixes.Add(prefix);
		}

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
			var allLinks = html.DocumentNode.SelectNodes(xpathSelector);

			if (allLinks == null)
				return;

			foreach (HtmlNode link in allLinks)
			{
				HtmlAttribute href = link.Attributes[linkAttribute];

				if (href != null && !String.IsNullOrEmpty(href.Value) && this.LinkFound != null)
				{
					if (!this.ignoredPrefixes.Any(x => href.Value.StartsWith(x)))
					{
						var handler = this.LinkFound;

						handler(this, new HyperlinkEventArgs
						{
							Document = html,
							FilePath = documentPath,
							Link = href.Value,
							Line = href.Line,
							Column = href.LinePosition
						});
					}
				}
			}
		}
	}

}
