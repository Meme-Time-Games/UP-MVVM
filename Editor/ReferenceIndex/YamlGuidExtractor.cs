using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MVVM.CoreEditor
{
    public class YamlGuidExtractor
    {
        private static readonly Regex GuidPattern =
            new Regex(@"guid:\s*([0-9a-f]{32})", RegexOptions.Compiled);

        public IReadOnlyCollection<string> GetGuidsFromYaml(string yaml)
        {
            if (string.IsNullOrEmpty(yaml))
                return Array.Empty<string>();

            HashSet<string> guids = new HashSet<string>();

            foreach (Match guidMatch in GuidPattern.Matches(yaml))
                guids.Add(guidMatch.Groups[1].Value);

            return guids;
        }
    }
}
