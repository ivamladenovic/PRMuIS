using System;
using System.Net.Sockets;
using System.Text;

namespace Filijala
{
    internal class Filijala
    {
        static void Main(string[] args)
        {
            try
            {
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

                TcpListener tcpListener = new TcpListener(System.Net.IPAddress.Any, port);
                tcpListener.Start();
                Console.WriteLine($"Filijala pokrenuta na portu {port} (TCP).");

                while (true)
                {
                    TcpClient client = tcpListener.AcceptTcpClient();
                    Console.WriteLine("Klijent se povezao.");

                    NetworkStream stream = client.GetStream();
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    string response = string.Empty;
                    response = ProslediZahtevServeru("INIT"); // inicijalno možeš preskočiti

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

                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
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
                TcpClient serverClient = new TcpClient("127.0.0.1", 8888);
                NetworkStream serverStream = serverClient.GetStream();
                byte[] requestBytes = Encoding.UTF8.GetBytes(request);
                serverStream.Write(requestBytes, 0, requestBytes.Length);

                // Čitanje odgovora od servera
                byte[] responseBytes = new byte[1024];
                int bytesRead = serverStream.Read(responseBytes, 0, responseBytes.Length);
                string response = Encoding.UTF8.GetString(responseBytes, 0, bytesRead);

                Console.WriteLine($"Filijala je primila odgovor od servera: {response}");

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
