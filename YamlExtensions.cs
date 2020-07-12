using System;
using YamlDotNet.RepresentationModel;

namespace LingvoGraph
{
    static class YamlExtensions
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