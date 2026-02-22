using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace Tradutor.Views
{
    public partial class MouseTecladoWindow : Window
    {
        // API nativa do Windows para definir velocidade do mouse
        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam,
            ref int pvParam, uint fWinIni);

        private const uint SPI_SETMOUSESPEED = 0x0071;
        private const uint SPI_GETMOUSESPEED = 0x0070;
        private const uint SPIF_UPDATEINIFILE = 0x01;
        private const uint SPIF_SENDCHANGE = 0x02;

        public MouseTecladoWindow()
        {
            InitializeComponent();
            CarregarVelocidadeAtual();
        }

        private void CarregarVelocidadeAtual()
        {
            int velocidade = 10;
            SystemParametersInfo(SPI_GETMOUSESPEED, 0, ref velocidade, 0);
            SliderMouse.Value = velocidade;
        }

        private void SliderMouse_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (LblMouse == null) return;

            int valor = (int)SliderMouse.Value;
            string descricao = valor <= 5 ? "Lento" : valor <= 10 ? "Médio" : valor <= 15 ? "Rápido" : "Muito Rápido";
            LblMouse.Text = $"Velocidade atual: {descricao}";

            // Aplica a velocidade em tempo real
            SystemParametersInfo(SPI_SETMOUSESPEED, 0, ref valor,
                SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
        }

        private void MaisOpcoesMouse_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("main.cpl") { UseShellExecute = true });
        }

        private void BotaoEsquerdo_Click(object sender, RoutedEventArgs e)
        {
            // 0 = botão esquerdo como principal
            Registry.SetValue(
                @"HKEY_CURRENT_USER\Control Panel\Mouse",
                "SwapMouseButtons", "0");

            // Aplica imediatamente via API
            int dummy = 0;
            SystemParametersInfo(0x0021, 0, ref dummy, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);

            MessageBox.Show("✅ Botão esquerdo definido como principal!",
                "Mouse", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BotaoDireito_Click(object sender, RoutedEventArgs e)
        {
            var resposta = MessageBox.Show(
                "Isso vai trocar os botões do mouse.\n\n" +
                "Para canhoto: o botão DIREITO passará a ser o principal (para clicar e selecionar).\n\n" +
                "Deseja continuar?",
                "Trocar Botões do Mouse", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (resposta == MessageBoxResult.Yes)
            {
                Registry.SetValue(
                    @"HKEY_CURRENT_USER\Control Panel\Mouse",
                    "SwapMouseButtons", "1");

                int dummy = 1;
                SystemParametersInfo(0x0021, 1, ref dummy, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);

                MessageBox.Show("✅ Botão direito definido como principal!\n\nPara voltar ao normal, clique em 'Botão Esquerdo (padrão)'.",
                    "Mouse", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AjustarTeclado_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("control", "keyboard") { UseShellExecute = true });
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