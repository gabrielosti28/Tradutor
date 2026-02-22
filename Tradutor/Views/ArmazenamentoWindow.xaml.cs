using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace Tradutor.Views
{
    public partial class ArmazenamentoWindow : Window
    {
        // API do Windows para calcular tamanho da lixeira
        [StructLayout(LayoutKind.Sequential)]
        private struct SHQUERYRBINFO
        {
            public int cbSize;
            public long i64Size;
            public long i64NumItems;
        }

        [DllImport("shell32.dll")]
        private static extern int SHQueryRecycleBin(string pszRootPath,
            ref SHQUERYRBINFO pSHQueryRBInfo);

        public ArmazenamentoWindow()
        {
            InitializeComponent();
            CarregarDiscos();
            CarregarTamanhoLixeira();
        }

        // ─── MODELO DO DISCO ───────────────────────────────────────

        public class DiscoInfo
        {
            public string Icone { get; set; } = "💽";
            public string Nome { get; set; } = "";
            public string Resumo { get; set; } = "";
            public double PorcentagemUsada { get; set; }
            public string PorcentagemTexto { get; set; } = "";
            public Brush CorBarra { get; set; } = Brushes.SteelBlue;
            public string Aviso { get; set; } = "";
            public Visibility AvisoVisivel { get; set; } = Visibility.Collapsed;
        }

        // ─── CARREGAR DISCOS ───────────────────────────────────────

        private void CarregarDiscos()
        {
            var discos = new List<DiscoInfo>();

            foreach (var drive in DriveInfo.GetDrives())
            {
                if (!drive.IsReady) continue;

                long totalGB = drive.TotalSize / (1024 * 1024 * 1024);
                long livreGB = drive.AvailableFreeSpace / (1024 * 1024 * 1024);
                long usadoGB = totalGB - livreGB;
                double porcentagem = totalGB > 0
                    ? Math.Round((double)usadoGB / totalGB * 100, 1)
                    : 0;

                string tipo = drive.DriveType switch
                {
                    DriveType.Fixed => "💽",
                    DriveType.Removable => "📦",
                    DriveType.CDRom => "💿",
                    DriveType.Network => "🌐",
                    _ => "💾"
                };

                string nomeDisco = string.IsNullOrWhiteSpace(drive.VolumeLabel)
                    ? $"Disco {drive.Name.Replace("\\", "")}"
                    : $"{drive.VolumeLabel} ({drive.Name.Replace("\\", "")})";

                Brush cor = porcentagem < 70
                    ? new SolidColorBrush(Color.FromRgb(46, 139, 87))
                    : porcentagem < 90
                        ? new SolidColorBrush(Color.FromRgb(210, 140, 0))
                        : new SolidColorBrush(Color.FromRgb(180, 40, 40));

                string aviso = "";
                Visibility avisoVisivel = Visibility.Collapsed;
                if (porcentagem >= 90)
                {
                    aviso = "⚠️ Atenção! Este disco está quase cheio. Considere apagar arquivos desnecessários.";
                    avisoVisivel = Visibility.Visible;
                }
                else if (porcentagem >= 70)
                {
                    aviso = "💡 Este disco está ficando cheio. Fique de olho!";
                    avisoVisivel = Visibility.Visible;
                }

                discos.Add(new DiscoInfo
                {
                    Icone = tipo,
                    Nome = nomeDisco,
                    Resumo = $"{livreGB} GB livres de {totalGB} GB no total",
                    PorcentagemUsada = porcentagem,
                    PorcentagemTexto = $"{porcentagem}% usado",
                    CorBarra = cor,
                    Aviso = aviso,
                    AvisoVisivel = avisoVisivel
                });
            }

            ListaDiscos.ItemsSource = discos;
        }

        // ─── TAMANHO DA LIXEIRA ────────────────────────────────────

        private void CarregarTamanhoLixeira()
        {
            try
            {
                var info = new SHQUERYRBINFO();
                info.cbSize = Marshal.SizeOf(typeof(SHQUERYRBINFO));
                SHQueryRecycleBin(null!, ref info);

                long bytes = info.i64Size;
                string tamanho = bytes switch
                {
                    < 1024 => $"{bytes} bytes",
                    < 1024 * 1024 => $"{bytes / 1024} KB",
                    < 1024 * 1024 * 1024 => $"{bytes / (1024 * 1024)} MB",
                    _ => $"{bytes / (1024 * 1024 * 1024):F1} GB"
                };

                long itens = info.i64NumItems;
                string descricao = itens == 0
                    ? "Lixeira vazia ✅"
                    : $"{tamanho} ocupados — {itens} arquivo(s) na lixeira";

                LblTamanhoLixeira.Text = descricao;
                LblTamanhoLixeira.Foreground = itens == 0
                    ? new SolidColorBrush(Color.FromRgb(46, 139, 87))
                    : new SolidColorBrush(Color.FromRgb(30, 58, 95));
            }
            catch
            {
                LblTamanhoLixeira.Text = "Não foi possível calcular";
            }
        }

        // ─── LIMPEZA ───────────────────────────────────────────────

        private void LimpezaDisco_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "🧹 A ferramenta de Limpeza de Disco vai abrir agora.\n\n" +
                "Marque as caixinhas dos tipos de arquivo que quer apagar\n" +
                "e clique em OK. Não se preocupe — seus documentos,\n" +
                "fotos e músicas não serão apagados!",
                "Limpeza de Disco", MessageBoxButton.OK, MessageBoxImage.Information);

            Process.Start(new ProcessStartInfo("cleanmgr.exe")
            { UseShellExecute = true });
        }

        private void StorageSense_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("ms-settings:storagesense")
            { UseShellExecute = true });
        }

        private void EsvaziarLixeira_Click(object sender, RoutedEventArgs e)
        {
            var resposta = MessageBox.Show(
                "🗑️ Tem certeza que deseja esvaziar a Lixeira?\n\n" +
                "Os arquivos serão apagados permanentemente e\n" +
                "não poderão ser recuperados depois.\n\n" +
                "Só confirme se tiver certeza que não precisa\n" +
                "de nenhum arquivo que foi deletado recentemente.",
                "Esvaziar Lixeira",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (resposta == MessageBoxResult.Yes)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = "-Command \"Clear-RecycleBin -Force -ErrorAction SilentlyContinue\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                })!.WaitForExit();

                MessageBox.Show(
                    "✅ Lixeira esvaziada com sucesso!",
                    "Lixeira", MessageBoxButton.OK, MessageBoxImage.Information);

                // Atualiza tudo para refletir o espaço liberado
                CarregarDiscos();
                CarregarTamanhoLixeira();
            }
        }

        private void AbrirLixeira_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("explorer.exe", "shell:RecycleBinFolder")
            { UseShellExecute = true });
        }

        private void DesfragmentarDisco_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "🔧 A ferramenta de Otimização de Disco vai abrir agora.\n\n" +
                "Selecione o disco que quer organizar e clique em\n" +
                "'Otimizar'. O processo pode demorar alguns minutos.\n\n" +
                "💡 Dica: Se aparecer 'Unidade de Estado Sólido' ou 'SSD',\n" +
                "não é necessário desfragmentar — o Windows já cuida disso!",
                "Organizar Disco", MessageBoxButton.OK, MessageBoxImage.Information);

            Process.Start(new ProcessStartInfo("dfrgui.exe")
            { UseShellExecute = true });
        }

        // ─── ARQUIVOS GRANDES ──────────────────────────────────────

        private async void EncontrarArquivosGrandes_Click(object sender, RoutedEventArgs e)
        {
            var janela = new ArquivosGrandesWindow();
            janela.ShowDialog();
        }
    }
}