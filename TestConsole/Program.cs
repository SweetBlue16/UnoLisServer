using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Data;

class Program
{
    static void Main()
    {
        using (var context = new UNOContext())
        {
            var count = context.Player.Count();
            Console.WriteLine($"Hay {count} jugadores en la base.");

            var newPlayer = new Player
            {
                nickname = "SweetBlue16",
                fullName = "Mauricio Noriega"
            };

            var newAccount = new Account
            {
                email = "p.sweetblue16@gmail.com",
                password = "Contraseña segura",
                Player = newPlayer
            };

            context.Player.Add(newPlayer);
            context.Account.Add(newAccount);
            context.SaveChanges();

            Console.WriteLine("Jugador agregado correctamente!");
        }
        Console.ReadKey();
    }
}
