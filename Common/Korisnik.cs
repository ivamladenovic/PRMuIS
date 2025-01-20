namespace Common
{
    public class Korisnik
    {
        public string Id { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public double StanjeNaRačunu { get; set; }
        public string Lozinka { get; set; }
        public double LimitZaIsplatu { get; set; }  

        public Korisnik(string id, string ime, string prezime, double limitZaIsplatu, string lozinka)
        {
            Id = Id;
            Ime = ime;
            Prezime = prezime;
            StanjeNaRačunu = 0;  
            Lozinka = lozinka;
            LimitZaIsplatu = limitZaIsplatu;  
        }

        public override string ToString()
        {
            return $"{Ime} {Prezime} (ID: {Id}) - Stanje na računu: {StanjeNaRačunu} - Limit za isplatu: {LimitZaIsplatu} - lozinka {Lozinka}";
        }
    }
}
