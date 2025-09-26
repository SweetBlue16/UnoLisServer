using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Data
{
    public class Program
    {
        static void Main(string[] args)
        {
            using (var context = new AppDbContext())
            {
                // Test: contar jugadores
                var count = context.Players.Count();
                Console.WriteLine($"Hay {count} jugadores en la base.");
            }

            Console.ReadKey();
        }
    }
}
