using NAudio.CoreAudioApi;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Tradutor.Views
{
    public partial class SonsWindow : Window
    {
        private readonly MMDeviceEnumerator _enumerador = new MMDeviceEnumerator();
        private MMDevice? _dispositivoPrincipal;
        private bool _carregando = true;

        public SonsWindow()
        {
            InitializeComponent();
            CarregarVolumeAtual();
        }

        // ─── VOLUME ────────────────────────────────────────────────

        private void CarregarVolumeAtual()
        {
            try
            {
                _dispositivoPrincipal = _enumerador.GetDefaultAudioEndpoint(
                    DataFlow.Render, Role.Multimedia);

                // Lê o volume atual do sistema (0.0 a 1.0) e converte para 0-100
                float volumeAtual = _dispositivoPrincipal.AudioEndpointVolume.MasterVolumeLevelScalar;
                SliderVolume.Value = Math.Round(volumeAtual * 100);

                bool silenciado = _dispositivoPrincipal.AudioEndpointVolume.Mute;
                if (silenciado)
                    LblVolume.Text = "🔇 Silenciado";
                else
                    LblVolume.Text = $"Volume atual: {(int)SliderVolume.Value}%";
            }
            catch
            {
                LblVolume.Text = "Não foi possível ler o volume";
            }
            finally
            {
                _carregando = false;
            }
        }

        private void SliderVolume_ValueChanged(object sender,
            RoutedPropertyChangedEventArgs<double> e)
        {
            if (_carregando || LblVolume == null) return;

            try
            {
                int volume = (int)SliderVolume.Value;
                LblVolume.Text = $"Volume atual: {volume}%";

                // Define o volume mestre real do Windows
                _dispositivoPrincipal = _enumerador.GetDefaultAudioEndpoint(
                    DataFlow.Render, Role.Multimedia);
                _dispositivoPrincipal.AudioEndpointVolume.MasterVolumeLevelScalar =
                    volume / 100f;

                // Se estava silenciado, dessilencia automaticamente
                if (_dispositivoPrincipal.AudioEndpointVolume.Mute)
                    _dispositivoPrincipal.AudioEndpointVolume.Mute = false;
            }
            catch { }
        }

        private void Silenciar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _dispositivoPrincipal = _enumerador.GetDefaultAudioEndpoint(
                    DataFlow.Render, Role.Multimedia);

                bool jaSilenciado = _dispositivoPrincipal.AudioEndpointVolume.Mute;

                if (jaSilenciado)
                {
                    // Se já está silenciado, dessilencia
                    _dispositivoPrincipal.AudioEndpointVolume.Mute = false;
                    LblVolume.Text = $"Volume atual: {(int)SliderVolume.Value}%";
                    MessageBox.Show(
                        "🔊 Som reativado!\n\nSeu computador voltará a emitir sons normalmente.",
                        "Som Reativado", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Silencia
                    _dispositivoPrincipal.AudioEndpointVolume.Mute = true;
                    LblVolume.Text = "🔇 Silenciado";
                    MessageBox.Show(
                        "🔇 Computador silenciado!\n\nClique novamente em 'Silenciar' para reativar o som.",
                        "Som Desativado", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch
            {
                MessageBox.Show(
                    "Não foi possível alterar o som.\nTente ajustar pelo controle de volume do Windows.",
                    "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // ─── DEMAIS FUNÇÕES ────────────────────────────────────────

        private void AbrirMixer_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("sndvol.exe")
            { UseShellExecute = true });
        }

        private void ConfigurarNotificacoes_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("ms-settings:notifications")
            { UseShellExecute = true });
        }

        private void TrocarEsquema_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("mmsys.cpl")
            { UseShellExecute = true });
        }

        private void DesativarSons_Click(object sender, RoutedEventArgs e)
        {
            var resposta = MessageBox.Show(
                "Tem certeza que deseja desativar todos os sons do Windows?\n\n" +
                "Você pode reativar a qualquer momento clicando em 'Trocar Esquema de Sons'.",
                "Desativar Sons", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (resposta == MessageBoxResult.Yes)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = "-Command \"Set-ItemProperty -Path " +
                                "'HKCU:\\AppEvents\\Schemes' " +
                                "-Name '(Default)' -Value '.None'\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                MessageBox.Show(
                    "🔕 Todos os sons do Windows foram desativados!\n\n" +
                    "Para reativar, clique em 'Trocar Esquema de Sons' e\n" +
                    "escolha um esquema na lista.",
                    "Sons Desativados", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void GerenciarDispositivos_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("ms-settings:sound")
            { UseShellExecute = true });
        }

        // Libera recursos ao fechar
        protected override void OnClosed(EventArgs e)
        {
            _dispositivoPrincipal?.Dispose();
            _enumerador.Dispose();
            base.OnClosed(e);
        }
    }
}