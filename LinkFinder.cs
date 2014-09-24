using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeByTwoTasks
{
	public class LinkFinder
	{
		private List<string> ignoredPrefixes = new List<string>();

		public event EventHandler<HyperlinkEventArgs> LinkFound;

		public void Ignore(string prefix)
		{
			this.ignoredPrefixes.Add(prefix);
		}

		public void Find(IEnumerable<string> documents)
		{
			const string xpathSelector = "//a[@href]";
			const string linkAttribute = "href";

			foreach (string document in documents)
			{
				try
				{
					HtmlDocument html = new HtmlDocument();
					html.Load(document);

					if (html.DocumentNode == null)
						continue;

					var allLinks = html.DocumentNode.SelectNodes(xpathSelector);

					if (allLinks == null)
						continue;

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
									FilePath = document,
									Link = href.Value,
									Line = href.Line,
									Column = href.LinePosition
								});
							}
						}
					}
				}
				catch (Exception)
				{
					// continue
				}
			}
		}
	}

}
