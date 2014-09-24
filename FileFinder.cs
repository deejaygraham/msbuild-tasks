using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ThreeByTwoTasks
{
	public class FileFinder
	{
		public IEnumerable<string> Find(string folder, string filter)
		{
			var htmlList = new List<string>();

			foreach (var html in Directory.EnumerateFiles(folder, filter, SearchOption.AllDirectories))
			{
				htmlList.Add(html);
			}

			return htmlList;
		}
	}
}
