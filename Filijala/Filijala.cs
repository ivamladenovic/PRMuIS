using Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Filijala
{
    internal class Filijala
    {
        private static double budzet; 
        private static List<Korisnik> korisnici = new List<Korisnik>(); 

        static void Main(string[] args)
        {
            try
            {
                UdpClient udpClient = new UdpClient();
                string initMessage = "INIT|"; 
                byte[] initBytes = Encoding.UTF8.GetBytes(initMessage);
                udpClient.Send(initBytes, initBytes.Length, "127.0.0.1", 8888);

                IPEndPoint serverEndPoint = null;
                byte[] serverResponse = udpClient.Receive(ref serverEndPoint);
                string serverMessage = Encoding.UTF8.GetString(serverResponse);
                Console.WriteLine($"Odgovor od servera: {serverMessage}");

                budzet = double.Parse(serverMessage); 

                TcpListener tcpListener = new TcpListener(IPAddress.Any, 8889);
                tcpListener.Start();
                Console.WriteLine("Filijala sluša na portu 8889 (TCP).");

                while (true)
                {
                    TcpClient client = tcpListener.AcceptTcpClient();
                    Console.WriteLine("Klijent se povezao.");

                    NetworkStream stream = client.GetStream();
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    string response = ObradiZahtev(request);
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

        private static string ObradiZahtev(string request)
        {
            if (request.StartsWith("REGISTRACIJA"))
            {
                return Registracija(request);
            }
            else if (request.StartsWith("STANJE"))
            {
                return PregledStanja(request);
            }
            else if (request.StartsWith("TRANSAKCIJA"))
            {
                return Transakcija(request);
            }
            else
            {
                return "Nepoznata akcija.";
            }
        }

        private static string Registracija(string request)
        {
            string[] delovi = request.Split('|');
            if (delovi.Length == 5) 
            {
                string ime = delovi[1];
                string prezime = delovi[2];
                string lozinka = delovi[3];
                double pocetnoStanje;

                if (!double.TryParse(delovi[4], out pocetnoStanje) || pocetnoStanje < 0)
                {
                    return "Greška: Nevalidan iznos početnog stanja.";
                }

                string idKorisnika = Guid.NewGuid().ToString();
                Korisnik noviKorisnik = new Korisnik(idKorisnika, ime, prezime, pocetnoStanje, lozinka);
                korisnici.Add(noviKorisnik);
                return $"Registracija uspešna!";
            }
            else
            {
                return "Greška: Nevalidni podaci za registraciju.";
            }
        }


        private static string PregledStanja(string request)
        {
            string[] delovi = request.Split('|');
            if (delovi.Length == 2)  
            {
                string lozinka = delovi[1];  

                Korisnik korisnik = korisnici.Find(k => k.Lozinka == lozinka);

                if (korisnik != null)
                {
                    return $"Stanje na računu korisnika {korisnik.Ime} {korisnik.Prezime} : {korisnik.StanjeNaRačunu} dinara.";
                }
                else
                {
                    return "Greška: Pogrešna lozinka.";
                }
            }
            else
            {
                return "Greška: Nevalidni podaci za pregled stanja.";
            }
        }


        private static string Transakcija(string request)
        {
            string[] delovi = request.Split('|');
            if (delovi.Length == 2)  
            {
                double iznos;
                if (!double.TryParse(delovi[1], out iznos) || iznos <= 0)
                {
                    return "Greška: Nevalidan iznos transakcije. Unesite pozitivan iznos.";
                }

                return $"Transakcija uspešna! Uneti iznos: {iznos}.";
            }
            else
            {
                return "Greška: Nevalidni podaci za transakciju.";
            }
        }

    }
}
