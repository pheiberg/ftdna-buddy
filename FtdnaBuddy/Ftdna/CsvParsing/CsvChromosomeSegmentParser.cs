using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using FtdnaBuddy.Ftdna.Model;

namespace FtdnaBuddy.Ftdna.CsvParsing
{
    public class CsvChromosomeSegmentParser
    {
        public IEnumerable<ChromosomeSegment> Parse(TextReader reader)
        {
            var segments = new List<ChromosomeSegment>();
            
            string line = reader.ReadLine();
            if (line == null)
                return segments;
            
            for(line = reader.ReadLine(); line != null; line = reader.ReadLine())
            {
                var processedLine = ParseLine(line);
                if (processedLine != null)
                {
                    segments.Add(processedLine);
                }
            }
            return segments;
        }

        private ChromosomeSegment ParseLine(string line)
        {
            string[] parts = line.Split(',');
            
            if (parts.Length < 7)
                return null;

            return new ChromosomeSegment
            {
                Name = parts[0].Trim('\"'),
                MatchName = string.Join(',', parts.Skip(1).Take(parts.Length - 6)).Trim('\"'),
                Chromosome = parts[parts.Length - 5],
                StartLocation = long.Parse(parts[parts.Length - 4]),
                EndLocation = long.Parse(parts[parts.Length - 3]),
                Centimorgans = double.Parse(parts[parts.Length - 2]),
                MatchingSnps = int.Parse(parts[parts.Length - 1])
            };
        }
    }
}