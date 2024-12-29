using System;
using System.Net.Sockets;
using System.Text;

namespace Klijent
{
    internal class Client
    {
        static void Main(string[] args)
        {
            try
            {
                string serverAddress = "127.0.0.1";
                int port = 8889; 

                while (true)
                {
                    Console.WriteLine("Izaberite opciju:");
                    Console.WriteLine("1. Registracija");
                    Console.WriteLine("2. Pregled stanja");
                    Console.WriteLine("3. Transakcija");
                    Console.WriteLine("4. Izlaz");
                    string izbor = Console.ReadLine();

                    if (izbor == "4") break;

                    TcpClient tcpClient = new TcpClient(serverAddress, port);
                    NetworkStream stream = tcpClient.GetStream();

                    string zahtev = string.Empty;
                    switch (izbor)
                    {
                        case "1":
                            Console.Write("Unesite ime: ");
                            string ime = Console.ReadLine();
                            Console.Write("Unesite prezime: ");
                            string prezime = Console.ReadLine();
                            Console.Write("Unesite lozinku: ");
                            string lozinka = Console.ReadLine();
                            Console.Write("Unesite početno stanje na računu: ");
                            double stanje;
                            while (!double.TryParse(Console.ReadLine(), out stanje) || stanje < 0)
                            {
                                Console.Write("Pogrešan unos! Unesite validan broj za stanje: ");
                            }

                            zahtev = $"REGISTRACIJA|{ime}|{prezime}|{lozinka}|{stanje}";
                            break;

                        case "2":
                            Console.Write("Unesite lozinku: ");
                            string lozinkaKorisnika = Console.ReadLine();

                            zahtev = $"STANJE|{lozinkaKorisnika}";  
                            break;

                        case "3":
                            Console.Write("Unesite iznos za transakciju: ");
                            double iznos;
                            while (!double.TryParse(Console.ReadLine(), out iznos) || iznos <= 0)
                            {
                                Console.Write("Pogrešan unos! Unesite validan iznos: ");
                            }
                            zahtev = $"TRANSAKCIJA|{iznos}";
                            break;

                        default:
                            Console.WriteLine("Nepoznata opcija.");
                            continue;
                    }

                    byte[] messageBytes = Encoding.UTF8.GetBytes(zahtev);
                    stream.Write(messageBytes, 0, messageBytes.Length);

                    byte[] responseBuffer = new byte[1024];
                    int bytesRead = stream.Read(responseBuffer, 0, responseBuffer.Length);
                    string responseMessage = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);
                    Console.WriteLine($"Odgovor od filijale: {responseMessage}");

                    tcpClient.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška: {ex.Message}");
            }
        }
    }
}
