using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Data; // Asegúrate de usar el namespace correcto de tu modelo

class Program
{
    static void Main()
    {
        using (var context = new UNOContext())
        {
            // 🔹 Contar los jugadores
            var count = context.Player.Count();
            Console.WriteLine($"Hay {count} jugadores en la base.");

            // 🔹 Insertar un nuevo jugador de prueba
            var newPlayer = new Player
            {
                nickname = "TestModelPlayer",
                fullName = "Jugador creado desde el modelo"
            };

            context.Player.Add(newPlayer);
            context.SaveChanges();

            Console.WriteLine("Jugador agregado correctamente!");
        }

        Console.ReadKey();
    }
}
