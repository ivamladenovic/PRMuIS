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
        private static readonly string sifra = "tajna123"; // Tajna šifra za enkripciju

        static void Main(string[] args)
        {
            try
            {
                // --- 1. UDP deo: cekanje START poruke sa servera ---
                int udpPort = 7777;
                using (UdpClient udpClient = new UdpClient(udpPort))
                {
                    Console.WriteLine($"Čekam UDP START poruku na portu {udpPort}...");
                    IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] receivedBytes = udpClient.Receive(ref remoteEP);
                    string message = Encoding.UTF8.GetString(receivedBytes);

                    if (message == "START")
                    {
                        Console.WriteLine($"Primljena START poruka od servera ({remoteEP.Address}).");
                    }
                    else
                    {
                        Console.WriteLine("Nepoznata UDP poruka, izlazim.");
                        return;
                    }
                }

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

                TcpListener tcpListener = new TcpListener(IPAddress.Any, port);
                tcpListener.Start();
                Console.WriteLine($"Filijala pokrenuta na portu {port} (TCP).");

                while (true)
                {
                    TcpClient client = tcpListener.AcceptTcpClient();
                    Console.WriteLine("Klijent se povezao.");

                    NetworkStream stream = client.GetStream();
                    byte[] buffer = new byte[4096];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);

                    string encryptedRequest = Encoding.UTF8.GetString(buffer, 0, bytesRead);
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
                        client.Close();
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

                    response += "<END>"; // Za detekciju kraja odgovora na klijentskoj strani
                    Console.WriteLine($"Originalan odgovor za klijenta: {response}");

                    string encryptedResponse = Enkriptor.Encrypt(response, sifra);
                    Console.WriteLine($"ENKRIPTOVAN odgovor ka klijentu: {encryptedResponse}");

                    byte[] responseBytes = Encoding.UTF8.GetBytes(encryptedResponse);
                    stream.Write(responseBytes, 0, responseBytes.Length);
                    client.Close();
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

                TcpClient serverClient = new TcpClient("127.0.0.1", 8888);
                NetworkStream serverStream = serverClient.GetStream();

                byte[] requestBytes = Encoding.UTF8.GetBytes(encryptedRequest);
                serverStream.Write(requestBytes, 0, requestBytes.Length);

                byte[] responseBytes = new byte[4096];
                int bytesRead = serverStream.Read(responseBytes, 0, responseBytes.Length);
                string encryptedResponse = Encoding.UTF8.GetString(responseBytes, 0, bytesRead);

                Console.WriteLine($"Primljen ENKRIPTOVAN odgovor od servera: {encryptedResponse}");

                string response = Enkriptor.Decrypt(encryptedResponse, sifra);
                Console.WriteLine($"Dekriptovan odgovor od servera: {response}");

                serverClient.Close();
                return response;
            }
            catch (Exception ex)
            {
                return $"Greška pri komunikaciji sa serverom: {ex.Message}";
            }
        }
    }
}
