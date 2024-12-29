using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Korisnik
    {
        public string IdKorisnika { get; set; }  
        public string Ime { get; set; }       
        public string Prezime { get; set; }   
        public double StanjeNaRačunu { get; set; } 
        public string Lozinka { get; set; }  

        public Korisnik(string Id, string ime, string prezime, double stanjeNaRačunu, string lozinka)
        {
            IdKorisnika = Id;
            Ime = ime;
            Prezime = prezime;
            StanjeNaRačunu = stanjeNaRačunu;
            Lozinka = lozinka;
        }

        public override string ToString()
        {
            return $"{Ime} {Prezime} (ID: {IdKorisnika}) - Stanje na računu: {StanjeNaRačunu} - lozinka {Lozinka}";
        }
    }
}

