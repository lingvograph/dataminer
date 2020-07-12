using System;
using YamlDotNet.RepresentationModel;

namespace LingvoGraph
{
    class DataSource
    {
        public string Name;
        public Func<Input, string> Url;
        public YamlDocument Schema;
    }
}