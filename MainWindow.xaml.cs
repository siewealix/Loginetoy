using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Media;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace Logiciel_nettoyage
{
    public partial class MainWindow : Window
    {
        public DirectoryInfo WinTemp;
        public DirectoryInfo appTemp;
        public string version = "1.0.0";

        public MainWindow()
        {
            InitializeComponent(); // Assurez-vous d'appeler InitializeComponent()
            WinTemp = new DirectoryInfo(@"C:\Windows\Temp");
            appTemp = new DirectoryInfo(System.IO.Path.GetTempPath());
            CheckVersion();
            getDate();
        }


       public Boolean CheckVersion()
        {
            string url = "https://www.google.com";


            using (WebClient client = new WebClient())
            {
                string actu = client.DownloadString(url);
                if (actu != version)
                {
                    actuTxt.Background = new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString("#FFEE9E9E"));
                    actuTxt.Content = "Une mise à jour est disponible! ";

                }

                return (actu == version);
            }

        }

        private void Button_MAJ_Click(object sender, RoutedEventArgs e)
        {
            Boolean est_a_jour = CheckVersion();

            if (est_a_jour)
            {
                MessageBox.Show("Votre logiciel est à jour !", "Mise à jour", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            else
            {
                MessageBox.Show("Une mise à jour est disponible!", "Mise à jour", MessageBoxButton.OK, MessageBoxImage.Information);
            }

        }

        private void Button_Histo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("À Faire !", "Historique", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Button_Web_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("https://www.sietech.com")
                {
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Navigation", MessageBoxButton.OKCancel, MessageBoxImage.Error);
            }
        }

        private void Button_Analyser_Click(object sender, RoutedEventArgs e)
        {
            AnalyseFolder(); // Appel de la méthode d'analyse
        }

        public void AnalyseFolder()
        {
            Console.WriteLine("Début de l'analyse....");
            long totalSize = 0;

            try { 
            totalSize += DirSize(WinTemp) / 1000000;
            totalSize += DirSize(appTemp) / 1000000;
            } catch (Exception ex)
            {
                Console.WriteLine("Erreur: " + ex.Message);
            }

            espace.Content = totalSize + "Mb";
            datetxt.Content = DateTime.Today;
            titre.Content = "Analyse du PC terminée";
        }

        // Méthode pour calculer la taille d'un dossier
        public long DirSize(DirectoryInfo dir)
        {
            return dir.GetFiles().Sum(fi => fi.Length) + dir.GetDirectories().Sum(di => DirSize(di));
        }

        // Méthode pour vider un dossier
        public void ClearTempData(DirectoryInfo di)
        {
            foreach (FileInfo file in di.GetFiles())
            {
                try
                {
                    file.Delete();
                    Console.WriteLine(file.FullName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erreur: " + ex.Message);
                }
            }

            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                try
                {
                    dir.Delete(true);
                    Console.WriteLine(dir.FullName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erreur: " + ex.Message);
                }
            }
        }

        private void Button_Nettoyer_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Nettoyage en cours...");
            btnclean.Content = "NETTOYAGE EN COURS";
            Clipboard.Clear();

            try {
                ClearTempData(WinTemp);

            }
            catch (Exception ex) { 
                Console.WriteLine("Erreur: " + ex.Message);
            }

            try
            {
                ClearTempData(appTemp); 
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur: " + ex.Message);
            }

            imgclean.Visibility = Visibility.Collapsed;
            btnclean.Content = "NETTOYAGE TERMINE";
            espace.Content = "0 Mb";
        }

        public void getDate()
        {
            string date = DateTime.Today.ToString();
            try {
                File.WriteAllText("date.txt", date);
            }catch (Exception ex)
            {
                Console.WriteLine("Erreur: " + ex.Message);
            }
            try
            {
                datetxt.Content = File.ReadAllText("date.txt");
            }catch(Exception ex)
            {
                Console.WriteLine("Erreur: " + ex.Message);
            }
            

        }
    }
}
