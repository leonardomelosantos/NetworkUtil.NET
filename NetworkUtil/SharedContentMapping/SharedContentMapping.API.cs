using System.Runtime.InteropServices;

namespace NetworkUtil
{
    /// <summary>
    /// Classe responsável pelo mapeamento de unidades de rede. Utilize os métodos estáticos.
    /// </summary>
    public partial class SharedContentMapping
    {

        [DllImport("mpr.dll")]
        private static extern int WNetAddConnection2A(ref structNetResource pstNetRes, string psPassword, string psUsername, int piFlags);
        [DllImport("mpr.dll")]
        private static extern int WNetCancelConnection2A(string psName, int piFlags, int pfForce);
        [DllImport("mpr.dll")]
        private static extern int WNetConnectionDialog(int phWnd, int piType);
        [DllImport("mpr.dll")]
        private static extern int WNetDisconnectDialog(int phWnd, int piType);
        [DllImport("mpr.dll")]
        private static extern int WNetRestoreConnectionW(int phWnd, string psLocalDrive);

        [StructLayout(LayoutKind.Sequential)]
        private struct structNetResource
        {
            public int iScope;
            public int iType;
            public int iDisplayType;
            public int iUsage;
            public string sLocalName;
            public string sRemoteName;
            public string sComment;
            public string sProvider;
        }

        private const int RESOURCETYPE_DISK = 0x1;

        // Somente para Windows NT
        private const int CONNECT_COMMANDLINE = 0x00000800;
        private const int CONNECT_CMD_SAVECRED = 0x00001000;
        // Padrão
        private const int CONNECT_INTERACTIVE = 0x00000008;
        private const int CONNECT_PROMPT = 0x00000010;
        private const int CONNECT_UPDATE_PROFILE = 0x00000001;
        // Internet Explorer 4 ou superior
        private const int CONNECT_REDIRECT = 0x00000080;
    }
}
