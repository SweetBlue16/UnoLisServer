using System;
using System.Linq;
using UnoLisServer.Data;          // referencia a tu DbContext
using UnoLisServer.Data.Entities; // referencia a tus entidades

namespace UnoLisServer.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var context = new AppDbContext())
            {
                // INSERT de prueba
                var player = new Player
                {
                    Nickname = "TestPlayer",
                    FullName = "Jugador de Prueba"
                };

                context.Players.Add(player);
                context.SaveChanges();

                // SELECT de prueba
                var count = context.Players.Count();
                Console.WriteLine($"Hay {count} jugadores en la base.");

                // Mostrar último jugador agregado
                var last = context.Players.OrderByDescending(p => p.PlayerId).FirstOrDefault();
                Console.WriteLine($"Último jugador agregado: {last.Nickname} ({last.FullName})");
            }

            Console.ReadKey();
        }
    }
}


