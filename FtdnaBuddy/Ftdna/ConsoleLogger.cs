using System;

namespace FtdnaBuddy.Ftdna
{
    internal class ConsoleLogger : ILogger
    {
        public void LogInfo(string text)
        {
            Console.WriteLine(text);
        }

        public void LogError(string text)
        {
            Console.Error.WriteLine(text);
        }
    }
}
