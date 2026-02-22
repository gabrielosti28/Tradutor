using System.Management;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Tradutor.Views
{
    public partial class PecasMaquinaWindow : Window
    {
        // ─── CAMPO PARA GUARDAR AS PEÇAS CARREGADAS ────────────────
        private List<PecaInfo> _pecasCarregadas = new();

        public PecasMaquinaWindow()
        {
            InitializeComponent();
            Loaded += async (s, e) => await CarregarPecasAsync();
        }

        private async Task CarregarPecasAsync()
        {
            var pecas = await Task.Run(() => LerPecas());
            _pecasCarregadas = pecas;

            BarraProgresso.Visibility = Visibility.Collapsed;
            LblStatus.Text = "✅ Informações carregadas com sucesso!";

            foreach (var peca in pecas)
                PainelPecas.Children.Add(CriarCartaoPeca(peca));
        }

        // ─── LER PEÇAS VIA WMI ─────────────────────────────────────

        private List<PecaInfo> LerPecas()
        {
            var lista = new List<PecaInfo>();

            lista.Add(LerProcessador());
            lista.Add(LerMemoriaRAM());
            lista.Add(LerPlacaVideo());
            lista.Add(LerSistemaOperacional());
            lista.Add(LerPlacaMae());
            lista.AddRange(LerDiscos());
            lista.AddRange(LerPlacasRede());
            lista.Add(LerBateria());
            lista.Add(LerAudio());
            lista.Add(LerMonitor());

            return lista.Where(p => p.Valor != "Não foi possível ler").ToList();
        }

        private PecaInfo LerProcessador()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT Name, NumberOfCores, MaxClockSpeed FROM Win32_Processor");

                foreach (ManagementObject obj in searcher.Get())
                {
                    string nome = obj["Name"]?.ToString()?.Trim() ?? "Desconhecido";
                    string nucleos = obj["NumberOfCores"]?.ToString() ?? "?";
                    string velocidade = obj["MaxClockSpeed"] != null
                        ? $"{double.Parse(obj["MaxClockSpeed"].ToString()!) / 1000:F1} GHz"
                        : "?";

                    string temperatura = LerTemperaturaProcessador();

                    return new PecaInfo
                    {
                        Icone = "⚙️",
                        Nome = "Processador (CPU)",
                        Valor = nome,
                        Explicacao = "O processador é o cérebro do computador. " +
                                     "Ele executa todas as tarefas e cálculos.",
                        Detalhe = $"Núcleos: {nucleos}  •  Velocidade: {velocidade}" +
                                  (temperatura != "" ? $"  •  Temperatura: {temperatura}" : ""),
                        Dica = AvaliarTemperatura(temperatura) ??
                               (nucleos != "?" && int.Parse(nucleos) >= 4
                                   ? "✅ Bom para uso do dia a dia"
                                   : "💡 Para tarefas simples como navegar e editar documentos")
                    };
                }
            }
            catch { }

            return new PecaInfo
            {
                Icone = "⚙️",
                Nome = "Processador (CPU)",
                Valor = "Não foi possível ler",
                Explicacao = "O processador é o cérebro do computador."
            };
        }

        private string LerTemperaturaProcessador()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    @"root\WMI",
                    "SELECT CurrentTemperature FROM MSAcpi_ThermalZoneTemperature");

                foreach (ManagementObject obj in searcher.Get())
                {
                    double kelvin = double.Parse(obj["CurrentTemperature"].ToString()!);
                    double celsius = (kelvin - 2732) / 10.0;

                    if (celsius > 0 && celsius < 120)
                        return $"{celsius:F0}°C";
                }
            }
            catch { }

            return "";
        }

        private string? AvaliarTemperatura(string temperatura)
        {
            if (string.IsNullOrEmpty(temperatura)) return null;

            if (double.TryParse(temperatura.Replace("°C", "").Trim(), out double temp))
            {
                return temp switch
                {
                    <= 50 => "✅ Temperatura ótima — processador bem refrigerado",
                    <= 70 => "✅ Temperatura normal para uso do dia a dia",
                    <= 85 => "⚠️ Temperatura elevada — verifique se o computador está ventilado",
                    _ => "⚠️ Temperatura muito alta! Desligue e limpe as ventoinhas do computador"
                };
            }

            return null;
        }

        private PecaInfo LerMemoriaRAM()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem");

                foreach (ManagementObject obj in searcher.Get())
                {
                    long totalKB = long.Parse(obj["TotalVisibleMemorySize"].ToString()!);
                    long livreKB = long.Parse(obj["FreePhysicalMemory"].ToString()!);
                    long totalGB = totalKB / (1024 * 1024);
                    long livreGB = livreKB / (1024 * 1024);
                    long usadoGB = totalGB - livreGB;

                    string dica = totalGB >= 16
                        ? "✅ Excelente para qualquer uso"
                        : totalGB >= 8
                            ? "✅ Boa para uso do dia a dia"
                            : totalGB >= 4
                                ? "⚠️ Suficiente para uso básico"
                                : "⚠️ Pode deixar o computador lento";

                    return new PecaInfo
                    {
                        Icone = "🧠",
                        Nome = "Memória RAM",
                        Valor = $"{totalGB} GB no total",
                        Explicacao = "A memória RAM é como a mesa de trabalho do computador. " +
                                     "Quanto mais RAM, mais coisas você pode fazer ao mesmo tempo " +
                                     "sem o computador travar.",
                        Detalhe = $"Em uso agora: {usadoGB} GB  •  Livre: {livreGB} GB",
                        Dica = dica
                    };
                }
            }
            catch { }

            return new PecaInfo
            {
                Icone = "🧠",
                Nome = "Memória RAM",
                Valor = "Não foi possível ler",
                Explicacao = "A memória RAM é como a mesa de trabalho do computador."
            };
        }

        private PecaInfo LerPlacaVideo()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT Name, AdapterRAM FROM Win32_VideoController");

                foreach (ManagementObject obj in searcher.Get())
                {
                    string nome = obj["Name"]?.ToString() ?? "Desconhecida";
                    long ramBytes = obj["AdapterRAM"] != null
                        ? long.Parse(obj["AdapterRAM"].ToString()!) : 0;
                    string ram = ramBytes > 0
                        ? $"{ramBytes / (1024 * 1024 * 1024)} GB de memória"
                        : "memória integrada";

                    return new PecaInfo
                    {
                        Icone = "🎮",
                        Nome = "Placa de Vídeo",
                        Valor = nome,
                        Explicacao = "A placa de vídeo é responsável por mostrar as imagens " +
                                     "na tela. Quanto melhor a placa, mais fluidas ficam as " +
                                     "imagens, vídeos e jogos.",
                        Detalhe = $"Memória de vídeo: {ram}",
                        Dica = nome.Contains("NVIDIA") || nome.Contains("AMD") ||
                               nome.Contains("Radeon")
                            ? "✅ Placa dedicada — ótima para vídeos e jogos"
                            : "💡 Placa integrada — boa para uso do dia a dia"
                    };
                }
            }
            catch { }

            return new PecaInfo
            {
                Icone = "🎮",
                Nome = "Placa de Vídeo",
                Valor = "Não foi possível ler",
                Explicacao = "A placa de vídeo mostra as imagens na tela."
            };
        }

        private PecaInfo LerSistemaOperacional()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT Caption, Version, OSArchitecture FROM Win32_OperatingSystem");

                foreach (ManagementObject obj in searcher.Get())
                {
                    string nome = obj["Caption"]?.ToString() ?? "Windows";
                    string arquitetura = obj["OSArchitecture"]?.ToString() ?? "";

                    return new PecaInfo
                    {
                        Icone = "🪟",
                        Nome = "Sistema Operacional",
                        Valor = nome,
                        Explicacao = "O sistema operacional é o programa principal do computador. " +
                                     "Ele controla tudo e permite que você use outros programas. " +
                                     "O Windows é o sistema operacional mais usado no mundo.",
                        Detalhe = $"Arquitetura: {arquitetura}",
                        Dica = nome.Contains("11")
                            ? "✅ Você tem o Windows mais moderno!"
                            : nome.Contains("10")
                                ? "✅ Windows 10 — seguro e bem suportado"
                                : "⚠️ Considere atualizar seu Windows"
                    };
                }
            }
            catch { }

            return new PecaInfo
            {
                Icone = "🪟",
                Nome = "Sistema Operacional",
                Valor = "Não foi possível ler",
                Explicacao = "O sistema operacional controla o computador."
            };
        }

        private PecaInfo LerPlacaMae()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT Manufacturer, Product FROM Win32_BaseBoard");

                foreach (ManagementObject obj in searcher.Get())
                {
                    string fabricante = obj["Manufacturer"]?.ToString() ?? "Desconhecido";
                    string modelo = obj["Product"]?.ToString() ?? "Desconhecido";

                    return new PecaInfo
                    {
                        Icone = "🔌",
                        Nome = "Placa-Mãe",
                        Valor = $"{fabricante} — {modelo}",
                        Explicacao = "A placa-mãe é a peça principal que conecta todas as " +
                                     "outras peças do computador entre si. É como o esqueleto " +
                                     "do computador.",
                        Detalhe = $"Fabricante: {fabricante}",
                        Dica = "💡 Importante para saber a compatibilidade do seu computador"
                    };
                }
            }
            catch { }

            return new PecaInfo
            {
                Icone = "🔌",
                Nome = "Placa-Mãe",
                Valor = "Não foi possível ler",
                Explicacao = "A placa-mãe conecta todas as peças do computador."
            };
        }

        private List<PecaInfo> LerDiscos()
        {
            var lista = new List<PecaInfo>();
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT Model, Size FROM Win32_DiskDrive");

                int numero = 1;
                foreach (ManagementObject obj in searcher.Get())
                {
                    string modelo = obj["Model"]?.ToString() ?? "Desconhecido";
                    long tamanhoBytes = obj["Size"] != null
                        ? long.Parse(obj["Size"].ToString()!) : 0;
                    long tamanhoGB = tamanhoBytes / (1024 * 1024 * 1024);
                    string tipo = modelo.ToUpper().Contains("SSD") ||
                                  modelo.ToUpper().Contains("NVME") ||
                                  modelo.ToUpper().Contains("SOLID")
                        ? "SSD (rápido)" : "HD (tradicional)";

                    lista.Add(new PecaInfo
                    {
                        Icone = "💽",
                        Nome = $"Disco {numero} — Armazenamento",
                        Valor = modelo,
                        Explicacao = "O disco é onde ficam salvos todos os seus arquivos, " +
                                     "fotos, programas e o próprio Windows. " +
                                     "Existem dois tipos: o HD tradicional (mais lento) " +
                                     "e o SSD (muito mais rápido).",
                        Detalhe = $"Tamanho: {tamanhoGB} GB  •  Tipo detectado: {tipo}",
                        Dica = tipo.Contains("SSD")
                            ? "✅ SSD — seu computador liga e abre programas rapidamente!"
                            : "💡 HD tradicional — funciona bem, mas um SSD deixaria tudo mais rápido"
                    });
                    numero++;
                }
            }
            catch { }
            return lista;
        }

        private List<PecaInfo> LerPlacasRede()
        {
            var lista = new List<PecaInfo>();
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT Name, AdapterType FROM Win32_NetworkAdapter " +
                    "WHERE PhysicalAdapter = True");

                foreach (ManagementObject obj in searcher.Get())
                {
                    string nome = obj["Name"]?.ToString() ?? "Desconhecida";
                    string tipo = obj["AdapterType"]?.ToString() ?? "";

                    if (nome.ToLower().Contains("virtual") ||
                        nome.ToLower().Contains("loopback") ||
                        nome.ToLower().Contains("miniport")) continue;

                    bool wifi = nome.ToLower().Contains("wireless") ||
                                nome.ToLower().Contains("wi-fi") ||
                                nome.ToLower().Contains("wifi");

                    lista.Add(new PecaInfo
                    {
                        Icone = wifi ? "📶" : "🔌",
                        Nome = wifi ? "Placa de Rede — Wi-Fi" : "Placa de Rede — Cabo",
                        Valor = nome,
                        Explicacao = wifi
                            ? "Esta placa permite que seu computador se conecte " +
                              "à internet sem fio, pelo Wi-Fi."
                            : "Esta placa permite que seu computador se conecte " +
                              "à internet usando um cabo de rede.",
                        Detalhe = string.IsNullOrEmpty(tipo) ? "" : $"Tipo: {tipo}",
                        Dica = "✅ Placa de rede detectada e funcional"
                    });
                }
            }
            catch { }
            return lista;
        }

        private PecaInfo LerBateria()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT Name, EstimatedChargeRemaining, BatteryStatus FROM Win32_Battery");

                foreach (ManagementObject obj in searcher.Get())
                {
                    string nome = obj["Name"]?.ToString() ?? "Bateria";
                    string carga = obj["EstimatedChargeRemaining"]?.ToString() ?? "?";
                    int status = obj["BatteryStatus"] != null
                        ? int.Parse(obj["BatteryStatus"].ToString()!) : 0;

                    string statusTexto = status switch
                    {
                        1 => "Descarregando (na bateria)",
                        2 => "Carregando",
                        3 => "Totalmente carregada",
                        _ => "Status desconhecido"
                    };

                    return new PecaInfo
                    {
                        Icone = "🔋",
                        Nome = "Bateria (Notebook)",
                        Valor = nome,
                        Explicacao = "A bateria permite usar o notebook sem precisar " +
                                     "estar conectado à tomada. Quando a carga acaba, " +
                                     "o computador desliga automaticamente.",
                        Detalhe = $"Carga atual: {carga}%  •  Status: {statusTexto}",
                        Dica = int.TryParse(carga, out int c) && c > 50
                            ? "✅ Bateria com boa carga"
                            : "💡 Considere conectar o carregador em breve"
                    };
                }
            }
            catch { }

            return new PecaInfo
            {
                Icone = "🔋",
                Nome = "Bateria",
                Valor = "Não foi possível ler",
                Explicacao = ""
            };
        }

        private PecaInfo LerAudio()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT Name FROM Win32_SoundDevice");

                var dispositivos = new List<string>();
                foreach (ManagementObject obj in searcher.Get())
                {
                    string nome = obj["Name"]?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(nome))
                        dispositivos.Add(nome);
                }

                if (dispositivos.Count > 0)
                {
                    return new PecaInfo
                    {
                        Icone = "🔊",
                        Nome = "Dispositivo de Áudio",
                        Valor = dispositivos[0],
                        Explicacao = "O dispositivo de áudio é responsável pelo som do " +
                                     "seu computador — permite ouvir músicas, vídeos, " +
                                     "chamadas e os sons do sistema.",
                        Detalhe = dispositivos.Count > 1
                            ? $"+ {dispositivos.Count - 1} outro(s) dispositivo(s) detectado(s)"
                            : "",
                        Dica = "✅ Áudio detectado e disponível"
                    };
                }
            }
            catch { }

            return new PecaInfo
            {
                Icone = "🔊",
                Nome = "Dispositivo de Áudio",
                Valor = "Não foi possível ler",
                Explicacao = ""
            };
        }

        private PecaInfo LerMonitor()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT Name, ScreenWidth, ScreenHeight FROM Win32_VideoController");

                foreach (ManagementObject obj in searcher.Get())
                {
                    string largura = obj["ScreenWidth"]?.ToString() ?? "?";
                    string altura = obj["ScreenHeight"]?.ToString() ?? "?";
                    string resolucao = largura != "?" && altura != "?"
                        ? $"{largura} x {altura} pixels" : "Não detectada";

                    string qualidade = largura != "?" &&
                                       int.TryParse(largura, out int w) && w >= 1920
                        ? "✅ Resolução Full HD ou superior — imagem muito nítida"
                        : "💡 Resolução padrão — adequada para uso do dia a dia";

                    return new PecaInfo
                    {
                        Icone = "🖥️",
                        Nome = "Tela / Monitor",
                        Valor = $"Resolução: {resolucao}",
                        Explicacao = "A resolução da tela define a nitidez da imagem. " +
                                     "Quanto maior a resolução, mais detalhes aparecem " +
                                     "na tela — textos mais nítidos, imagens mais claras.",
                        Detalhe = resolucao != "Não detectada"
                            ? $"A tela exibe {largura} pontos na horizontal e {altura} na vertical"
                            : "",
                        Dica = qualidade
                    };
                }
            }
            catch { }

            return new PecaInfo
            {
                Icone = "🖥️",
                Nome = "Tela / Monitor",
                Valor = "Não foi possível ler",
                Explicacao = ""
            };
        }

        // ─── MODELO ────────────────────────────────────────────────

        private class PecaInfo
        {
            public string Icone { get; set; } = "";
            public string Nome { get; set; } = "";
            public string Valor { get; set; } = "";
            public string Explicacao { get; set; } = "";
            public string Detalhe { get; set; } = "";
            public string Dica { get; set; } = "";
        }

        // ─── CRIAR CARTÃO VISUAL ───────────────────────────────────

        private Border CriarCartaoPeca(PecaInfo peca)
        {
            var cartao = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(18),
                Margin = new Thickness(0, 0, 0, 12),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 6,
                    ShadowDepth = 1,
                    Opacity = 0.1
                }
            };

            var painel = new StackPanel();

            var titulo = new StackPanel { Orientation = Orientation.Horizontal };
            titulo.Children.Add(new TextBlock
            {
                Text = peca.Icone,
                FontSize = 22,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            });
            titulo.Children.Add(new TextBlock
            {
                Text = peca.Nome,
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(30, 58, 95)),
                VerticalAlignment = VerticalAlignment.Center
            });
            painel.Children.Add(titulo);

            painel.Children.Add(new TextBlock
            {
                Text = peca.Valor,
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(42, 96, 150)),
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 6, 0, 6),
                TextWrapping = TextWrapping.Wrap
            });

            painel.Children.Add(new TextBlock
            {
                Text = peca.Explicacao,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(80, 80, 80)),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 6)
            });

            if (!string.IsNullOrEmpty(peca.Detalhe))
                painel.Children.Add(new TextBlock
                {
                    Text = peca.Detalhe,
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(120, 120, 120)),
                    Margin = new Thickness(0, 0, 0, 6),
                    TextWrapping = TextWrapping.Wrap
                });

            if (!string.IsNullOrEmpty(peca.Dica))
            {
                bool positivo = peca.Dica.StartsWith("✅");
                painel.Children.Add(new Border
                {
                    Background = new SolidColorBrush(positivo
                        ? Color.FromRgb(230, 245, 235)
                        : Color.FromRgb(255, 248, 225)),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(10, 5, 10, 5),
                    Child = new TextBlock
                    {
                        Text = peca.Dica,
                        FontSize = 11,
                        Foreground = new SolidColorBrush(positivo
                            ? Color.FromRgb(30, 100, 50)
                            : Color.FromRgb(150, 100, 0)),
                        TextWrapping = TextWrapping.Wrap
                    }
                });
            }

            cartao.Child = painel;
            return cartao;
        }

        // ─── COPIAR INFORMAÇÕES ────────────────────────────────────

        private void CopiarInformacoes_Click(object sender, RoutedEventArgs e)
        {
            if (_pecasCarregadas.Count == 0)
            {
                MessageBox.Show(
                    "As informações ainda estão sendo carregadas.\nAguarde um momento.",
                    "Aguarde", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var texto = new StringBuilder();
            texto.AppendLine("=== INFORMAÇÕES DO COMPUTADOR ===");
            texto.AppendLine($"Data: {DateTime.Now:dd/MM/yyyy HH:mm}");
            texto.AppendLine();

            foreach (var peca in _pecasCarregadas)
            {
                texto.AppendLine($"[ {peca.Nome} ]");
                texto.AppendLine($"  {peca.Valor}");
                if (!string.IsNullOrEmpty(peca.Detalhe))
                    texto.AppendLine($"  {peca.Detalhe}");
                if (!string.IsNullOrEmpty(peca.Dica))
                    texto.AppendLine($"  {peca.Dica}");
                texto.AppendLine();
            }

            texto.AppendLine("=== FIM DO RELATÓRIO ===");
            texto.AppendLine("Gerado pelo Tradutor do Computador");

            Clipboard.SetText(texto.ToString());

            MessageBox.Show(
                "📋 Informações copiadas!\n\n" +
                "Agora você pode colar em qualquer lugar:\n" +
                "• No WhatsApp para mandar para alguém\n" +
                "• No bloco de notas para salvar\n" +
                "• No e-mail para enviar a um técnico\n\n" +
                "Para colar, use Ctrl+V ou clique com o botão\n" +
                "direito e escolha 'Colar'.",
                "Copiado!", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}