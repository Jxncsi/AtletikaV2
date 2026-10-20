namespace Atletika
{
    public class Versenyzo
    {
		int rajtszam;
		string nev;
		string versenyszam;
		string nem;
		int eletkor;
		bool or, vr, er;
		public Versenyzo(string sor)
		{
			string[] split = sor.Split(';');
			rajtszam = int.Parse(split[0]);
			nev = split[1];
			versenyszam = split[2];
			nem = split[3];
			eletkor = int.Parse(split[4]);
			or = split[5] == "1";
			vr = split[6] == "1";
			er = split[7] == "1";
		}

		public int Rajtszam => rajtszam;
		public string Nev => nev;
		public string Versenyszam => versenyszam;
		public string Nem => nem;
		public int Eletkor => eletkor;
		public bool OR => or;
		public bool VR => vr;
		public bool ER => er;

		public string MentesFormazas() => $"{rajtszam};{nev};{versenyszam};{nem};{eletkor};{(or ? 1 : 0)};{(vr ? 1 : 0)};{(er ? 1 : 0)}\n";

		public string SzovegesLeiras()
		{
			return $"{Nev}\nA {Eletkor} éves {(Nem == "F" ? "férfi" : "női")} versenyző a(z) {Versenyszam} versenyszámban indul, rajtszáma {Rajtszam}. " +
				$"Olimpiai csúccsal {(or ? "rendelkezik" : "nem rendelkezik")}, világcsúccsal {(vr ? "rendelkezik" : "nem rendelkezik")}, Európa-csúccsal {(er ? "rendelkezik" : "nem rendelkezik")}.\n\n";
		}
    }
}
