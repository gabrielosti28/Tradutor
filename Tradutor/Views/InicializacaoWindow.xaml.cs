using Microsoft.Win32;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Tradutor.Views
{
    public partial class InicializacaoWindow : Window
    {
        private List<ProgramaInicializacao> _programas = new();

        // Dicionário de descrições amigáveis para programas conhecidos
        private static readonly Dictionary<string, (string Descricao, string Impacto)>
            _descricaoConhecida = new(StringComparer.OrdinalIgnoreCase)
            {
                ["OneDrive"] = ("Armazenamento em nuvem da Microsoft", "Médio"),
                ["Spotify"] = ("Música — abre o Spotify automaticamente", "Baixo"),
                ["Discord"] = ("Chat para jogos e comunidades", "Médio"),
                ["Steam"] = ("Loja e biblioteca de jogos", "Alto"),
                ["Teams"] = ("Videochamadas e mensagens Microsoft", "Alto"),
                ["Skype"] = ("Videochamadas da Microsoft", "Médio"),
                ["Chrome"] = ("Navegador Google Chrome", "Médio"),
                ["Dropbox"] = ("Armazenamento em nuvem Dropbox", "Médio"),
                ["Zoom"] = ("Videochamadas Zoom", "Médio"),
                ["WhatsApp"] = ("Mensagens WhatsApp no computador", "Baixo"),
                ["Telegram"] = ("Mensagens Telegram no computador", "Baixo"),
                ["AdobeUpdater"] = ("Atualizações dos programas Adobe", "Baixo"),
                ["GoogleDrive"] = ("Armazenamento em nuvem Google", "Médio"),
                ["Cortana"] = ("Assistente virtual da Microsoft", "Médio"),
                ["SecurityHealth"] = ("Proteção do Windows Defender", "Baixo"),
                ["nvtray"] = ("Bandeja da placa de vídeo NVIDIA", "Baixo"),
                ["RtkAudioService"] = ("Serviço de áudio Realtek", "Baixo"),
            };

        public InicializacaoWindow()
        {
            InitializeComponent();
            Loaded += async (s, e) => await CarregarProgramasAsync();
        }

        // ─── MODELO ────────────────────────────────────────────────

        public class ProgramaInicializacao
        {
            public string IconeStatus { get; set; } = "";
            public string Nome { get; set; } = "";
            public string Descricao { get; set; } = "";
            public string Impacto { get; set; } = "";
            public bool Ativo { get; set; }
            public string ChaveRegistro { get; set; } = "";
            public string CaminhoExe { get; set; } = "";
        }

        // ─── CARREGAR ──────────────────────────────────────────────

        private async Task CarregarProgramasAsync()
        {
            var programas = await Task.Run(() => LerProgramasInicializacao());
            _programas = programas;

            BarraProgresso.Visibility = Visibility.Collapsed;
            LblStatus.Text = programas.Count == 0
                ? "✅ Nenhum programa de inicialização encontrado."
                : $"✅ {programas.Count} programa(s) encontrado(s). " +
                  $"Clique em um para selecioná-lo.";

            ListaProgramas.ItemsSource = _programas;
        }

        private List<ProgramaInicializacao> LerProgramasInicializacao()
        {
            var lista = new List<ProgramaInicializacao>();

            // Chaves do registro onde ficam os programas de inicialização
            var chaves = new[]
            {
                (Caminho: @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Run",
                 Escopo: "Usuário"),
                (Caminho: @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Run",
                 Escopo: "Sistema"),
                (Caminho: @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run",
                 Escopo: "Sistema 32bit"),
            };

            foreach (var (caminho, _) in chaves)
            {
                try
                {
                    // Abre a chave do registro correta
                    var hive = caminho.StartsWith("HKEY_CURRENT_USER")
                        ? Registry.CurrentUser
                        : Registry.LocalMachine;

                    var subCaminho = caminho.Substring(caminho.IndexOf('\\') + 1);

                    using var chave = hive.OpenSubKey(subCaminho, writable: false);
                    if (chave == null) continue;

                    foreach (var nome in chave.GetValueNames())
                    {
                        string? valorExe = chave.GetValue(nome)?.ToString();
                        if (string.IsNullOrEmpty(valorExe)) continue;

                        // Extrai nome limpo (remove caminho e extensão)
                        string nomeLimpo = LimparNome(nome);

                        // Busca descrição amigável
                        var (descricao, impacto) = BuscarDescricao(nomeLimpo, valorExe);

                        lista.Add(new ProgramaInicializacao
                        {
                            IconeStatus = "✅ Ativo",
                            Nome = nomeLimpo,
                            Descricao = descricao,
                            Impacto = impacto,
                            Ativo = true,
                            ChaveRegistro = caminho,
                            CaminhoExe = valorExe
                        });
                    }
                }
                catch { }
            }

            return lista
                .GroupBy(p => p.Nome)
                .Select(g => g.First())
                .OrderBy(p => p.Nome)
                .ToList();
        }

        private string LimparNome(string nome)
        {
            // Remove caracteres técnicos e deixa legível
            return nome
                .Replace(".exe", "")
                .Replace("_", " ")
                .Replace("-", " ")
                .Trim();
        }

        private (string Descricao, string Impacto) BuscarDescricao(
            string nome, string caminho)
        {
            // Tenta encontrar uma descrição conhecida
            foreach (var par in _descricaoConhecida)
            {
                if (nome.Contains(par.Key, StringComparison.OrdinalIgnoreCase) ||
                    caminho.Contains(par.Key, StringComparison.OrdinalIgnoreCase))
                    return par.Value;
            }

            // Tenta ler a descrição do próprio arquivo .exe
            try
            {
                string exePath = caminho.Trim('"').Split(' ')[0];
                if (System.IO.File.Exists(exePath))
                {
                    var info = FileVersionInfo.GetVersionInfo(exePath);
                    if (!string.IsNullOrEmpty(info.FileDescription))
                        return (info.FileDescription, "Desconhecido");
                }
            }
            catch { }

            return ("Programa de inicialização", "Desconhecido");
        }

        // ─── ATIVAR / DESATIVAR ────────────────────────────────────

        private void AtivarPrograma_Click(object sender, RoutedEventArgs e)
        {
            if (ListaProgramas.SelectedItem is not ProgramaInicializacao programa)
            {
                MessageBox.Show("Por favor, clique em um programa da lista para selecioná-lo.",
                    "Nenhum selecionado", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (programa.Ativo)
            {
                MessageBox.Show(
                    $"✅ '{programa.Nome}' já está ativo na inicialização!",
                    "Já ativo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var hive = programa.ChaveRegistro.StartsWith("HKEY_CURRENT_USER")
                    ? Registry.CurrentUser : Registry.LocalMachine;
                var sub = programa.ChaveRegistro.Substring(
                    programa.ChaveRegistro.IndexOf('\\') + 1);

                using var chave = hive.OpenSubKey(sub, writable: true)
                    ?? hive.CreateSubKey(sub);
                chave.SetValue(programa.Nome, programa.CaminhoExe);

                programa.Ativo = true;
                programa.IconeStatus = "✅ Ativo";
                ListaProgramas.Items.Refresh();

                MessageBox.Show(
                    $"✅ '{programa.Nome}' foi ativado!\n\n" +
                    "Ele abrirá automaticamente na próxima vez que o computador ligar.",
                    "Ativado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show(
                    "Não foi possível ativar o programa.\n" +
                    "Tente usar o Gerenciador Avançado.",
                    "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DesativarPrograma_Click(object sender, RoutedEventArgs e)
        {
            if (ListaProgramas.SelectedItem is not ProgramaInicializacao programa)
            {
                MessageBox.Show("Por favor, clique em um programa da lista para selecioná-lo.",
                    "Nenhum selecionado", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Proteção contra desativar componentes críticos do sistema
            string[] criticos = { "SecurityHealth", "WindowsDefender",
                                  "Realtek", "RtkAudio", "nvtray" };

            if (criticos.Any(c => programa.Nome.Contains(c,
                StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show(
                    $"⚠️ '{programa.Nome}' é um componente importante do sistema.\n\n" +
                    "Desativá-lo pode afetar o funcionamento do computador.\n" +
                    "Não recomendamos desativar este item.",
                    "Componente do Sistema",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var resposta = MessageBox.Show(
                $"Deseja desativar '{programa.Nome}' da inicialização?\n\n" +
                $"📋 O que faz: {programa.Descricao}\n\n" +
                "O programa NÃO será desinstalado — ele apenas não\n" +
                "abrirá mais sozinho quando o computador ligar.\n\n" +
                "Você ainda poderá abri-lo manualmente quando quiser.",
                "Desativar da Inicialização",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (resposta != MessageBoxResult.Yes) return;

            try
            {
                var hive = programa.ChaveRegistro.StartsWith("HKEY_CURRENT_USER")
                    ? Registry.CurrentUser : Registry.LocalMachine;
                var sub = programa.ChaveRegistro.Substring(
                    programa.ChaveRegistro.IndexOf('\\') + 1);

                using var chave = hive.OpenSubKey(sub, writable: true);
                chave?.DeleteValue(programa.Nome, throwOnMissingValue: false);

                _programas.Remove(programa);
                ListaProgramas.Items.Refresh();
                ListaProgramas.ItemsSource = null;
                ListaProgramas.ItemsSource = _programas;

                LblStatus.Text = $"✅ '{programa.Nome}' removido da inicialização.";

                MessageBox.Show(
                    $"✅ '{programa.Nome}' foi desativado da inicialização!\n\n" +
                    "Na próxima vez que o computador ligar, ele não abrirá\n" +
                    "sozinho — mas você ainda pode abri-lo quando quiser.",
                    "Desativado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show(
                    "Não foi possível desativar o programa.\n" +
                    "Tente usar o Gerenciador Avançado.",
                    "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AbrirGerenciador_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "⚙️ O Gerenciador de Tarefas vai abrir agora.\n\n" +
                "Clique na aba 'Inicializar' para ver todos os\n" +
                "programas que abrem com o Windows.\n\n" +
                "Para desativar um programa, clique nele com o\n" +
                "botão direito e escolha 'Desabilitar'.",
                "Gerenciador de Tarefas",
                MessageBoxButton.OK, MessageBoxImage.Information);

            Process.Start(new ProcessStartInfo("taskmgr.exe")
            { UseShellExecute = true });
        }
    }
}