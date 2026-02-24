using Microsoft.Win32;
using System.Diagnostics;
using System.Management;
using System.Windows;
using System.Windows.Media;

namespace Tradutor.Views
{
    public partial class SegurancaWindow : Window
    {
        public SegurancaWindow()
        {
            InitializeComponent();
            Loaded += async (s, e) => await VerificarSegurancaAsync();
        }

        private async Task VerificarSegurancaAsync()
        {
            var resultado = await Task.Run(() => VerificarTudo());
            AplicarResultados(resultado);
        }

        // ─── VERIFICAÇÕES ──────────────────────────────────────────

        private record ResultadoSeguranca(
            bool AntivirusAtivo,
            string AntivirusNome,
            bool FirewallAtivo,
            bool WindowsAtualizado
        );

        private ResultadoSeguranca VerificarTudo()
        {
            return new ResultadoSeguranca(
                AntivirusAtivo: VerificarAntivirus(out string nome),
                AntivirusNome: nome,
                FirewallAtivo: VerificarFirewall(),
                WindowsAtualizado: VerificarAtualizacoes()
            );
        }

        private bool VerificarAntivirus(out string nome)
        {
            nome = "Nenhum detectado";
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    @"root\SecurityCenter2",
                    "SELECT displayName, productState FROM AntiVirusProduct");

