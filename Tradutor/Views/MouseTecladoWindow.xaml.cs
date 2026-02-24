using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace Tradutor.Views
{
    public partial class MouseTecladoWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam,
            IntPtr pvParam, uint fWinIni);

        private const uint SPI_SETMOUSESPEED = 0x0071;
        private const uint SPIF_UPDATEINIFILE = 0x01;
        private const uint SPIF_SENDCHANGE = 0x02;

        public MouseTecladoWindow()
        {
            InitializeComponent();
        }

        // ─── VELOCIDADE DO MOUSE ───────────────────────────────────

        private void VelocidadeLenta_Click(object sender, RoutedEventArgs e)
        {
            AplicarVelocidade(4, "Lento 🐢");
        }

        private void VelocidadeMedia_Click(object sender, RoutedEventArgs e)
        {
            AplicarVelocidade(10, "Médio 👆");
        }

        private void VelocidadeRapida_Click(object sender, RoutedEventArgs e)
        {
            AplicarVelocidade(16, "Rápido 🐇");
        }

        private void AplicarVelocidade(int valor, string descricao)
        {
            try
            {
                // Salva no registro
                Registry.SetValue(
                    @"HKEY_CURRENT_USER\Control Panel\Mouse",
                    "MouseSensitivity", valor.ToString());

                // Aplica via API nativa
                SystemParametersInfo(SPI_SETMOUSESPEED, 0,
                    (IntPtr)valor, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);

                LblMouse.Text = $"Velocidade atual: {descricao}";

                MessageBox.Show(
                    $"✅ Velocidade do mouse alterada para: {descricao}\n\n" +
                    "Mova o mouse agora para sentir a diferença!",
                    "Velocidade do Mouse",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show(
                    "Não foi possível alterar a velocidade do mouse.\n" +
                    "Tente usar 'Mais opções de Mouse' abaixo.",
                    "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // ─── DEMAIS FUNÇÕES ────────────────────────────────────────

        private void MaisOpcoesMouse_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("main.cpl")
            { UseShellExecute = true });
        }

        private void BotaoEsquerdo_Click(object sender, RoutedEventArgs e)
        {
            Registry.SetValue(
                @"HKEY_CURRENT_USER\Control Panel\Mouse",
                "SwapMouseButtons", "0");

            SystemParametersInfo(0x0021, 0, (IntPtr)0,
                SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);

            MessageBox.Show("✅ Botão esquerdo definido como principal!",
                "Mouse", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BotaoDireito_Click(object sender, RoutedEventArgs e)
        {
            var resposta = MessageBox.Show(
                "Isso vai trocar os botões do mouse.\n\n" +
                "Para canhoto: o botão DIREITO passará a ser o principal.\n\n" +
                "Deseja continuar?",
                "Trocar Botões do Mouse", MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resposta == MessageBoxResult.Yes)
            {
                Registry.SetValue(
                    @"HKEY_CURRENT_USER\Control Panel\Mouse",
                    "SwapMouseButtons", "1");

                SystemParametersInfo(0x0021, 1, (IntPtr)1,
                    SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);

                MessageBox.Show(
                    "✅ Botão direito definido como principal!\n\n" +
                    "Para voltar ao normal, clique em 'Botão Esquerdo (padrão)'.",
                    "Mouse", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AjustarTeclado_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("control", "keyboard")
            { UseShellExecute = true });
        }

        private void ConfigurarIdioma_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("ms-settings:regionlanguage")
            { UseShellExecute = true });
        }

        private void AjustarCursor_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("control", "main.cpl,,2")
            { UseShellExecute = true });
        }
    }
}