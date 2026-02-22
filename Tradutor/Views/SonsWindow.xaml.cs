using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace Tradutor.Views
{
    public partial class SonsWindow : Window
    {
        // API nativa do Windows para controlar o volume
        [DllImport("winmm.dll")]
        private static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);

        public SonsWindow()
        {
            InitializeComponent();
            CarregarVolumeAtual();
        }

        private void CarregarVolumeAtual()
        {
            // Tenta ler o volume atual do sistema para iniciar o slider no valor certo
            try
            {
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powershell",
                        Arguments = "-Command \"(Get-AudioDevice -Playback).Volume\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                // Deixamos o slider no valor padrão 50 — sem dependência externa
            }
            catch { }
        }

        private void SliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (LblVolume == null) return;

            int volume = (int)SliderVolume.Value;
            LblVolume.Text = $"Volume atual: {volume}%";

            // Converte para o formato da API do Windows (0 a 65535 em cada canal)
            uint valor = (uint)(volume * 65535 / 100);
            uint volumeWindows = valor | (valor << 16);
            waveOutSetVolume(IntPtr.Zero, volumeWindows);
        }

        private void Silenciar_Click(object sender, RoutedEventArgs e)
        {
            SliderVolume.Value = 0;
            waveOutSetVolume(IntPtr.Zero, 0);
            MessageBox.Show("🔇 Computador silenciado!\n\nMova o controle de volume para ouvir novamente.",
                "Som desativado", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AbrirMixer_Click(object sender, RoutedEventArgs e)
        {
            // Abre o mixer de volume do Windows
            Process.Start(new ProcessStartInfo("sndvol.exe") { UseShellExecute = true });
        }

        private void ConfigurarNotificacoes_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("ms-settings:notifications")
            { UseShellExecute = true });
        }

        private void TrocarEsquema_Click(object sender, RoutedEventArgs e)
        {
            // Abre o painel clássico de sons do Windows
            Process.Start(new ProcessStartInfo("mmsys.cpl") { UseShellExecute = true });
        }

        private void DesativarSons_Click(object sender, RoutedEventArgs e)
        {
            var resposta = MessageBox.Show(
                "Tem certeza que deseja desativar todos os sons do Windows?\n\n" +
                "Você pode reativar a qualquer momento clicando em 'Trocar Esquema de Sons'.",
                "Desativar Sons", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (resposta == MessageBoxResult.Yes)
            {
                // Define o esquema de sons como "Sem Sons" via PowerShell
                Process.Start(new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = "-Command \"Set-ItemProperty -Path 'HKCU:\\AppEvents\\Schemes' -Name '(Default)' -Value '.None'\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                MessageBox.Show("🔕 Todos os sons do Windows foram desativados!",
                    "Sons desativados", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void GerenciarDispositivos_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("ms-settings:sound")
            { UseShellExecute = true });
        }
    }
}