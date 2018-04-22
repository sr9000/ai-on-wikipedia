using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ai
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("print query = ");
            string init = Console.ReadLine().ToLower();
            var finder = AI.FindAssociationsAsync(new SGF().AggregateFrom(init.Split(',').Select(x => new MSGF().SetContent(x)).ToList()));
            var associationsResult = finder.GetResult();
            while (true)
            {
                string cmd = Console.ReadLine().ToLower();
                if (cmd == "exit")
                {
                    return;
                }
                if (cmd == "get")
                {
                    associationsResult = finder.GetResult();
                }
                else if (cmd == "stop")
                {
                    associationsResult.Pause();
                }
                else if (cmd == "start")
                {
                    associationsResult.Run(uint.Parse(Console.ReadLine()));
                }
                else if (cmd == "export")
                {
                    associationsResult.ExportToTextFile(Console.ReadLine());
                }
                else if (cmd == "print")
                {
                    foreach (var source in associationsResult.FingUsecases().OrderBy(pair => -pair.Value))
                    {
                        Console.WriteLine("..." + source.Key + " " + source.Value);
                    }
                }
            }
        }
    }
}
