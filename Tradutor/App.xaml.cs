using Microsoft.Win32;
using System.Windows;

namespace Tradutor
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (!IsWindows10OuMais())
            {
                var janela = new WindowsAntigoWindow();
                janela.ShowDialog();
                Shutdown();
            }
        }

        public static bool IsWindows10OuMais()
        {
            try
            {
                // Lê direto do registro — método mais confiável no .NET 9
                string? versaoStr = Registry.GetValue(
                    @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion",
                    "CurrentBuildNumber", null) as string;

                if (int.TryParse(versaoStr, out int build))
                {
                    // Windows 10 começa no build 10240
                    // Windows 11 começa no build 22000
                    return build >= 10240;
                }

                return false;
            }
            catch
            {
                // Se não conseguir ler, assume que é compatível
                // para não bloquear o usuário por engano
                return true;
            }
        }
    }
}