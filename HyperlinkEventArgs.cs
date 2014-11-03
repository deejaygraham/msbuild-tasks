using HtmlAgilityPack;
using System;

namespace MsBuild.ThreeByTwo.Tasks
{
	internal class HyperlinkEventArgs : EventArgs
	{
		public HtmlDocument Document { get; set; }

		public string FilePath { get; set; }

		public string Link { get; set; }

		public int Line { get; set; }

		public int Column { get; set; }
	}

}
