using Microsoft.Win32;
using System.Diagnostics;
using System.Windows;

namespace Tradutor.Views
{
    public partial class AcessibilidadeWindow : Window
    {
        public AcessibilidadeWindow()
        {
            InitializeComponent();
        }

        // ─── VISÃO ─────────────────────────────────────────────────

        private void AbrirLupa_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "🔍 A Lupa do Windows vai abrir agora.\n\n" +
                "Use os botões  +  e  −  para aumentar ou diminuir o zoom.\n\n" +
                "💡 Dica: Pressione a tecla Windows + + (mais)\n" +
                "para abrir a lupa rapidamente a qualquer momento.\n" +
                "Para fechar, pressione Windows + Esc.",
                "Lupa", MessageBoxButton.OK, MessageBoxImage.Information);

            Process.Start(new ProcessStartInfo("magnify.exe")
            { UseShellExecute = true });
        }

        private void AtivarAltoContraste_Click(object sender, RoutedEventArgs e)
        {
            var resposta = MessageBox.Show(
                "🌑 Deseja ativar o Alto Contraste?\n\n" +
                "As cores da tela vão mudar para preto e branco de alto contraste.\n" +
                "Você pode voltar ao normal clicando em 'Desativar' a qualquer momento.",
                "Alto Contraste", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (resposta == MessageBoxResult.Yes)
            {
                // Ativa o tema de alto contraste via registro
                Registry.SetValue(
                    @"HKEY_CURRENT_USER\Control Panel\Accessibility\HighContrast",
                    "Flags", "122", RegistryValueKind.String);

                Process.Start(new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = "-Command \"" +
                        "(New-Object -ComObject Shell.Application)" +
                        ".ToggleDesktop()\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                // Forma mais confiável: abre as configurações direto
                Process.Start(new ProcessStartInfo(
                    "ms-settings:easeofaccess-highcontrast")
                { UseShellExecute = true });

                MessageBox.Show(
                    "A tela de configuração do Alto Contraste abriu.\n\n" +
                    "Ative o botão 'Usar alto contraste' e escolha\n" +
                    "o tema que preferir.",
                    "Alto Contraste",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DesativarAltoContraste_Click(object sender, RoutedEventArgs e)
        {
            Registry.SetValue(
                @"HKEY_CURRENT_USER\Control Panel\Accessibility\HighContrast",
                "Flags", "122", RegistryValueKind.String);

            Process.Start(new ProcessStartInfo(
                "ms-settings:easeofaccess-highcontrast")
            { UseShellExecute = true });

            MessageBox.Show(
                "A tela de configuração abriu.\n\n" +
                "Desative o botão 'Usar alto contraste' para\n" +
                "voltar às cores normais.",
                "Alto Contraste",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AjustarTexto_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("ms-settings:easeofaccess-display")
            { UseShellExecute = true });
        }

        // ─── AUDIÇÃO ───────────────────────────────────────────────

        private void AbrirLegendas_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "💬 As configurações de Legendas vão abrir agora.\n\n" +
                "Procure por 'Legendas ao vivo' e ative o botão.\n\n" +
                "Depois disso, qualquer som no seu computador\n" +
                "aparecerá como texto na tela em tempo real.",
                "Legendas Automáticas",
                MessageBoxButton.OK, MessageBoxImage.Information);

            Process.Start(new ProcessStartInfo(
                "ms-settings:easeofaccess-closedcaptioning")
            { UseShellExecute = true });
        }

        private void AbrirAlertasVisuais_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "⚡ As configurações de Alertas Visuais vão abrir agora.\n\n" +
                "Procure por 'Alertas de som' e escolha se quer que\n" +
                "a tela pisque no lugar de emitir sons de aviso.",
                "Alertas Visuais",
                MessageBoxButton.OK, MessageBoxImage.Information);

            Process.Start(new ProcessStartInfo(
                "ms-settings:easeofaccess-audio")
            { UseShellExecute = true });
        }

        // ─── MOBILIDADE ────────────────────────────────────────────

        private void AbrirTecladoTela_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "⌨️ O Teclado na Tela vai abrir agora.\n\n" +
                "Clique nas teclas com o mouse para digitar.\n\n" +
                "💡 Para fechar, clique no X no canto do teclado.",
                "Teclado na Tela",
                MessageBoxButton.OK, MessageBoxImage.Information);

            Process.Start(new ProcessStartInfo("osk.exe")
            { UseShellExecute = true });
        }

        private void AbrirTeclasAderencia_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "📌 As configurações de Teclas de Aderência vão abrir agora.\n\n" +
                "Ative a opção 'Teclas de aderência' para poder usar\n" +
                "atalhos como Ctrl+Alt+Del pressionando uma tecla por vez,\n" +
                "sem precisar segurar todas ao mesmo tempo.",
                "Teclas de Aderência",
                MessageBoxButton.OK, MessageBoxImage.Information);

            Process.Start(new ProcessStartInfo(
                "ms-settings:easeofaccess-keyboard")
            { UseShellExecute = true });
        }

        private void AtivarNarrador_Click(object sender, RoutedEventArgs e)
        {
            var resposta = MessageBox.Show(
                "🔈 Deseja ativar o Narrador?\n\n" +
                "O computador começará a ler em voz alta o que\n" +
                "está na tela — textos, botões e menus.\n\n" +
                "Para parar o Narrador a qualquer momento,\n" +
                "pressione Windows + Enter.",
                "Narrador", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (resposta == MessageBoxResult.Yes)
            {
                Process.Start(new ProcessStartInfo("narrator.exe")
                { UseShellExecute = true });
            }
        }

        private void DesativarNarrador_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = "-Command \"Stop-Process -Name Narrator -ErrorAction SilentlyContinue\"",
                UseShellExecute = false,
                CreateNoWindow = true
            });

            MessageBox.Show(
                "🔇 Narrador desativado!\n\n" +
                "O computador não lerá mais o conteúdo da tela em voz alta.",
                "Narrador", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ─── CENTRAL ───────────────────────────────────────────────

        private void AbrirCentralAcessibilidade_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("ms-settings:easeofaccess")
            { UseShellExecute = true });
        }
    }
}