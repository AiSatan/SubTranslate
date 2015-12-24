using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SubReader
{
	class Program
	{
		static Dictionary<string, int> _Worlds = new Dictionary<string, int>();
		static Dictionary<string, string> _WorldsDialog = new Dictionary<string, string>();

		static void Main(string[] args)
		{
			if (args.Length > 0)
			{
				EachFile = false;
				Console.WriteLine("Start for: {0}", args[0]);
				var files = Directory.GetFiles(args[0], "*.ass");
				Console.WriteLine("files: {0}", files.Count());
				foreach (var file in files)
				{
					var lines = File.ReadLines(file);
					var reg = new Regex("(?<test>.*),,(?<res>[A-z '{}0-9\\.\"-\\(\\-)]*)");
					foreach (var line in lines)
					{
						var resultMath = reg.Match(line);
						if (resultMath.Success)
						{
							var dialog = resultMath.Groups["res"].Value.Replace("\\N", " ").Replace("{\\blur2}", "");
							var worlds = dialog.Split(' ', '.', ',', '!', '-', '"', '}');
							foreach (var w in worlds)
							{

								var world = w.ToUpper();

								if (w == "")
								{
									//Console.WriteLine();
									continue;
									//_Worlds.Add(world, 1);
								}
								if (!_Worlds.ContainsKey(world))
								{
									_Worlds.Add(world, 1);
									if (!_WorldsDialog.ContainsKey(world))
									{
										_WorldsDialog.Add(world, dialog.Replace(w, "<b>" + world + "</b>"));
									}
								}
								else
								{
									_Worlds[world]++;
								}
							}
						}
					}
					if (EachFile)
					{
						var lis = (from source in _Worlds.OrderByDescending(pair => pair.Value)
								   let translate = GetTranslate(source.Key)
								   let img = GetImage(source.Key)
								   let tr = translate.Aggregate(string.Empty, (current, s) => current + s)
								   select source.Key + " (" + source.Value + "); " + tr + ";<img src='" + img + "'></img>").ToList();
						File.WriteAllLines(file + "_translate.txt", lis);
						_Worlds.Clear();
						Console.WriteLine("file: {0}", file);

					}
				}
				var len = _Worlds.Count;
				int n = 0;
				var resultList = (from source in _Worlds.OrderByDescending(pair => pair.Value)
								  let translate = GetTranslate(source.Key).Aggregate(string.Empty, (current, s) => current + s)
								  let dialog = _WorldsDialog[source.Key]
								  let dialogTranslate = GetTranslate(_WorldsDialog[source.Key].Replace("<b>", "    ").Replace("</b>", "     ")).Aggregate(string.Empty, (current, s) => current + s)
								  let t = new Action(() => Console.WriteLine(++n + " of " + len + ":" + dialogTranslate.Replace("    ", "<b>").Replace("     ", "</b>"))).BeginInvoke(ar => { }, n)
								  select string.Format("{0} ({1});{3};{2};{4}", source.Key, source.Value, translate, dialog, dialogTranslate)).ToList();

				File.WriteAllLines(string.Format("test{0}.txt", Path.GetRandomFileName()), resultList);

				//
			}
		}

		private static string GetImage(string world, int repid = 0)
		{
			try
			{
				Thread.Sleep(repid * 1000);

				var wc = new WebClient { Encoding = Encoding.UTF8 };
				var address = new Uri("https://www.google.ru/search?" +
									  "q=" + world.Trim() +
									  "&tbm=isch");
				var str = wc.DownloadString(address);
				return str;
			}
			catch
			{
				return GetImage(world, ++repid);
			}
		}

		public static string[] GetTranslate(string world, int repid = 0)
		{
			try
			{
				Thread.Sleep(repid * 1000);

				var wc = new WebClient { Encoding = Encoding.UTF8 };
				var address = new Uri("https://translate.yandex.net/api/v1.5/tr.json/translate?" +
									  "key=" +
									  "&text=" +
									  world.Trim()
									  + "&lang=en-ru");
				var str = wc.DownloadString(address);
				return JsonConvert.DeserializeObject<Translate>(str).text;
			}
			catch
			{
				return GetTranslate(world, ++repid);
			}
		}

		public class Translate
		{
			public int code { get; set; }
			public string lang { get; set; }
			public string[] text { get; set; }
		}

		public static bool EachFile { get; set; }
	}
}
