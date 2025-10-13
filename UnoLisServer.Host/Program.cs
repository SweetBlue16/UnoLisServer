using System;
using System.ServiceModel;
using UnoLisServer.Services;

namespace UnoLisServer.Host
{
    internal class Program
    {
 

        static void Main(string[] args)
        {
            Console.Title = "UNO LIS - WCF Server";

            // 🔹 Definir los hosts a levantar
            ServiceHost[] hosts = new ServiceHost[]
            {
                new ServiceHost(typeof(AuthManager)),
                new ServiceHost(typeof(ProfileManager)),
                new ServiceHost(typeof(FriendsManager)),
                new ServiceHost(typeof(PartyHostManager)),
                new ServiceHost(typeof(PartyClientManager)),
                new ServiceHost(typeof(GameplayManager)),
                new ServiceHost(typeof(NotificationsManager)) // 👈 agregado
            };

            try
            {
                foreach (var host in hosts)
                {
                    host.Open();
                    Console.WriteLine($"✔️ Servicio {host.Description.ServiceType.Name} activo.");
                }

                Console.WriteLine("\n==============================");
                Console.WriteLine("   UNO LIS SERVER EN EJECUCIÓN");
                Console.WriteLine("==============================\n");
                Console.WriteLine("Presiona [ENTER] para detener...");

                Console.ReadLine();

                foreach (var host in hosts)
                {
                    host.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al iniciar servicios: {ex.Message}");

                foreach (var host in hosts)
                {
                    if (host.State == CommunicationState.Faulted)
                    {
                        host.Abort();
                    }
                }
            }
            finally
            {
                Console.WriteLine(" ");
            }
        }
    }
}

