using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Common; // Dodato za Enkriptor

namespace Filijala
{
    internal class Filijala
    {
        private static double maksimalniBudzet = 0;
        private static readonly string sifra = "iva12"; // Tajna šifra za enkripciju

        static void Main(string[] args)
        {
            try
            {
                // --- 1. UDP deo: cekanje START poruke sa servera ---
                int udpPort = 7777;

                // Kreiramo UDP socket
                Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                // Bindujemo socket na port i sve interfejse
                udpSocket.Bind(new IPEndPoint(IPAddress.Any, udpPort));

                Console.WriteLine($"Čekam UDP START poruku na portu {udpPort}...");

                // Pripremimo buffer i endpoint za prijem podataka
                byte[] buffer = new byte[1024];
                EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

                // Primamo poruku sa bilo kog izvora
                int bytesRead = udpSocket.ReceiveFrom(buffer, ref remoteEP);

                // Pretvaramo primljene bajtove u string
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                if (message == "START")
                {
                    Console.WriteLine($"Primljena START poruka od servera ({remoteEP.ToString()}).");
                }
                else
                {
                    Console.WriteLine("Nepoznata UDP poruka, izlazim.");
                    udpSocket.Close();
                    return;
                }

                udpSocket.Close();

                // --- 2. TCP inicijalizacija filijale: salji INIT serveru ---
                string initResponse = ProslediZahtevServeru("INIT");
                if (initResponse.StartsWith("MAKS_BUDZET|"))
                {
                    string[] parts = initResponse.Split('|');
                    if (parts.Length == 2 && double.TryParse(parts[1], out double budzet))
                    {
                        maksimalniBudzet = budzet;
                        Console.WriteLine($"Maksimalni budžet preuzet od servera: {maksimalniBudzet}");
                    }
                }
                else
                {
                    Console.WriteLine("Neuspešna inicijalizacija filijale sa serverom.");
                }

                // --- 3. TCP server za klijente ---
                int port = 8889;
                if (args.Length > 0 && int.TryParse(args[0], out int uneseniPort))
                {
                    port = uneseniPort;
                }
                else
                {
                    Console.Write("Unesite port na kojem želite da pokrenete filijalu (npr. 8890): ");
                    while (!int.TryParse(Console.ReadLine(), out port)) ;
                }

                Socket listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listenerSocket.Bind(new IPEndPoint(IPAddress.Any, port));
                listenerSocket.Listen(10);
                Console.WriteLine($"Filijala pokrenuta na portu {port} (TCP).\n");

                while (true)
                {
                    Socket klijentSocket = listenerSocket.Accept();
                    Console.WriteLine("Klijent se povezao.");

                    byte[] recvBuffer = new byte[4096];
                    int bytesReadClient = klijentSocket.Receive(recvBuffer);

                    string encryptedRequest = Encoding.UTF8.GetString(recvBuffer, 0, bytesReadClient);
                    Console.WriteLine($"Primljen ENKRIPTOVAN zahtev od klijenta: {encryptedRequest}");

                    string request;
                    try
                    {
                        request = Enkriptor.Decrypt(encryptedRequest, sifra);
                        Console.WriteLine($"Dekriptovan zahtev: {request}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Neuspela dekripcija zahteva: " + ex.Message);
                        klijentSocket.Close();
                        continue;
                    }

                    string response;

                    if (request.StartsWith("REGISTRACIJA") || request.StartsWith("PRIJAVA") ||
                        request.StartsWith("STANJE") || request.StartsWith("TRANSAKCIJA") ||
                        request.StartsWith("TRANSFER") || request.StartsWith("ISTORIJA"))
                    {
                        response = ProslediZahtevServeru(request);
                    }
                    else
                    {
                        response = "Nepoznat zahtev.";
                    }

                    Console.WriteLine($"Originalan odgovor za klijenta: {response}");

                    string encryptedResponse = Enkriptor.Encrypt(response, sifra);
                    Console.WriteLine($"ENKRIPTOVAN odgovor ka klijentu: {encryptedResponse}");

                    byte[] responseBytes = Encoding.UTF8.GetBytes(encryptedResponse);
                    klijentSocket.Send(responseBytes);
                    klijentSocket.Shutdown(SocketShutdown.Both);
                    klijentSocket.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška na filijali: {ex.Message}");
            }
        }

        private static string ProslediZahtevServeru(string request)
        {
            try
            {
                Console.WriteLine($"Originalan zahtev ka serveru: {request}");
                string encryptedRequest = Enkriptor.Encrypt(request, sifra);
                Console.WriteLine($"ENKRIPTOVAN zahtev ka serveru: {encryptedRequest}");

                Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Connect("127.0.0.1", 8888);

                byte[] requestBytes = Encoding.UTF8.GetBytes(encryptedRequest);
                serverSocket.Send(requestBytes);

                byte[] responseBytes = new byte[4096];
                int bytesRead = serverSocket.Receive(responseBytes);

                string encryptedResponse = Encoding.UTF8.GetString(responseBytes, 0, bytesRead);
                Console.WriteLine($"Primljen ENKRIPTOVAN odgovor od servera: {encryptedResponse}");

                string response = Enkriptor.Decrypt(encryptedResponse, sifra);
                Console.WriteLine($"Dekriptovan odgovor od servera: {response}");

                serverSocket.Shutdown(SocketShutdown.Both);
                serverSocket.Close();

                return response;
            }
            catch (Exception ex)
            {
                return $"Greška pri komunikaciji sa serverom: {ex.Message}";
            }
        }
    }
}
