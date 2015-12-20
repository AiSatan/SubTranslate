using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SubReader
{
	class Program
	{
		static Dictionary<string, int> _Worlds = new Dictionary<string, int>();

		static void Main(string[] args)
		{
			if (args.Length > 0)
			{
				Console.WriteLine(String.Format("Start for: {0}", args[0]));
				var files = Directory.GetFiles(args[0], "*.ass");
				Console.WriteLine(String.Format("files: {0}", files.Count()));
				foreach (var file in files)
				{
					var lines = File.ReadLines(file);
					var reg = new Regex("(?<test>.*),,(?<res>[A-z '{}0-9\\.\"-\\(\\-)]*)");
					foreach (var line in lines)
					{
						var resultMath = reg.Match(line);
						if (resultMath.Success)
						{
							var worlds = resultMath.Groups["res"].Value.Replace("\\N", " ").Split(' ', '.', ',', '!', '-', '"', '}');
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
								}
								else
								{
									_Worlds[world]++;
								}
							}
						}
					}
				}

				var li = (from source in _Worlds.OrderByDescending(pair => pair.Value)
						  let translate = GetTranslate(source.Key)
						  let tr = translate.Aggregate(string.Empty, (current, s) => current + s)
						  select source.Key + " - " + source.Value + " ( " + tr + " );").ToList();
				File.WriteAllLines("test.txt", li);

				//
			}
		}

		public static string[] GetTranslate(string world)
		{
			var wc = new WebClient();
			wc.Encoding = System.Text.Encoding.UTF8;
			var address = new Uri("https://translate.yandex.net/api/v1.5/tr.json/translate?" +
								  "key=trnsl.1.1.20151220T182743Z.c02b4d4fae48d3e4.508eb90227d665a668681a2a77540fac08ce17e9" +
								  "&text=" +
								  world.Trim()
								  + "&lang=en-ru");
			var str = wc.DownloadString(address);

			return JsonConvert.DeserializeObject<Translate>(str).text;
		}

		public class Translate
		{
			public int code { get; set; }
			public string lang { get; set; }
			public string[] text { get; set; }
		}
	}
}
