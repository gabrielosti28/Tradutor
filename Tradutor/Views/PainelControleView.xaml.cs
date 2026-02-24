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
            Process.Start(new ProcessStartInfo("SystemPropertiesComputerName.exe")
            { UseShellExecute = true });
        }

        private void AbrirDesempenho_Click(object sender, RoutedEventArgs e)
        {
            var janela = new PecasMaquinaWindow();
            janela.ShowDialog();
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
        private void AbrirSons_Click(object sender, RoutedEventArgs e)
        {
            var janela = new SonsWindow();
            janela.ShowDialog();
        }
        private void AbrirMouseTeclado_Click(object sender, RoutedEventArgs e)
        {
            var janela = new MouseTecladoWindow();
            janela.ShowDialog();
        }
        private void AbrirEnergia_Click(object sender, RoutedEventArgs e)
        {
            var janela = new EnergiaWindow();
            janela.ShowDialog();
        }
        private void AbrirArmazenamento_Click(object sender, RoutedEventArgs e)
        {
            var janela = new ArmazenamentoWindow();
            janela.ShowDialog();
        }
        private void AbrirSeguranca_Click(object sender, RoutedEventArgs e)
        {
            var janela = new SegurancaWindow();
            janela.ShowDialog();
        }
        private void AbrirInicializacao_Click(object sender, RoutedEventArgs e)
        {
            var janela = new InicializacaoWindow();
            janela.ShowDialog();
        }
        private void AbrirAcessibilidade_Click(object sender, RoutedEventArgs e)
        {
            var janela = new AcessibilidadeWindow();
            janela.ShowDialog();
        }


    }
}