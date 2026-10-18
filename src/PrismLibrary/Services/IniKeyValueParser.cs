using System;
using System.Collections.Generic;
using System.IO;

namespace PrismLibrary.Services
{
    public static class IniKeyValueParser
    {
        public static Dictionary<string, string> ParseFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            return Parse(File.ReadAllText(path));
        }

        public static Dictionary<string, string> Parse(string content)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(content))
            {
                return result;
            }

            using (var reader = new StringReader(content))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length == 0 || line[0] == '#' || line[0] == ';' || line[0] == '[')
                    {
                        continue;
                    }

                    var separator = line.IndexOf('=');
                    if (separator <= 0)
                    {
                        continue;
                    }

                    var key = line.Substring(0, separator).Trim();
                    var value = line.Substring(separator + 1).Trim();
                    if (key.Length == 0)
                    {
                        continue;
                    }

                    result[key] = value;
                }
            }

            return result;
        }
    }
}
