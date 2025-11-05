using NAudio;
using NAudio.Wave;
using System;
using System.Diagnostics.Eventing.Reader;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Semana_13_1_
{
    internal class Program
    {
        static string archivoHistorial = "HistorialIncendios.txt";
        
        static void Main()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("==========================================");
            Console.WriteLine("     SISTEMA DE MONITOREO CONTRA INCENDIOS");
            Console.WriteLine("==========================================");
            Console.ResetColor();

            Console.Write("Ingrese la cantidad de pisos: ");
            int nro_pisos = int.Parse(Console.ReadLine());

            string[] pisos = new string[nro_pisos];
            for (int i = 0; i < nro_pisos; i++)
            {
                Console.Write($"Nombre del piso {i + 1}: ");
                pisos[i] = Console.ReadLine();
            }

            int[] sensoresHumo = new int[nro_pisos];
            int[] sensoresTemperatura = new int[nro_pisos];
            bool[] lucesEstromboticas = new bool[nro_pisos];

            Random r = new Random();
            bool energiaRegular = true;
            bool energiaRespaldo = false;
            int contador = 0;


            if (!File.Exists(archivoHistorial))
            {
                File.WriteAllText(archivoHistorial, "=== HISTORIAL DEL SISTEMA CONTRA INCENDIOS ===\n\n");
            }

            int opcion;
            do
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("========= MENÚ PRINCIPAL =========");
                Console.ResetColor();
                Console.WriteLine("1. Monitorear sistema");
                Console.WriteLine("2. Mostrar información de pisos e historial");
                Console.WriteLine("3. Salir");
                Console.Write("Seleccione una opción: ");
                opcion = int.Parse(Console.ReadLine());

                switch (opcion)
                {
                    case 1:
                        MonitorearSistema(pisos, sensoresHumo, sensoresTemperatura, lucesEstromboticas, r,
                            ref energiaRegular, ref energiaRespaldo, ref contador);
                        break;

                    case 2:
                        MostrarInformacion(pisos);
                        break;

                    case 3:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\nSaliendo del sistema...");
                        Console.ResetColor();
                        Thread.Sleep(1500);
                        break;

                    default:
                        Console.WriteLine("Opción inválida. Presione ENTER para intentar de nuevo...");
                        Console.ReadLine();
                        break;
                }

            } while (opcion != 3);
        }

        static bool EscapePresionado()
        {
            if (Console.KeyAvailable)
            {
                var tecla = Console.ReadKey(true);
                if (tecla.Key == ConsoleKey.Escape)
                    return true;
            }
            return false;
        }
        static void MonitorearSistema(string[] pisos, int[] sensoresHumo, int[] sensoresTemperatura,
                                      bool[] luces, Random r, ref bool energiaRegular,
                                      ref bool energiaRespaldo, ref int contador)
        {
            
            do
            {
                contador++;
                if (contador % 10 == 0)
                {
                    energiaRegular = (r.Next(0, 100) > 10);
                    energiaRespaldo = !energiaRegular;
                }

                InicializarSensores(sensoresHumo, sensoresTemperatura, pisos.Length, r);

                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"{"PANEL DE CONTROL - SISTEMA CONTRA INCENDIOS",60}");
                Console.ResetColor();

                Console.WriteLine($"\n{"Estado Energía",-20} {"Regular:",-10} {(energiaRegular ? "Activa" : "Inactiva"),-10} {"Respaldo:",-10} {(energiaRespaldo ? "Activa" : "Inactiva"),-10}");
                Console.WriteLine($"\n{"ID",-5} {"Piso",-15} {"Humo(%)",-10} {"Temp(°C)",-15} {"Luz",-10}");

                bool alarmaGeneral = false;
                bool alarmaadvertencia = false;
                for (int i = 0; i < pisos.Length; i++)
                {
                    bool Alarma = sensoresHumo[i] > 2 || sensoresTemperatura[i] > 50;
                    luces[i] = Alarma;
                    if (Alarma) alarmaadvertencia = true;
                }
                for (int i = 0; i < pisos.Length; i++)
                {
                    bool alarma = sensoresHumo[i] > 3 || sensoresTemperatura[i] > 57;
                    luces[i] = alarma;
                    if (alarma) alarmaGeneral = true;

                    if (alarma)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.BackgroundColor = ConsoleColor.Red;
                        RegistrarEvento(pisos[i], sensoresHumo[i], sensoresTemperatura[i]);
                    }
                    else if (alarmaadvertencia)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.BackgroundColor = ConsoleColor.DarkYellow;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.BackgroundColor = ConsoleColor.Black;
                    }

                    Console.WriteLine($"{(i + 1),-5} {pisos[i],-15} {sensoresHumo[i],-10} {sensoresTemperatura[i],-15} {(alarma ? "Activa" : "Inactiva"),-10}");
                    Console.ResetColor();
                }

                if (alarmaGeneral)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n¡ALARMA ACTIVADA! Evacúe inmediatamente.");
                    Console.ResetColor();
                    alarma();
                }


                else if (alarmaadvertencia)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("\nSe detecta un posible incendio");
                    Console.ResetColor();

                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\nSistema estable. No se detectan anomalías.");
                    Console.ResetColor();
                }


                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nActualización automática cada 10 segundos...");
                Console.WriteLine("Presione ESC para detener el monitoreo.");
                Console.ResetColor();

                int segundos = 10;
                for (int i = segundos; i > 0; i--)
                {
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write($"Siguiente actualización en {i} segundos...   ");
                    Thread.Sleep(1000);
                }

            } while (!EscapePresionado());
        }
        static void alarma()
        {
            AudioFileReader ubicacionCancion = new AudioFileReader("alarma-de-evacuacin-evacuacion.mp3");
            WaveOutEvent reproductor = new WaveOutEvent();
            reproductor.Init(ubicacionCancion);
            reproductor.Play();
        }
       
        static void InicializarSensores(int[] sensoresHumo, int[] sensoresTemperatura, int nro_pisos, Random r)
        {
            for (int i = 0; i < nro_pisos; i++)
            {
                sensoresHumo[i] = r.Next(0, 6);
                sensoresTemperatura[i] = r.Next(20, 61);
            }
        }

        static void RegistrarEvento(string piso, int humo, int temperatura)
        {
            string registro = $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] ALERTA en {piso} | Humo: {humo}% | Temperatura: {temperatura}°C\n";
            File.AppendAllText(archivoHistorial, registro);
        }

        static void MostrarInformacion(string[] pisos)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== INFORMACIÓN DE PISOS ===\n");
            Console.ResetColor();

            for (int i = 0; i < pisos.Length; i++)
                Console.WriteLine($"Piso {i + 1}: {pisos[i]}");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n=== HISTORIAL DE EVENTOS ===\n");
            Console.ResetColor();

            if (File.Exists(archivoHistorial))
            {
                string contenido = File.ReadAllText(archivoHistorial);
                Console.WriteLine(contenido);
            }
            else
            {
                Console.WriteLine("No hay eventos registrados.");
            }

            Console.WriteLine("\nPresione ENTER para volver al menú...");
            Console.ReadLine();
        }
    }
}