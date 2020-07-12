using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using YamlDotNet.RepresentationModel;

namespace LingvoGraph
{
    struct Input
    {
        public string Text;
        public string Lang;

        public Input(string text, string lang)
        {
            Text = text;
            Lang = lang;
        }
    }

    class DataSource
    {
        public string Name;
        public Func<Input, string> Url;
        public YamlDocument Schema;
    }

    class DataSources
    {
        static YamlDocument LoadYaml(string name)
        {
            string cwd = Environment.GetEnvironmentVariable("YAML_DIR");
            if (string.IsNullOrEmpty(cwd))
            {
                cwd = Path.Combine(Environment.CurrentDirectory, "../../");
            }

            var path = Path.Combine(cwd, name);
            using var input = new StreamReader(path);
            var yaml = new YamlStream();
            yaml.Load(input);
            return yaml.Documents[0];
        }
        
        public static readonly DataSource Macmillan = new DataSource
        {
            Name = "macmillan",
            Url = t => $"https://www.macmillandictionary.com/dictionary/british/{t.Text}",
            Schema = LoadYaml("macmillan.yml"),
        };
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            // TODO process all words from stdin
            await Parse(DataSources.Macmillan, new Input("girl", "en"));
        }
        
        static async Task Parse(DataSource src, Input input)
        {
            var url = src.Url(input);
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var doc = await context.OpenAsync(url);
            var root = src.Schema.RootNode as YamlSequenceNode;
            if (root == null)
            {
                throw new InvalidOperationException("Expect list of nodes");
            }

            IEnumerable<object> Exec(YamlMappingNode query)
            {
                var selector = query["selector"].ToString();
                var term = query.Get("term");
                var audio = query.Get("audio");
                var exclude = query.Get("exclude")?.ToString();
                var uniq = query.Get("uniq")?.ToString() == "true";
                var set = new HashSet<string>();
                foreach (var elem in doc.QuerySelectorAll(selector))
                {
                    if (term != null)
                    {
                        var name = term.ToString();
                        var text = elem.TextContent.Strip();
                        if (exclude != null && text.Equals(exclude)) continue;
                        if (uniq && set.Contains(text)) continue;
                        yield return $"({name},lang={input.Lang},text=\"{text}\")";
                        if (uniq) set.Add(text);
                        continue;
                    }

                    if (audio is YamlSequenceNode seq)
                    {
                        foreach (var cmd in seq.Children.Select(t => t.ToString()))
                        {
                            var val = (cmd.StartsWith("@")
                                ? elem.GetAttribute(cmd.Substring(1))
                                : elem.TextContent);
                            val = val.Strip();
                            if (string.IsNullOrEmpty(val))
                            {
                                continue;
                            }

                            yield return $"(audio,url=\"{val}\")";
                        }
                    }
                }
            }

            foreach (var child in root.Children.OrEmpty())
            {
                var query = child as YamlMappingNode;
                if (query == null)
                {
                    Console.Error.WriteLine("skip node {0}", child);
                    continue;
                }

                foreach (var result in Exec(query))
                {
                    Console.WriteLine(result);
                }
            }
        }

        
    }

    static class EnumerableExtensions
    {
        public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T> seq)
        {
            return seq ?? Enumerable.Empty<T>();
        }
    }

    static class YamlExt
    {
        public static YamlNode Get(this YamlNode node, string key)
        {
            try
            {
                return node[key];
            }
            catch (Exception ignore)
            {
                return null;
            }
        }

        public static string Strip(this string s)
        {
            return (s ?? "").Trim().Trim((char) 8203);
        }
    }
}
