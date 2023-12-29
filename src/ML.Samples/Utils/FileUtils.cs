using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ML.Samples.Utils
{
    public static class FileUtils
    {

        public static Task AddLineAsync(string file, string line)
            => File.AppendAllLinesAsync(file, [line]);

        public static Task AddCsvLineAsync(string file, IEnumerable<object> items)
        {
            var strs = items.Select(i => $"\"{i}\"").ToArray();
            return AddLineAsync(file, string.Join(",", strs));
        }


    }
}
