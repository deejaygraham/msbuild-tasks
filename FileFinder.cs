using System.Collections.Generic;
using System.IO;

namespace MsBuild.ThreeByTwo.Tasks
{
	internal class FileFinder
	{
		public IEnumerable<string> Find(string folder, string filter, SearchOption searchDepth)
		{
			var htmlList = new List<string>();

			foreach (var html in Directory.EnumerateFiles(folder, filter, searchDepth))
			{
				htmlList.Add(html);
			}

			return htmlList;
		}
	}
}