                foreach (ManagementObject obj in searcher.Get())
                {
                    nome = obj["displayName"]?.ToString() ?? "Antivírus";
                    int state = int.Parse(obj["productState"]?.ToString() ?? "0");
                    // Bit 12 indica se está ativo
                    bool ativo = ((state >> 12) & 0xF) == 1;
                    return ativo;
                }
            }
            catch { }
            return false;
        }

        private bool VerificarFirewall()
        {
            try
            {
                // Lê o estado do firewall do perfil padrão (público) via registro
                string? valor = Registry.GetValue(
                    @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile",
                    "EnableFirewall", null)?.ToString();
                return valor == "1";
            }
            catch { }
            return false;
        }

        private bool VerificarAtualizacoes()
        {
            try
            {
                // Verifica quando foi a última atualização bem-sucedida
                string? data = Registry.GetValue(
                    @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\Results\Install",
                    "LastSuccessTime", null)?.ToString();

                if (!string.IsNullOrEmpty(data) &&
                    DateTime.TryParse(data, out DateTime ultima))
                {
                    // Considera atualizado se recebeu atualização nos últimos 30 dias
                    return (DateTime.Now - ultima).TotalDays <= 30;
                }
            }
            catch { }
            return false;
        }

        // ─── APLICAR RESULTADOS NA TELA ────────────────────────────

        private void AplicarResultados(ResultadoSeguranca r)
        {
            int problemas = 0;

            // Antivírus
            if (r.AntivirusAtivo)
            {
                IconeAntivirus.Text = "✅";
                LblAntivirus.Text = $"Antivírus ativo: {r.AntivirusNome}";
                LblAntivirusDetalhe.Text = "Seu computador está sendo protegido contra vírus.";
                CartaoAntivirus.Background = new SolidColorBrush(
                    Color.FromRgb(240, 255, 244));
            }
            else
            {
                IconeAntivirus.Text = "⚠️";
                LblAntivirus.Text = "Antivírus não detectado ou desativado!";
                LblAntivirusDetalhe.Text =
                    "Recomendamos ativar o Windows Defender imediatamente.";
                CartaoAntivirus.Background = new SolidColorBrush(
                    Color.FromRgb(255, 245, 245));
                problemas++;
            }

            // Firewall
            if (r.FirewallAtivo)
            {
                IconeFirewall.Text = "✅";
                LblFirewall.Text = "Firewall ativo";
                LblFirewallDetalhe.Text =
                    "A barreira de proteção está funcionando normalmente.";
                CartaoFirewall.Background = new SolidColorBrush(
                    Color.FromRgb(240, 255, 244));
            }
            else
            {
                IconeFirewall.Text = "⚠️";
                LblFirewall.Text = "Firewall desativado!";
                LblFirewallDetalhe.Text =
                    "Seu computador está sem a barreira de proteção. Recomendamos ativar.";
                CartaoFirewall.Background = new SolidColorBrush(
                    Color.FromRgb(255, 245, 245));
                problemas++;
            }

            // Atualizações
            if (r.WindowsAtualizado)
            {
                IconeAtualizacoes.Text = "✅";
                LblAtualizacoes.Text = "Windows atualizado recentemente";
                LblAtualizacoesDetalhe.Text =
                    "Seu Windows recebeu atualizações de segurança nos últimos 30 dias.";
                CartaoAtualizacoes.Background = new SolidColorBrush(
                    Color.FromRgb(240, 255, 244));
            }
            else
            {
                IconeAtualizacoes.Text = "⚠️";
                LblAtualizacoes.Text = "Windows pode estar desatualizado";
                LblAtualizacoesDetalhe.Text =
                    "Não encontramos atualizações recentes. Recomendamos verificar agora.";
                CartaoAtualizacoes.Background = new SolidColorBrush(
                    Color.FromRgb(255, 245, 245));
                problemas++;
            }

            // Banner geral
            if (problemas == 0)
            {
                IconeStatus.Text = "✅";
                LblStatusGeral.Text = "Seu computador está protegido!";
                LblStatusDetalhe.Text =
                    "Antivírus, Firewall e Windows Update estão todos funcionando.";
                BannerStatus.Background = new SolidColorBrush(
                    Color.FromRgb(232, 245, 233));
            }
            else if (problemas == 1)
            {
                IconeStatus.Text = "⚠️";
                LblStatusGeral.Text = "Atenção: há 1 ponto de segurança para verificar";
                LblStatusDetalhe.Text =
                    "Veja abaixo o item marcado em vermelho e siga as instruções.";
                BannerStatus.Background = new SolidColorBrush(
                    Color.FromRgb(255, 248, 225));
            }
            else
            {
                IconeStatus.Text = "🚨";
                LblStatusGeral.Text =
                    $"Atenção: {problemas} pontos de segurança precisam de cuidado!";
                LblStatusDetalhe.Text =
                    "Veja abaixo os itens marcados e siga as instruções para proteger seu computador.";
                BannerStatus.Background = new SolidColorBrush(
                    Color.FromRgb(255, 235, 235));
            }
        }

        // ─── BOTÕES ────────────────────────────────────────────────

        private void AbrirDefender_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("ms-settings:windowsdefender")
            { UseShellExecute = true });
        }

        private void FazerVarredura_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "🔍 O Windows Defender vai abrir agora.\n\n" +
                "Dentro dele, clique em:\n" +
                "  'Proteção contra vírus e ameaças'\n" +
                "  → 'Opções de verificação'\n" +
                "  → 'Verificação rápida'\n\n" +
                "O processo leva alguns minutos e verifica\n" +
                "os lugares mais comuns onde vírus se escondem.",
                "Varredura de Vírus",
                MessageBoxButton.OK, MessageBoxImage.Information);

            Process.Start(new ProcessStartInfo("ms-settings:windowsdefender")
            { UseShellExecute = true });
        }

        private void AbrirFirewall_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("ms-settings:windowsdefender-firewall")
            { UseShellExecute = true });
        }

        private void AtivarFirewall_Click(object sender, RoutedEventArgs e)
        {
            var resposta = MessageBox.Show(
                "Deseja ativar o Firewall agora?\n\n" +
                "O Firewall protege seu computador bloqueando\n" +
                "tentativas de invasão pela internet.\n\n" +
                "Recomendamos fortemente mantê-lo sempre ativo.",
                "Ativar Firewall",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (resposta == MessageBoxResult.Yes)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = "advfirewall set allprofiles state on",
                    UseShellExecute = false,
                    CreateNoWindow = true
                })?.WaitForExit();

                MessageBox.Show(
                    "✅ Firewall ativado com sucesso!\n\n" +
                    "Seu computador agora tem a barreira de proteção ativa.",
                    "Firewall Ativado",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Recarrega as verificações
                Loaded -= async (s, e) => await VerificarSegurancaAsync();
                _ = VerificarSegurancaAsync();
            }
        }

        private void AbrirAtualizacoes_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("ms-settings:windowsupdate")
            { UseShellExecute = true });
        }

        private void AlterarSenha_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "🔑 A tela de contas vai abrir agora.\n\n" +
                "Para alterar sua senha:\n" +
                "  → Clique em 'Opções de entrada'\n" +
                "  → Clique em 'Senha'\n" +
                "  → Clique em 'Alterar'\n\n" +
                "💡 Uma boa senha tem letras, números e\n" +
                "pelo menos 8 caracteres.",
                "Alterar Senha",
                MessageBoxButton.OK, MessageBoxImage.Information);

            Process.Start(new ProcessStartInfo("ms-settings:signinoptions")
            { UseShellExecute = true });
        }

        private void GerenciarContas_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("ms-settings:yourinfo")
            { UseShellExecute = true });
        }
    }
}