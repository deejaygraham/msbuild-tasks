using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeByTwoTasks
{
	public class HyperlinkEventArgs : EventArgs
	{
		public HtmlDocument Document { get; set; }

		public string FilePath { get; set; }

		public string Link { get; set; }

		public int Line { get; set; }

		public int Column { get; set; }
	}

}
