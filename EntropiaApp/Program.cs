using System;
using EntropiaApp.Services;
using EntropiaApp.Utils;

namespace EntropiaApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var fileReader = new FileReader();
            fileReader.ReadFile();
            var rows = fileReader.Arrays;
            var antropyCalculator = new LastRowEntropyService(rows);
            antropyCalculator.CalculateEntropies();
            Console.ReadKey();
        }
    }
}
