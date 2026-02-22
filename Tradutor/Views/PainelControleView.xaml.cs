using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Tradutor.Views
{
    public partial class PainelControleView : UserControl
    {
        public PainelControleView()
        {
            InitializeComponent();
        }

        private void AbrirNomePC_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("SystemPropertiesComputerName.exe");
        }

        private void AbrirDesempenho_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("ms-settings:about") { UseShellExecute = true });
        }

        private void AbrirDataHora_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("ms-settings:dateandtime") { UseShellExecute = true });
        }

        private void AbrirProgramas_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("ms-settings:appsfeatures") { UseShellExecute = true });
        }

        private void AbrirAtualizacoes_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("ms-settings:windowsupdate") { UseShellExecute = true });
        }

        private void AbrirRede_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("ms-settings:network") { UseShellExecute = true });
        }
        private void AbrirAparencia_Click(object sender, RoutedEventArgs e)
        {
            var janela = new AparenciaWindow();
            janela.ShowDialog();
        }


    }
}