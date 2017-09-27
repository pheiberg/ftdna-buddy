using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            int nameEnd = line.IndexOf("\",\"", StringComparison.OrdinalIgnoreCase);
            string remainder = line.Substring(nameEnd + 3);
            string name = line.Substring(1, nameEnd - 1);
            int matchNameEnd = remainder.LastIndexOf("\",", StringComparison.OrdinalIgnoreCase);
            string matchName = remainder.Substring(0, matchNameEnd);
            string[] parts = remainder.Substring(matchNameEnd + 2).Split(',');

            if (parts.Length != 5)
                return null;
            
            return new ChromosomeSegment
            {
                Name = name,
                MatchName = matchName,
                Chromosome = parts[0],
                StartLocation = long.Parse(parts[1]),
                EndLocation = long.Parse(parts[2]),
                Centimorgans = double.Parse(parts[3]),
                MatchingSnps = int.Parse(parts[4])
            };
        }
    }
}