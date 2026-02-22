using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace Tradutor.Views
{
    public partial class EnergiaWindow : Window
    {
        // API do Windows para impedir suspensão
        [DllImport("kernel32.dll")]
        private static extern uint SetThreadExecutionState(uint esFlags);

        private const uint ES_CONTINUOUS = 0x80000000;
        private const uint ES_SYSTEM_REQUIRED = 0x00000001;
        private const uint ES_DISPLAY_REQUIRED = 0x00000002;

        private bool _mantendoAcordado = false;

        public EnergiaWindow()
        {
            InitializeComponent();
            CarregarPlanoAtual();
        }

        // ─── PLANO ATUAL ───────────────────────────────────────────

        private void CarregarPlanoAtual()
        {
            try
            {
                var processo = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powercfg",
                        Arguments = "/getactivescheme",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                processo.Start();
                string saida = processo.StandardOutput.ReadToEnd();
                processo.WaitForExit();

                // A saída vem assim: "Esquema de Energia GUID: xxxx-xxxx  (Nome do Plano)"
                // Extraímos o nome que fica entre parênteses
                string nome = "Desconhecido";
                int inicio = saida.IndexOf('(');
                int fim = saida.IndexOf(')');
                if (inicio >= 0 && fim > inicio)
                    nome = saida.Substring(inicio + 1, fim - inicio - 1).Trim();

                // Traduz nomes em inglês para português
                nome = nome switch
                {
                    "Balanced" => "Balanceado ⚖️",
                    "Power saver" => "Econômico 🌿",
                    "High performance" => "Alto Desempenho 🚀",
                    "Ultimate Performance" => "Desempenho Máximo 🏆",
                    _ => nome
                };

                LblPlanoAtual.Text = nome;
            }
            catch
            {
                LblPlanoAtual.Text = "Não foi possível identificar";
            }
        }

        // ─── PLANOS DE ENERGIA ─────────────────────────────────────

        private void AtivarEconomico_Click(object sender, RoutedEventArgs e)
        {
            ExecutarPowercfg("/setactive SCHEME_MAX");
            LblPlanoAtual.Text = "Econômico 🌿";
            MessageBox.Show(
                "🌿 Plano Econômico ativado!\n\n" +
                "Seu computador agora vai economizar mais energia.\n" +
                "Ideal para deixar ligado por longos períodos.",
                "Plano de Energia", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AtivarBalanceado_Click(object sender, RoutedEventArgs e)
        {
            ExecutarPowercfg("/setactive SCHEME_BALANCED");
            LblPlanoAtual.Text = "Balanceado ⚖️";
            MessageBox.Show(
                "⚖️ Plano Balanceado ativado!\n\n" +
                "Este é o plano recomendado para a maioria das pessoas.\n" +
                "Bom desempenho sem desperdiçar energia.",
                "Plano de Energia", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AtivarAltoDesempenho_Click(object sender, RoutedEventArgs e)
        {
            var resposta = MessageBox.Show(
                "🚀 Deseja ativar o Alto Desempenho?\n\n" +
                "Seu computador ficará mais rápido, mas vai consumir\n" +
                "mais energia e pode esquentar mais.\n\n" +
                "Recomendado apenas quando precisar de máxima velocidade.",
                "Alto Desempenho", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (resposta == MessageBoxResult.Yes)
            {
                ExecutarPowercfg("/setactive SCHEME_MIN");
                LblPlanoAtual.Text = "Alto Desempenho 🚀";
                MessageBox.Show(
                    "🚀 Plano de Alto Desempenho ativado!",
                    "Plano de Energia", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExecutarPowercfg(string argumentos)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "powercfg",
                Arguments = argumentos,
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }

        // ─── TELA E SUSPENSÃO ──────────────────────────────────────

        private void AjustarTela_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("ms-settings:powersleep")
            { UseShellExecute = true });
        }

        private void AjustarSuspender_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("ms-settings:powersleep")
            { UseShellExecute = true });
        }

        private void ConfigurarBotao_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("control",
                "/name Microsoft.PowerOptions /page pageGlobalSettings")
            { UseShellExecute = true });
        }

        // ─── MANTER ACORDADO ───────────────────────────────────────

        private void ManterAcordado_Click(object sender, RoutedEventArgs e)
        {
            if (_mantendoAcordado)
            {
                MessageBox.Show(
                    "☕ O computador já está sendo mantido acordado!\n\n" +
                    "Clique em 'Liberar' quando sua tarefa terminar.",
                    "Já ativo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Impede que o sistema e a tela durmam
            SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED);
            _mantendoAcordado = true;

            LblStatusAcordado.Text = "☕ Computador sendo mantido acordado!";
            LblStatusAcordado.Foreground =
                new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(30, 100, 50));

            MessageBox.Show(
                "☕ Pronto! O computador não vai dormir enquanto esta janela estiver aberta.\n\n" +
                "Quando sua tarefa terminar, clique em 'Liberar' para\n" +
                "tudo voltar ao normal.",
                "Mantendo Acordado", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LiberarSono_Click(object sender, RoutedEventArgs e)
        {
            // Libera o bloqueio de suspensão
            SetThreadExecutionState(ES_CONTINUOUS);
            _mantendoAcordado = false;

            LblStatusAcordado.Text = "😴 O computador pode suspender normalmente";
            LblStatusAcordado.Foreground =
                new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(100, 100, 100));

            MessageBox.Show(
                "😴 Liberado! O computador voltará a suspender normalmente\n" +
                "quando ficar sem uso.",
                "Liberado", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Garante que o bloqueio é liberado ao fechar a janela
        protected override void OnClosed(EventArgs e)
        {
            if (_mantendoAcordado)
                SetThreadExecutionState(ES_CONTINUOUS);

            base.OnClosed(e);
        }
    }
}