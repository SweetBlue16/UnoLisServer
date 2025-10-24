﻿using System;
using System.Configuration;
using System.ServiceModel;
using UnoLisServer.Services;

namespace UnoLisServer.Host
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "UNO LIS - WCF Server";

            try
            {
                // ⚙️ Configurar conexión EF desde variables de entorno
                ConfigInitializer.ApplyDatabaseConnectionFromEnv();
            }
            catch (Exception ex)
            {
                WriteError("Error al aplicar configuración de base de datos.", ex);
                return; // no continuar si no hay conexión válida
            }

            // 📦 Lista de servicios a hospedar
            ServiceHost[] hosts =
            {
                new ServiceHost(typeof(LoginManager)),
                new ServiceHost(typeof(RegisterManager)),
                new ServiceHost(typeof(ConfirmationManager)),
                new ServiceHost(typeof(ProfileEditManager)),
                new ServiceHost(typeof(ProfileViewManager)),
                new ServiceHost(typeof(FriendsManager)),
                new ServiceHost(typeof(PartyHostManager)),
                new ServiceHost(typeof(PartyClientManager)),
                new ServiceHost(typeof(GameplayManager)),
                new ServiceHost(typeof(NotificationsManager)),
                new ServiceHost(typeof(ChatManager)),
                new ServiceHost(typeof(LeaderboardsManager)),
                new ServiceHost(typeof(ShopManager)),
                new ServiceHost(typeof(LogoutManager))
            };

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("🚀 Iniciando servicios UNO LIS...");
            Console.ResetColor();

            // 🧱 Iniciar cada servicio individualmente con control de excepciones
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
            Console.WriteLine("=================================");
            Console.ResetColor();
            Console.WriteLine("Presiona [ENTER] para detener...");
            Console.ReadLine();

            // 🧹 Cierre seguro
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
            Console.WriteLine("\n🛑 Servidor detenido correctamente.");
            Console.ResetColor();
        }

        // 🧾 Método auxiliar para imprimir errores uniformemente
        private static void WriteError(string context, Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ {context}");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"   → {ex.GetType().Name}: {ex.Message}\n");
            Console.ResetColor();
        }

        // 🔒 Cerrar host con seguridad
        private static void AbortHost(ServiceHost host)
        {
            if (host.State == CommunicationState.Faulted)
            {
                host.Abort();
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"   ⚠️ {host.Description.ServiceType.Name} abortado por error.");
                Console.ResetColor();
            }
        }
    }
}
