using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Korisnik
    {
        public string IdKorisnika { get; set; }   // ID korisnika
        public string Ime { get; set; }       // Ime korisnika
        public string Prezime { get; set; }   // Prezime korisnika
        public double StanjeNaRačunu { get; set; } // Stanje na računu

        // Konstruktor klase
        public Korisnik(string Id, string ime, string prezime, double stanjeNaRačunu)
        {
            IdKorisnika = Id;
            Ime = ime;
            Prezime = prezime;
            StanjeNaRačunu = stanjeNaRačunu;
        }

        // Metoda za ispis podataka o korisniku
        public override string ToString()
        {
            return $"{Ime} {Prezime} (ID: {IdKorisnika}) - Stanje na računu: {StanjeNaRačunu}";
        }
    }
}

