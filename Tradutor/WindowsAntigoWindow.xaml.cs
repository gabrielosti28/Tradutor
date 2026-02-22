using System.Diagnostics;
using System.Windows;

namespace Tradutor
{
    public partial class WindowsAntigoWindow : Window
    {
        public WindowsAntigoWindow()
        {
            InitializeComponent();
            MostrarVersaoAtual();
        }

        private void MostrarVersaoAtual()
        {
            var versao = Environment.OSVersion.Version;
            string nomeVersao = versao.Major switch
            {
                6 when versao.Minor == 1 => "Windows 7",
                6 when versao.Minor == 2 => "Windows 8",
                6 when versao.Minor == 3 => "Windows 8.1",
                _ => $"Windows (versão {versao.Major}.{versao.Minor})"
            };

            LblVersaoAtual.Text = $"Versão detectada no seu computador: {nomeVersao}";
        }

        private void AtualizarWindows10_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MessageBox.Show(
                "Vou abrir o site oficial da Microsoft para você.\n\n" +
                "Lá você encontrará o botão 'Baixar ferramenta agora'.\n" +
                "Essa ferramenta vai guiar você pela atualização passo a passo.\n\n" +
                "💡 Dica: Antes de atualizar, salve seus arquivos importantes\n" +
                "em um pen drive ou HD externo por precaução.",
                "Atualizar para Windows 10",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            Process.Start(new ProcessStartInfo(
                "https://www.microsoft.com/pt-br/software-download/windows10")
            { UseShellExecute = true });
        }

        private void AtualizarWindows11_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MessageBox.Show(
                "Vou abrir o site oficial da Microsoft para você.\n\n" +
                "Atenção: o Windows 11 tem requisitos mínimos de hardware.\n" +
                "Se não tiver certeza se seu computador é compatível,\n" +
                "use a opção 'Verificar compatibilidade' antes.\n\n" +
                "💡 Dica: Antes de atualizar, salve seus arquivos importantes\n" +
                "em um pen drive ou HD externo por precaução.",
                "Atualizar para Windows 11",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            Process.Start(new ProcessStartInfo(
                "https://www.microsoft.com/pt-br/software-download/windows11")
            { UseShellExecute = true });
        }

        private void VerificarCompatibilidade_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MessageBox.Show(
                "Vou abrir a ferramenta oficial da Microsoft chamada\n" +
                "'PC Health Check' (Verificação de Integridade do PC).\n\n" +
                "Ela analisa seu computador e diz em segundos se ele\n" +
                "é compatível com o Windows 11 — sem instalar nada.\n\n" +
                "💡 Procure pelo botão 'Verificar agora' após abrir.",
                "Verificar Compatibilidade",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            Process.Start(new ProcessStartInfo(
                "https://aka.ms/GetPCHealthCheckApp")
            { UseShellExecute = true });
        }
    }
}