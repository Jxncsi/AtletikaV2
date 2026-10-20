namespace Atletika
{
    public class Versenyzo
    {
        int rajtszam;
        string nev;
        string versenyszam;
        bool nem;
        int eletkor;
        bool ob, vb, eb;
		public Versenyzo(string sor)
		{
			string[] split = sor.Split(';');
			rajtszam = int.Parse(split[0]);
			nev = split[1];
			versenyszam = split[2];
			nem = split[3] == "F";
			eletkor = int.Parse(split[4]);
			ob = split[5] == "1";
			vb = split[6] == "1";
			eb = split[7] == "1";
		}

		public int Rajtszam => rajtszam;
		public string Nev => nev;
		public string VersenySzam => versenyszam;
		public bool Nem => nem;
		public int Eletkor => eletkor;
		public bool Ob => ob;
		public bool Vb => vb;
		public bool Eb => eb;

		public override string ToString() => $"{rajtszam} | {nev} | {versenyszam} | {(nem ? "Férfi" : "Nő")} | {eletkor} | {(ob ? 1 : 0)} | {(vb ? 1 : 0)} | {(eb ? 1 : 0)}";


		public string MentesFormazas() => $"{rajtszam};{nev};{versenyszam};{(nem ? "F" : "N")};{eletkor};{(ob ? 1 : 0)};{(vb ? 1 : 0)};{(eb ? 1 : 0)}";
    }
}
