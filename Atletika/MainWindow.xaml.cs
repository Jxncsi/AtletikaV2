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

namespace Atletika
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
		System.Collections.ObjectModel.ObservableCollection<Versenyzo> versenyzok = new();
        public MainWindow()
        {
            InitializeComponent();
            dgVersenyzok.ItemsSource = versenyzok;
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

			string vsz = ((ComboBoxItem)cbVersenySzam.SelectedItem).Content.ToString();
			string nemKod = rbFerfi.IsChecked == true ? "F" : "N";
			versenyzok.Add(new Versenyzo($"{rajtszam};{nev};{vsz};{nemKod};{sldEletkor.Value};{Convert.ToInt32(chkOlimpiai.IsChecked)};{Convert.ToInt32(chkVB.IsChecked)};{Convert.ToInt32(chkEB.IsChecked)}"));

			txtSzamlalo.Text = $"Regisztrált versenyzők: {versenyzok.Count}";

			MessageBox.Show($"A regisztráció sikeres. Kiosztott rajtszám: {rajtszam}", "Regisztráció", MessageBoxButton.OK, MessageBoxImage.Information);
			tbNev.Clear();
			txtRajtszam.Clear();
		}

		private bool Foglalt(int szam)
		{
			return versenyzok.Any(v => v.Rajtszam == szam);
		}

		private static void Figyelmeztetes(string uzenet)
		{
			MessageBox.Show(uzenet, "Figyelmeztetés", MessageBoxButton.OK, MessageBoxImage.Warning);
		}


		private void Mentes_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog sfd = new();
			sfd.Filter = "CSV-fájl (*.csv)|*.csv";
			if (sfd.ShowDialog() != true) return;
			StreamWriter sw = new(sfd.FileName, false, Encoding.UTF8);
			try
			{
				sw.WriteLine("Rajtszam;Nev;Versenyszam;Nem;Eletkor;OlimpiaiCsucs;VBCsucs;EBCsucs");
				foreach (Versenyzo versenyzo in versenyzok)
					sw.WriteLine(versenyzo.MentesFormazas());
			}
			catch (Exception ex)
			{
				MessageBox.Show("A mentés nem sikerült: " + ex.Message, "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			sw.Close();
		}

		private void Betoltes_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog ofd = new();
			ofd.Filter = "CSV-fájl (*.csv)|*.csv";
			if (ofd.ShowDialog() != true) return;
			StreamReader sr = new(ofd.FileName, Encoding.UTF8);
			try
			{
				versenyzok.Clear();
				sr.ReadLine();
				while (!sr.EndOfStream)
				{
					Versenyzo uj = new(sr.ReadLine());
					if (versenyzok.Any(x => x.Rajtszam == uj.Rajtszam))
					{
						MessageBox.Show("A fájlban ismétlődő rajtszám található: " + uj.Rajtszam, "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}
					versenyzok.Add(uj);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("A betöltés nem sikerült: " + ex.Message, "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			sr.Close();
			txtSzamlalo.Text = $"Regisztrált versenyzők: {versenyzok.Count}";
		}
	}
}
