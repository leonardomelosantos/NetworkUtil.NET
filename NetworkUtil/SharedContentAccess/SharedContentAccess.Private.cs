using System;
using System.Runtime.InteropServices;
using System.IO;

namespace NetworkUtil
{
    /// <summary>
    /// Classe responsável pela manipulação direta de conteúdo compartilhado (shareds folder).
    /// </summary>
    public partial class SharedContentAccess
    {
        private static string ConnectToRemoteInternal(string remoteUNC, string username, string password)
        {
            return ConnectToRemoteInternal(remoteUNC, username, password, false);
        }

        private static string ConnectToRemoteInternal(string remoteUNC, string username, string password, bool promptUser)
        {
            NETRESOURCE networkResource = new NETRESOURCE
            {
                dwType = RESOURCETYPE_DISK_2,
                lpRemoteName = remoteUNC
            };

            int result;
            if (promptUser)
                result = WNetUseConnection(IntPtr.Zero, networkResource, "", "", CONNECT_INTERACTIVE_2 | CONNECT_PROMPT_2, null, null, null);
            else
                result = WNetUseConnection(IntPtr.Zero, networkResource, password, username, 0, null, null, null);

            if (result == NO_ERROR) 
                return null;

            return GetErrorForNumber(result);
        }
    }
}
