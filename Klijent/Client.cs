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
                Console.Write("Unesite port filijale na koju želite da se povežete (npr. 8889, 8890...): ");
                int port;
                while (!int.TryParse(Console.ReadLine(), out port)) ;


                while (true)
                {
                    Console.WriteLine("Izaberite opciju:");
                    Console.WriteLine("1. Registracija");
                    Console.WriteLine("2. Prijava");
                    Console.WriteLine("3. Pregled stanja");
                    Console.WriteLine("4. Transakcija");
                    Console.WriteLine("5. Transfer sredstava");
                    Console.WriteLine("6. Pregled istorije transakcija");
                    Console.WriteLine("7. Izlaz");
                    string izbor = Console.ReadLine();

                    if (izbor == "7") break;

                    TcpClient tcpClient = new TcpClient(serverAddress, port);
                    NetworkStream stream = tcpClient.GetStream();

                    string zahtev = string.Empty;
                    switch (izbor)
                    {
                        case "1":
                            // Registracija
                            Console.Write("Unesite ime: ");
                            string ime = Console.ReadLine();
                            Console.Write("Unesite prezime: ");
                            string prezime = Console.ReadLine();

                            string lozinka;
                            do
                            {
                                Console.Write("Unesite lozinku (min 6 karaktera): ");
                                lozinka = Console.ReadLine();
                                if (lozinka.Length < 6)
                                {
                                    Console.WriteLine("Lozinka mora imati najmanje 6 karaktera!");
                                }
                            }
                            while (lozinka.Length < 6);

                            Console.Write("Unesite limit za isplatu sa računa: ");
                            double limitZaIsplatu;
                            while (!double.TryParse(Console.ReadLine(), out limitZaIsplatu) || limitZaIsplatu <= 0)
                            {
                                Console.Write("Pogrešan unos! Unesite validan broj za limit: ");
                            }

                            zahtev = $"REGISTRACIJA|{ime}|{prezime}|{lozinka}|{limitZaIsplatu}";
                            break;

                        case "2":
                            // Prijava
                            Console.Write("Unesite ime: ");
                            string imeKorisnika = Console.ReadLine();
                            Console.Write("Unesite lozinku: ");
                            string lozinkaKorisnika = Console.ReadLine();

                            zahtev = $"PRIJAVA|{imeKorisnika}|{lozinkaKorisnika}";
                            break;

                        case "3":
                            // Pregled stanja
                            Console.Write("Unesite lozinku: ");
                            string lozinkaStanja = Console.ReadLine();

                            zahtev = $"STANJE|{lozinkaStanja}";
                            break;

                        case "4":
                            // Transakcija
                            Console.WriteLine("Izaberite tip transakcije:");
                            Console.WriteLine("1. Uplata");
                            Console.WriteLine("2. Isplata");
                            string tipTransakcije = Console.ReadLine() == "1" ? "UPLATA" : "ISPLATA";

                            Console.Write("Unesite iznos za transakciju: ");
                            double iznos;
                            while (!double.TryParse(Console.ReadLine(), out iznos) || iznos <= 0)
                            {
                                Console.Write("Pogrešan unos! Unesite validan iznos: ");
                            }

                            Console.Write("Unesite lozinku: ");
                            string lozinkaTransakcije = Console.ReadLine();

                            zahtev = $"TRANSAKCIJA|{tipTransakcije}|{iznos}|{lozinkaTransakcije}";
                            break;

                        case "5":
                            // Transfer sredstava - NOVO
                            Console.Write("Unesite lozinku pošiljaoca: ");
                            string lozinkaPosiljaoca = Console.ReadLine();

                            Console.Write("Unesite lozinku primaoca: ");
                            string lozinkaPrimaoca = Console.ReadLine();

                            Console.Write("Unesite iznos za transfer: ");
                            double iznosTransfer;
                            while (!double.TryParse(Console.ReadLine(), out iznosTransfer) || iznosTransfer <= 0)
                            {
                                Console.Write("Pogrešan unos! Unesite validan iznos: ");
                            }

                            zahtev = $"TRANSFER|{lozinkaPosiljaoca}|{lozinkaPrimaoca}|{iznosTransfer}";
                            break;
                        case "6":
                            Console.Write("Unesite lozinku: ");
                            string lozinkaIstorije = Console.ReadLine();
                            zahtev = $"ISTORIJA|{lozinkaIstorije}";
                            break;

                        default:
                            Console.WriteLine("Nepoznata opcija.");
                            continue;
                    }

                    byte[] messageBytes = Encoding.UTF8.GetBytes(zahtev);
                    stream.Write(messageBytes, 0, messageBytes.Length);

                    //byte[] responseBuffer = new byte[1024];
                    //int bytesRead = stream.Read(responseBuffer, 0, responseBuffer.Length);
                    //string responseMessage = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);
                    //Console.WriteLine($"{responseMessage}");
                    byte[] buffer = new byte[4096];
                    StringBuilder odgovorBuilder = new StringBuilder();

                    int bytesRead;
                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        string deo = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        odgovorBuilder.Append(deo);

                        if (deo.Contains("<END>")) // 🟢 našli kraj poruke
                            break;
                    }

                    string odgovor = odgovorBuilder.ToString().Replace("<END>", "").Trim();
                    Console.WriteLine("\n--- Odgovor servera ---");
                    Console.WriteLine(odgovor);
                    Console.WriteLine("------------------------\n");

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
