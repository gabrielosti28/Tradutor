using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Tradutor.Views
{
    public partial class ArquivosGrandesWindow : Window
    {
        public class ArquivoInfo
        {
            public string Icone { get; set; } = "";
            public string Nome { get; set; } = "";
            public string Tamanho { get; set; } = "";
            public string Pasta { get; set; } = "";
            public string CaminhoCompleto { get; set; } = "";
            public long TamanhoBytes { get; set; }
        }

        public ArquivosGrandesWindow()
        {
            InitializeComponent();
        }

        private async void ProcurarArquivos_Click(object sender, RoutedEventArgs e)
        {
            LblStatus.Text = "🔍 Procurando arquivos grandes... Aguarde um momento.";
            BarraProgresso.Visibility = Visibility.Visible;
            BarraProgresso.IsIndeterminate = true;
            ListaArquivos.ItemsSource = null;

            var arquivos = await Task.Run(() => BuscarArquivosGrandes());

            BarraProgresso.IsIndeterminate = false;
            BarraProgresso.Visibility = Visibility.Collapsed;

            if (arquivos.Count == 0)
            {
                LblStatus.Text = "✅ Nenhum arquivo grande encontrado.";
                return;
            }

            ListaArquivos.ItemsSource = arquivos;
            LblStatus.Text = $"✅ Encontrados {arquivos.Count} arquivos grandes. " +
                             $"Clique em um para selecioná-lo.";
        }

        private List<ArquivoInfo> BuscarArquivosGrandes()
        {
            var resultado = new List<ArquivoInfo>();

            // Pastas comuns que o usuário conhece — sem pastas técnicas do sistema
            var pastasBusca = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
                Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.UserProfile), "Downloads"),
                Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.UserProfile), "Desktop")
            };

            long limiteMinimo = 50 * 1024 * 1024; // Arquivos acima de 50 MB

            foreach (var pasta in pastasBusca)
            {
                if (!Directory.Exists(pasta)) continue;

                try
                {
                    var arquivos = Directory.GetFiles(pasta, "*.*",
                        SearchOption.AllDirectories);

                    foreach (var caminho in arquivos)
                    {
                        try
                        {
                            var info = new FileInfo(caminho);
                            if (info.Length < limiteMinimo) continue;

                            resultado.Add(new ArquivoInfo
                            {
                                Icone = ObterIcone(info.Extension),
                                Nome = info.Name,
                                Tamanho = FormatarTamanho(info.Length),
                                Pasta = TraduzirPasta(info.DirectoryName ?? ""),
                                CaminhoCompleto = caminho,
                                TamanhoBytes = info.Length
                            });
                        }
                        catch { }
                    }
                }
                catch { }
            }

            // Ordena do maior para o menor
            return resultado.OrderByDescending(a => a.TamanhoBytes).Take(50).ToList();
        }

        private string ObterIcone(string extensao) => extensao.ToLower() switch
        {
            ".mp4" or ".avi" or ".mkv" or ".mov" => "🎬",
            ".mp3" or ".wav" or ".flac" => "🎵",
            ".jpg" or ".jpeg" or ".png" or ".bmp" => "🖼️",
            ".pdf" => "📄",
            ".zip" or ".rar" or ".7z" => "📦",
            ".exe" or ".msi" => "⚙️",
            ".iso" => "💿",
            ".doc" or ".docx" => "📝",
            _ => "📁"
        };

        private string TraduzirPasta(string caminho)
        {
            // Substitui caminhos técnicos por nomes amigáveis
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return caminho
                .Replace(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "📄 Documentos")
                .Replace(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "🎬 Vídeos")
                .Replace(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "🎵 Músicas")
                .Replace(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "🖼️ Imagens")
                .Replace(Path.Combine(userProfile, "Downloads"), "⬇️ Downloads")
                .Replace(Path.Combine(userProfile, "Desktop"), "🖥️ Área de Trabalho");
        }

        private string FormatarTamanho(long bytes) => bytes switch
        {
            < 1024 * 1024 => $"{bytes / 1024} KB",
            < 1024 * 1024 * 1024 => $"{bytes / (1024 * 1024)} MB",
            _ => $"{bytes / (1024.0 * 1024 * 1024):F1} GB"
        };

        private void AbrirArquivo_Click(object sender, RoutedEventArgs e)
        {
            if (ListaArquivos.SelectedItem is not ArquivoInfo arquivo)
            {
                MessageBox.Show("Por favor, clique em um arquivo da lista para selecioná-lo.",
                    "Nenhum arquivo selecionado", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Abre o explorador com o arquivo destacado
            Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{arquivo.CaminhoCompleto}\"")
            { UseShellExecute = true });
        }

        private void ApagarArquivo_Click(object sender, RoutedEventArgs e)
        {
            if (ListaArquivos.SelectedItem is not ArquivoInfo arquivo)
            {
                MessageBox.Show("Por favor, clique em um arquivo da lista para selecioná-lo.",
                    "Nenhum arquivo selecionado", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var resposta = MessageBox.Show(
                $"⚠️ Tem certeza que deseja apagar este arquivo?\n\n" +
                $"📄 {arquivo.Nome}\n" +
                $"📦 Tamanho: {arquivo.Tamanho}\n\n" +
                $"O arquivo irá para a Lixeira.\n" +
                $"Você pode recuperá-lo de lá se mudar de ideia.",
                "Apagar Arquivo",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (resposta == MessageBoxResult.Yes)
            {
                try
                {
                    // Envia para a lixeira em vez de apagar permanentemente
                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(
                        arquivo.CaminhoCompleto,
                        Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                        Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);

                    MessageBox.Show(
                        $"✅ Arquivo enviado para a Lixeira!\n\n" +
                        $"Se precisar recuperá-lo, abra a Lixeira na Área de Trabalho.",
                        "Arquivo Apagado", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Atualiza a lista
                    ProcurarArquivos_Click(sender, e);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Não foi possível apagar o arquivo.\n\nMotivo: {ex.Message}",
                        "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}