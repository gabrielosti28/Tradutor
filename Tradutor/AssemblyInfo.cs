using System.Windows;

namespace Tradutor
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var versao = Environment.OSVersion.Version;
            bool isWindows10ouMais = versao.Major > 6 ||
                                     (versao.Major == 6 && versao.Minor >= 2);

            // Windows 10 tem Major=10. Windows 7=6.1, Windows 8=6.2, Windows 8.1=6.3
            bool isWindows10 = versao.Major >= 10;

            if (!isWindows10)
            {
                var janela = new WindowsAntigoWindow();
                janela.ShowDialog();

                // Encerra o programa após o diálogo
                Shutdown();
            }
        }
    }
}