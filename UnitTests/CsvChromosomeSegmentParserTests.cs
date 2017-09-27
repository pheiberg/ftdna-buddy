using System.IO;
using System.Linq;
using FluentAssertions;
using FtdnaBuddy.Ftdna.CsvParsing;
using FtdnaBuddy.Ftdna.Model;
using Xunit;

namespace UnitTests
{
    public class CsvChromosomeSegmentParserTests
    {
        [Fact]
        public void SimpleLine_ShouldParseOk()
        {
            const string input = "NAME,MATCHNAME,CHROMOSOME,START LOCATION,END LOCATION,CENTIMORGANS,MATCHING SNPS\r\n" 
                                 + "\"Peter Heiberg\",\"(MG) c/o Leanne Garlock\",1,2,3,4.5,6";
            var reader = new StringReader(input);
            var parser = new CsvChromosomeSegmentParser();

            var result = parser.Parse(reader);

            var expected = new ChromosomeSegment
            {
                Name = "Peter Heiberg",
                MatchName = "(MG) c/o Leanne Garlock",
                Chromosome = "1",
                StartLocation = 2,
                EndLocation = 3,
                Centimorgans = 4.5,
                MatchingSnps = 6
            };
            result.First().Should().BeEquivalentTo(expected);
        }
        
        [Fact]
        public void MatchNameContainingUnescapedQuotes_ShouldParseOk()
        {
            const string input = "NAME,MATCHNAME,CHROMOSOME,START LOCATION,END LOCATION,CENTIMORGANS,MATCHING SNPS\r\n" 
                                 + "\"John Doe\",\"Robert \"Bob\" Foo\",1,2,3,4.5,6";
            var reader = new StringReader(input);
            var parser = new CsvChromosomeSegmentParser();
            
            var result = parser.Parse(reader);

            var expected = new ChromosomeSegment
            {
                Name = "John Doe",
                MatchName = "Robert \"Bob\" Foo",
                Chromosome = "1",
                StartLocation = 2,
                EndLocation = 3,
                Centimorgans = 4.5,
                MatchingSnps = 6
            };
            result.First().Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void RealFile_ShouldParseOk()
        {
            using(var file = File.OpenRead("../../../624718_Chromosome_Browser_Results_20170917.csv"))
            using (var reader = new StreamReader(file))
            {
                var parser = new CsvChromosomeSegmentParser();

                var result = parser.Parse(reader);

                result.Should().HaveCountGreaterOrEqualTo(500);
            }
        }

        [Fact]
        public void MatchNameContainingCommas_ShouldParseOk()
        {
            const string input = "NAME,MATCHNAME,CHROMOSOME,START LOCATION,END LOCATION,CENTIMORGANS,MATCHING SNPS\r\n" 
                               + "\"Peter Heiberg\",\"Erik Engebretsen,c/o Aage Harry Sørensen\",1,31908360,34303465,2.45,600";
            var reader = new StringReader(input);
            var parser = new CsvChromosomeSegmentParser();

            var result = parser.Parse(reader);

            var expected = new ChromosomeSegment
            {
                Name = "Peter Heiberg",
                MatchName = "Erik Engebretsen,c/o Aage Harry Sørensen",
                Chromosome = "1",
                StartLocation = 31908360,
                EndLocation = 34303465,
                Centimorgans = 2.45,
                MatchingSnps = 600
            };
            result.First().Should().BeEquivalentTo(expected);
        }
        
        [Fact]
        public void MatchNameContainingCommasAndQuotes_ShouldParseOk()
        {
            const string input = "NAME,MATCHNAME,CHROMOSOME,START LOCATION,END LOCATION,CENTIMORGANS,MATCHING SNPS\r\n" 
                                 + "\"Peter Heiberg\",\"Robert \"Bob\", Doe\",1,31908360,34303465,2.45,600";
            var reader = new StringReader(input);
            var parser = new CsvChromosomeSegmentParser();

            var result = parser.Parse(reader);

            var expected = new ChromosomeSegment
            {
                Name = "Peter Heiberg",
                MatchName = "Robert \"Bob\", Doe",
                Chromosome = "1",
                StartLocation = 31908360,
                EndLocation = 34303465,
                Centimorgans = 2.45,
                MatchingSnps = 600
            };
            result.First().Should().BeEquivalentTo(expected);
        }
        
        [Fact]
        public void NameContainingCommas_ShouldParseOk()
        {
            const string input = "NAME,MATCHNAME,CHROMOSOME,START LOCATION,END LOCATION,CENTIMORGANS,MATCHING SNPS\r\n" 
                                 + "\"Peter, Heiberg\",\"Robert \"Bob\", Doe\",1,31908360,34303465,2.45,600";
            var reader = new StringReader(input);
            var parser = new CsvChromosomeSegmentParser();

            var result = parser.Parse(reader);

            var expected = new ChromosomeSegment
            {
                Name = "Peter, Heiberg",
                MatchName = "Robert \"Bob\", Doe",
                Chromosome = "1",
                StartLocation = 31908360,
                EndLocation = 34303465,
                Centimorgans = 2.45,
                MatchingSnps = 600
            };
            result.First().Should().BeEquivalentTo(expected);
        }
        
        [Fact]
        public void NameContainingCommasAndQuotes_ShouldParseOk()
        {
            const string input = "NAME,MATCHNAME,CHROMOSOME,START LOCATION,END LOCATION,CENTIMORGANS,MATCHING SNPS\r\n" 
                                 + "\"Peter \"Peppe\", Heiberg\",\"Robert \"Bob\", Doe\",1,31908360,34303465,2.45,600";
            var reader = new StringReader(input);
            var parser = new CsvChromosomeSegmentParser();

            var result = parser.Parse(reader);

            var expected = new ChromosomeSegment
            {
                Name = "Peter \"Peppe\", Heiberg",
                MatchName = "Robert \"Bob\", Doe",
                Chromosome = "1",
                StartLocation = 31908360,
                EndLocation = 34303465,
                Centimorgans = 2.45,
                MatchingSnps = 600
            };
            result.First().Should().BeEquivalentTo(expected);
        }
    }
}