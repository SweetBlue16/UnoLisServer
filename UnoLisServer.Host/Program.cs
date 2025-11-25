using System;
using System.Configuration;
using System.ServiceModel;
using UnoLisServer.Services;
using UnoLisServer.Common.Helpers;

namespace UnoLisServer.Host
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "UNO LIS - WCF Server";


            Logger.Log("Iniciando servidor UNO LIS...");
            Logger.Log($"Conexión configurada para servidor: {Environment.MachineName}");

            try
            {
                ConfigInitializer.ApplyDatabaseConnectionFromEnv();
            }
            catch (Exception ex)
            {
                WriteError("Error al aplicar configuración de base de datos.", ex);
                return;
            }

            ServiceHost[] hosts =
            {
                new ServiceHost(typeof(LoginManager)),
                new ServiceHost(typeof(RegisterManager)),
                new ServiceHost(typeof(ConfirmationManager)),
                new ServiceHost(typeof(ProfileEditManager)),
                new ServiceHost(typeof(ProfileViewManager)),
                new ServiceHost(typeof(FriendsManager)),
                new ServiceHost(typeof(GameplayManager)),
                new ServiceHost(typeof(NotificationsManager)),
                new ServiceHost(typeof(ChatManager)),
                new ServiceHost(typeof(LeaderboardsManager)),
                new ServiceHost(typeof(ShopManager)),
                new ServiceHost(typeof(LogoutManager)),
                new ServiceHost(typeof(AvatarManager)),
                new ServiceHost(typeof(MatchmakingManager)),
                new ServiceHost(typeof(LobbyDuplexManager)),
                new ServiceHost(typeof(ReportManager))
            };

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("🚀 Iniciando servicios UNO LIS...");
            Console.ResetColor();

            foreach (var host in hosts)
            {
                try
                {
                    host.Open();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"✔️ {host.Description.ServiceType.Name} activo.");
                }
                catch (AddressAlreadyInUseException ex)
                {
                    WriteError($"El puerto del servicio {host.Description.ServiceType.Name} ya está en uso.", ex);
                    AbortHost(host);
                }
                catch (AddressAccessDeniedException ex)
                {
                    WriteError($"Acceso denegado al puerto de {host.Description.ServiceType.Name}. Ejecuta como Administrador.", ex);
                    AbortHost(host);
                }
                catch (ConfigurationErrorsException ex)
                {
                    WriteError($"Error en configuración de {host.Description.ServiceType.Name}.", ex);
                    AbortHost(host);
                }
                catch (InvalidOperationException ex)
                {
                    WriteError($"Configuración inválida o contrato ausente en {host.Description.ServiceType.Name}.", ex);
                    AbortHost(host);
                }
                catch (CommunicationException ex)
                {
                    WriteError($"Fallo de comunicación al iniciar {host.Description.ServiceType.Name}.", ex);
                    AbortHost(host);
                }
                catch (TimeoutException ex)
                {
                    WriteError($"Tiempo de espera excedido al iniciar {host.Description.ServiceType.Name}.", ex);
                    AbortHost(host);
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    WriteError($"Error de base de datos al iniciar {host.Description.ServiceType.Name}.", ex);
                    AbortHost(host);
                }
                catch (Exception ex)
                {
                    WriteError($"Error inesperado en {host.Description.ServiceType.Name}.", ex);
                    AbortHost(host);
                }
                finally
                {
                    Console.ResetColor();
                }
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=================================");
            Console.WriteLine("   UNO LIS SERVER EN EJECUCIÓN");
            Console.WriteLine("=================================\n");
            Console.ResetColor();
            Console.WriteLine("Presiona [ENTER] para detener...");
            Console.ReadLine();

            foreach (var host in hosts)
            {
                try
                {
                    if (host.State == CommunicationState.Opened)
                        host.Close();
                }
                catch
                {
                    host.Abort();
                }
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n Servidor detenido correctamente.");
            Console.ResetColor();
        }

        private static void WriteError(string context, Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($" {context}");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"   {ex.GetType().Name}: {ex.Message}\n");
            Console.ResetColor();
        }
        private static void AbortHost(ServiceHost host)
        {
            if (host.State == CommunicationState.Faulted)
            {
                host.Abort();
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"    {host.Description.ServiceType.Name} abortado por error.");
                Console.ResetColor();
            }
        }
    }
}
