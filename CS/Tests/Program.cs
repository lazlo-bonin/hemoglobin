using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using Hemoglobin;

namespace Tests
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			RedditSearch();
		}

		private static void RedditSearch()
		{
			var parser = new StandardHtmlParser(AnonymousStructure.FixedObject);

			parser.Binder.Processors.Add("commentCount", (value, arguments, alias) => ((string) value).Split(' ')[0]);

			var binding = HtmlBinding.Load(BindingPath("RedditSearch.hb"));

			Console.WriteLine(binding);

			Console.Write("Query: ");
			var query = Console.ReadLine();
			Console.WriteLine();

			var list = Html("https://www.reddit.com/search?q=" + query);

			var results = parser.Parse<dynamic[]>(binding, list);

			Process.Start(results[0].Link);

			foreach (var result in results)
			{
				Console.WriteLine(result.ToDebugString());

				Console.WriteLine();
			}

			Console.WriteLine("Done.");

			Console.ReadLine();
		}

		private static void Example()
		{
			var parser = new StandardHtmlParser(AnonymousStructure.ExpandoObject);

			var binding = HtmlBinding.Load(@"C:\binding.hb");

			var html = new WebClient().DownloadString("http://example.com");

			var results = parser.Parse<dynamic[]>(binding, html);

			Console.WriteLine(results[0].Title);
		}


		private static string BindingPath(string fileName)
		{
			return Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "Bindings" + Path.DirectorySeparatorChar +
			       fileName;
		}

		private static string Html(string url)
		{
			using (var client = new WebClient())
			{
				return client.DownloadString(url);
			}
		}
	}
}