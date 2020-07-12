using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using YamlDotNet.RepresentationModel;

namespace LingvoGraph
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            // TODO process all words from stdin
            await Parse(DataSources.Macmillan, new Input("girl", "en"));
        }

        private static async Task Parse(DataSource src, Input input)
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
}
