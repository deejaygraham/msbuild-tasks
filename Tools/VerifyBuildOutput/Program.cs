using System;
using System.IO;
using System.Text;

namespace VerifyBuildOutput
{
	class Program
	{
		static int Main(string[] args)
		{
			if (args.Length < 2)
			{
				Console.WriteLine("Usage: <folder> <msbuild output>");
				return -1;
			}

			string outputFolder = args[0];
			string outputFile = args[1];

			if (!Directory.Exists(outputFolder))
			{
				Console.WriteLine("Folder {0} does not exist", outputFolder);
				return -2;
			}
			
			using (TextWriter writer = new StreamWriter(outputFile))
			{
				const string DefaultBuildTarget = "Build";

				writer.WriteLine("<Project DefaultTargets=\"{0}\" ToolsVersion=\"4.0\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">", DefaultBuildTarget);

				const string MsBuildAssemblyName = "MsBuild.ThreeByTwo.Tasks.dll";

				writer.WriteLine();
				writer.WriteLine("\t<UsingTask TaskName=\"VerifyFile\" AssemblyFile=\"{0}\"/>", MsBuildAssemblyName);
				writer.WriteLine("\t<UsingTask TaskName=\"VerifyFolder\" AssemblyFile=\"{0}\"/>", MsBuildAssemblyName);
				writer.WriteLine();

				const string BuildFolderProperty = "BuildFolder";

				const string PropertyGroupBegin = "<PropertyGroup>";
				const string PropertyGroupEnd = "</PropertyGroup>";

				writer.WriteLine("\t" + PropertyGroupBegin);
				writer.WriteLine("\t\t<{1}>{0}\\</{1}>", outputFolder, BuildFolderProperty);
				writer.WriteLine("\t" + PropertyGroupEnd);

				const string FileFilter = "*.*";

				string MsBuildDollarVariable = string.Format("$({0})", BuildFolderProperty);

				const string ItemGroupBegin = "<ItemGroup>";
				const string ItemGroupEnd = "</ItemGroup>";

				const string FolderItemGroupName = "BuildFolders";
				const string FileItemGroupName = "BuiltBinaries";

				writer.WriteLine("\t" + ItemGroupBegin);
				writer.WriteLine(BuildFolderContent(outputFolder, FileFilter, MsBuildDollarVariable, FolderItemGroupName, "FileCount"));
				writer.WriteLine("\t" + ItemGroupEnd);
				writer.WriteLine();

				writer.WriteLine("\t" + ItemGroupBegin);
				writer.WriteLine(BuildFileContent(outputFolder, FileFilter, MsBuildDollarVariable, FileItemGroupName, "FileSize"));
				writer.WriteLine("\t" + ItemGroupEnd);
				writer.WriteLine();

				writer.WriteLine("\t<Target Name=\"{0}\">", DefaultBuildTarget);
				writer.WriteLine();

				writer.WriteLine("\t\t<VerifyFile Files=\"@({0})\" />", FileItemGroupName);
				writer.WriteLine("\t\t<VerifyFolder Folders=\"@({0})\" />", FolderItemGroupName);

				writer.WriteLine();
				writer.WriteLine("\t</Target>");

				writer.WriteLine();
				writer.WriteLine("</Project>");
			}

			return 0;
		}

		private static string BuildFolderContent(string folder, string filter, string folderVariable, string itemGroupName, string metadataName)
		{
			StringBuilder builder = new StringBuilder();

			var allFolders = Directory.GetDirectories(folder, filter, SearchOption.AllDirectories);

			foreach (var foundFolder in allFolders)
			{
				builder.AppendFormat("\n\t\t<{1} Include=\"{0}\">", foundFolder.Replace(folder + "\\", folderVariable), itemGroupName);
				builder.AppendFormat("\n\t\t\t<{1}>{0}</{1}>", Directory.GetFiles(foundFolder, "*.*", SearchOption.TopDirectoryOnly).Length, metadataName);
				builder.AppendFormat("\n\t\t</{0}>", itemGroupName);
			}

			return builder.ToString();
		}

		private static string BuildFileContent(string folder, string filter, string folderVariable, string itemGroupName, string metadataName)
		{
			StringBuilder builder = new StringBuilder();

			var allFiles = Directory.GetFiles(folder, filter, SearchOption.AllDirectories);

			foreach (var foundFile in allFiles)
			{
				builder.AppendFormat("\n\t\t<{1} Include=\"{0}\">", foundFile.Replace(folder + "\\", folderVariable), itemGroupName);
				builder.AppendFormat("\n\t\t\t<{1}>{0}</{1}>", new FileInfo(foundFile).Length, metadataName);
				builder.AppendFormat("\n\t\t</{0}>", itemGroupName);
			}

			return builder.ToString();
		}

	}
}
