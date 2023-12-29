using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ML.Samples.Utils
{
    public static class SampleTable
    {

        public static async Task<string> GenerateFileAsync(string path, int rows)
        {
            var file = Path.Combine(path, $"{Guid.NewGuid():N}.csv");

            var rnd = new Random();

            var lines = new List<string>();

            for (int i = 0; i < rows; i++)
            {

                var a = rnd.Next(1, 200);
                var b = rnd.Next(1, 200);
                var c = rnd.Next(1, 200);

                double res = (a * b + b * c) / c;

                lines.Add($"{a},{b},{c},{res}");
            }

            await File.WriteAllLinesAsync(file, lines.ToArray());

            return file;
        }

    }
}
