using System.Windows;

namespace Tradutor
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Carrega o Painel de Controle como tela inicial
            BtnPainelControle_Click(this, null);
        }

        private void BtnPainelControle_Click(object sender, RoutedEventArgs e)
        {
            ConteudoPrincipal.Content = new Views.PainelControleView();
        }

        private void BtnDrivers_Click(object sender, RoutedEventArgs e)
        {
            // Em breve
            MostrarEmBreve("Gerenciador de Drivers");
        }

        private void BtnImpressoras_Click(object sender, RoutedEventArgs e)
        {
            // Em breve
            MostrarEmBreve("Gerenciador de Impressoras");
        }

        private void BtnTerminal_Click(object sender, RoutedEventArgs e)
        {
            // Em breve
            MostrarEmBreve("Terminal Fácil");
        }

        private void MostrarEmBreve(string modulo)
        {
            ConteudoPrincipal.Content = new System.Windows.Controls.TextBlock
            {
                Text = $"🚧 O módulo '{modulo}' está sendo construído!\nVolte em breve.",
                FontSize = 20,
                TextAlignment = System.Windows.TextAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(100, 130, 160))
            };
       
        }
        
    
    }
}