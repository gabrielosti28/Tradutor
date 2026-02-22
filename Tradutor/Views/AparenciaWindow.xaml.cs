using Microsoft.Win32;
using System.Diagnostics;
using System.Windows;

namespace Tradutor.Views
{
    public partial class AparenciaWindow : Window
    {
        public AparenciaWindow()
        {
            InitializeComponent();
        }

        private void AtivarModoClaro_Click(object sender, RoutedEventArgs e)
        {
            // Altera o registro do Windows para modo claro
            Registry.SetValue(
                @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                "AppsUseLightTheme", 1, RegistryValueKind.DWord);
            Registry.SetValue(
                @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                "SystemUsesLightTheme", 1, RegistryValueKind.DWord);

            MessageBox.Show(
                "✅ Modo Claro ativado!\n\nAlguns programas podem precisar ser reiniciados para mostrar a mudança.",
                "Modo Claro", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AtivarModoEscuro_Click(object sender, RoutedEventArgs e)
        {
            Registry.SetValue(
                @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                "AppsUseLightTheme", 0, RegistryValueKind.DWord);
            Registry.SetValue(
                @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                "SystemUsesLightTheme", 0, RegistryValueKind.DWord);

            MessageBox.Show(
                "🌙 Modo Escuro ativado!\n\nAlguns programas podem precisar ser reiniciados para mostrar a mudança.",
                "Modo Escuro", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void TrocarPapelParede_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("ms-settings:personalization-background")
            { UseShellExecute = true });
        }

        private void AbrirSlideshow_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("ms-settings:personalization-background")
            { UseShellExecute = true });
        }

        private void EscolherCor_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("ms-settings:personalization-colors")
            { UseShellExecute = true });
        }

        private void AbrirTemas_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("ms-settings:themes")
            { UseShellExecute = true });
        }

        private void AjustarTexto_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("ms-settings:display")
            { UseShellExecute = true });
        }
    }
}