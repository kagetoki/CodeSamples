using System;
using System.Collections.Generic;
using System.Text;

namespace AgileFusion.Banking.Services.Utils
{
    public static class CsvParser
    {
        public static List<T> Parse<T>(string input, Func<IList<string>, T> objectCreator, char separator = ',')
        {
            return Parse<T>(input.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries), objectCreator, separator);
        }

        public static List<T> Parse<T>(IEnumerable<string> lines, Func<IList<string>, T> objectCreator, char separator = ',')
        {
            var result = new List<T>();
            foreach (string line in lines)
            {
                if (line.Trim().StartsWith("#"))
                {
                    continue;
                }
                var splits = line.Split(separator);
                var parts = new List<string>(splits.Length);
                StringBuilder sb = new StringBuilder();
                bool needToAppend = false;
                foreach (var split in splits)
                {

                    if (!string.IsNullOrEmpty(split) && split[0] == '"')
                    {
                        if(split[split.Length - 1] == '"')
                        {
                            parts.Add(split.Trim('"'));
                            continue;
                        }
                        sb.Append(split);
                        needToAppend = true;
                        continue;
                    }
                    if (!string.IsNullOrEmpty(split) && split[split.Length - 1] == '"')
                    {
                        sb.Append(separator).Append(split);
                        needToAppend = false;
                        parts.Add(sb.ToString().Trim('"'));
                        sb.Clear();
                        continue;
                    }
                    if (needToAppend)
                    {
                        sb.Append(separator).Append(split);
                        continue;
                    }
                    parts.Add(split);
                }

                var entry = objectCreator(parts);
                result.Add(entry);
            }

            return result;
        }
    }
}
