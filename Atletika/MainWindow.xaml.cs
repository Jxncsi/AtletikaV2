using System.IO;
using Microsoft.Win32;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.Unicode;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Atletika
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
		public ObservableCollection<Versenyzo> Versenyzok { get; } = new();
		ICollectionView? nezet;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            nezet = CollectionViewSource.GetDefaultView(Versenyzok);
            nezet.Filter = SzuroFeltetel;
            FrissitSzamlalo();
        }

		private void btnRegisztral_Click(object sender, RoutedEventArgs e)
		{
			string nev = tbNev.Text.Trim();
			if (nev == "") {
				Figyelmeztetes("A versenyző nevének megadása kötelező."); 
				return; 
			}
			if (nev.Contains(';') || nev.Contains('|'))
			{
				Figyelmeztetes("A névben pontosvessző és függőleges vonal nem használható.");
				return;
			}
			if (cbVersenySzam.SelectedItem == null) 
			{
				Figyelmeztetes("Válasszon versenyszámot.");
				return;
			}
			if (rbFerfi.IsChecked != true && rbNo.IsChecked != true)
			{
				Figyelmeztetes("Adja meg a versenyző nemét.");
				return;
			}

			int rajtszam;
			if (txtRajtszam.Text.Trim() != "")
			{
				if (!int.TryParse(txtRajtszam.Text.Trim(), out rajtszam) || rajtszam < 1 || rajtszam > 9999)
				{
					Figyelmeztetes("A rajtszám 1 és 9999 közötti egész szám lehet.");
					return;
				}
				if (Foglalt(rajtszam))
				{
					Figyelmeztetes("A kért rajtszám már foglalt. Adjon meg másikat, vagy hagyja üresen a mezőt.");
					return;
				}
			}
			else
			{
				rajtszam = 1;
				while (rajtszam <= 9999 && Foglalt(rajtszam)) rajtszam++;
				if (rajtszam > 9999)
				{
					Figyelmeztetes("Nincs több szabad rajtszám.");
					return;
				}
			}

			string vsz = cbVersenySzam.SelectedItem.ToString();
			string nemKod = rbFerfi.IsChecked == true ? "F" : "N";
			Versenyzok.Add(new Versenyzo($"{rajtszam};{nev};{vsz};{nemKod};{sldEletkor.Value};{Convert.ToInt32(chkOR.IsChecked)};{Convert.ToInt32(chkVR.IsChecked)};{Convert.ToInt32(chkER.IsChecked)}"));

			FrissitSzamlalo();

			MessageBox.Show($"A regisztráció sikeres. Kiosztott rajtszám: {rajtszam}", "Regisztráció", MessageBoxButton.OK, MessageBoxImage.Information);
			tbNev.Clear();
			txtRajtszam.Clear();
		}

		private bool Foglalt(int szam)
		{
			return Versenyzok.Any(v => v.Rajtszam == szam);
		}

		private static void Figyelmeztetes(string uzenet)
		{
			MessageBox.Show(uzenet, "Figyelmeztetés", MessageBoxButton.OK, MessageBoxImage.Warning);
		}
		private bool SzuroFeltetel(object item)
		{
			if (item is Versenyzo v)
			{
				if (cmbRekordSzuro == null || cmbRekordSzuro.SelectedIndex <= 0)
				{
					return true;
				}

				switch (cmbRekordSzuro.SelectedIndex)
				{
					case 1: return v.OR;
					case 2: return v.VR;
					case 3: return v.ER;
					default: return true;
				}
			}
			return false;
		}

		private void FrissitSzamlalo()
		{
			txtSzamlalo.Text = $"Látható: {nezet!.Cast<object>().Count()} / Összesen: {Versenyzok.Count}";
		}

		private void Szuro_Valtozott(object sender, SelectionChangedEventArgs e)
		{
			if (nezet == null) return;
			nezet.Refresh();
			FrissitSzamlalo();
		}

		private void Mentes_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog sfd = new();
			sfd.Filter = "CSV-fájl (*.csv)|*.csv|Szövegfájl (*.txt)|*.txt";
			sfd.FilterIndex = 1;
			sfd.AddExtension = false;
			if (sfd.ShowDialog() != true) return;

			bool csv = sfd.FilterIndex == 1;
			string tipus = csv ? ".csv" : ".txt";
			string fajl = sfd.FileName;
			string mostani = System.IO.Path.GetExtension(fajl);
			if (mostani == "") fajl += tipus;
			else if (!mostani.Equals(tipus, StringComparison.OrdinalIgnoreCase))
			{
				Figyelmeztetes($"A kiválasztott fájltípus ({tipus}) és a fájlnév kiterjesztése ({mostani}) nem egyezik.");
				return;
			}

			try
			{
				string tartalom = csv
					? "Rajtszam;Nev;Versenyszam;Nem;Eletkor;OR;VR;ER\n" + string.Concat(Versenyzok.Select(v => v.MentesFormazas()))
					: string.Concat(Versenyzok.Select(v => v.SzovegesLeiras()));
				File.WriteAllText(fajl, tartalom, Encoding.UTF8);
			}
			catch (Exception ex)
			{
				MessageBox.Show("A mentés nem sikerült: " + ex.Message, "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void Betoltes_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog ofd = new();
			ofd.Filter = "CSV-fájl (*.csv)|*.csv";
			if (ofd.ShowDialog() != true) return;
			List<Versenyzo> uj = new();
			try
			{
				string[] sorok = File.ReadAllLines(ofd.FileName, Encoding.UTF8);
				foreach (string sor in sorok.Skip(1).Where(s => s.Trim() != ""))
				{
					Versenyzo v = new(sor);
					if (uj.Any(x => x.Rajtszam == v.Rajtszam))
					{
						MessageBox.Show("A fájlban ismétlődő rajtszám található: " + v.Rajtszam, "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}
					uj.Add(v);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("A betöltés nem sikerült: " + ex.Message, "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			Versenyzok.Clear();
			foreach (Versenyzo v in uj) Versenyzok.Add(v);
			FrissitSzamlalo();
			MessageBox.Show($"Betöltve: {uj.Count} versenyző.", "Betöltés", MessageBoxButton.OK, MessageBoxImage.Information);
		}
	}
}
