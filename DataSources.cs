using System;
using System.IO;
using YamlDotNet.RepresentationModel;

namespace LingvoGraph
{
    static class DataSources
    {
        public static readonly DataSource Macmillan = new DataSource
        {
            Name = "macmillan",
            Url = t => $"https://www.macmillandictionary.com/dictionary/british/{t.Text}",
            Schema = LoadYaml("macmillan.yml"),
        };
        
        private static YamlDocument LoadYaml(string name)
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
    }
}
