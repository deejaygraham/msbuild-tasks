using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MsBuild.ThreeByTwo.Tasks
{
	/// <summary>
	/// Finds entities identified by #id
	/// </summary>
	internal class UniqueItemsFinder
	{
		public event EventHandler<HyperlinkEventArgs> IdFound;

		const string xpathSelector = "//*[@id]";
		const string idAttribute = "id";

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
			var tagsWithId = html.DocumentNode.SelectNodes(xpathSelector);

			if (tagsWithId == null)
				return;

			string htmlFile = System.IO.Path.GetFileName(documentPath);

			foreach (HtmlNode tag in tagsWithId)
			{
				HtmlAttribute id = tag.Attributes[idAttribute];

				if (id != null && !String.IsNullOrEmpty(id.Value) && this.IdFound != null)
				{
					var handler = this.IdFound;

					handler(this, new HyperlinkEventArgs
					{
						Document = html,
						FilePath = documentPath,
						Link = htmlFile + "#" + id.Value,
						Line = id.Line,
						Column = id.LinePosition
					});
				}
			}
		}
	}
}
