﻿using System;
using System.Runtime.InteropServices;
using System.IO;

namespace NetworkUtil
{
    /// <summary>
    /// Classe responsável pela manipulação direta de conteúdo compartilhado (shareds folder).
    /// </summary>
    public class SharedContentAccess
    {

        #region Public methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static ResultOperation ConnectSharedContent(string path, string username, string password)
        {
            ResultOperation retorno = new ResultOperation();
            retorno.ProcessedOK = true;

            try
            {
                if (!string.IsNullOrEmpty(path) && path.Trim().StartsWith(@"\\") && path.Trim().Length >= 3)
                {
                    // Verificando se o mapeamento já existe, pois se já existir não precisará mais proceder com o mapeamento
                    DirectoryInfo rootFolder = new DirectoryInfo(path);
                    if (!CommonInternal.IsAccessableFolder(rootFolder))
                    {
                        // Utilizando a API de acesso a pastas de rede
                        ConnectToRemoteInternal(path, username, password);

                        string[] folders = path.Split(new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries);
                        string subRemoteUNC = string.Empty;
                        for (int i = 0; i < (folders.Length - 1); i++)
                        {
                            subRemoteUNC = subRemoteUNC + folders[i] + @"\";
                        }
                        if (!string.IsNullOrEmpty(subRemoteUNC))
                        {
                            ConnectSharedContent(@"\\" + subRemoteUNC, username, password);
                        }
                    }
                }
                else
                {
                    retorno.ProcessedOK = false;
                    retorno.Message = "Folder selected isn't valid network path.";
                }
            }
            catch (Exception ex)
            {
                retorno.ProcessedOK = false;
                retorno.Exception = ex;
                retorno.Message = ex.Message;
            }

            return retorno;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="remoteUNC"></param>
        /// <returns></returns>
        public static string DisconnectRemote(string remoteUNC)
        {
            int ret = WNetCancelConnection2(remoteUNC, CONNECT_UPDATE_PROFILE_2, false);
            if (ret == NO_ERROR) return null;
            return GetErrorForNumber(ret);
        }

        #endregion

        #region Private methods

        private static string ConnectToRemoteInternal(string remoteUNC, string username, string password)
        {
            return ConnectToRemoteInternal(remoteUNC, username, password, false);
        }

        private static string ConnectToRemoteInternal(string remoteUNC, string username, string password, bool promptUser)
        {
            NETRESOURCE networkResource = new NETRESOURCE();
            networkResource.dwType = RESOURCETYPE_DISK_2;
            networkResource.lpRemoteName = remoteUNC;
            // nr.lpLocalName = "F:";

            int result;
            if (promptUser)
                result = WNetUseConnection(IntPtr.Zero, networkResource, "", "", CONNECT_INTERACTIVE_2 | CONNECT_PROMPT_2, null, null, null);
            else
                result = WNetUseConnection(IntPtr.Zero, networkResource, password, username, 0, null, null, null);

            if (result == NO_ERROR) return null;

            return GetErrorForNumber(result);
        }

        #endregion

        #region API Conts

        const int RESOURCE_CONNECTED = 0x00000001;
        const int RESOURCE_GLOBALNET = 0x00000002;
        const int RESOURCE_REMEMBERED = 0x00000003;
        const int RESOURCETYPE_ANY = 0x00000000;
        const int RESOURCETYPE_DISK_2 = 0x00000001;
        const int RESOURCETYPE_PRINT = 0x00000002;
        const int RESOURCEDISPLAYTYPE_GENERIC = 0x00000000;
        const int RESOURCEDISPLAYTYPE_DOMAIN = 0x00000001;
        const int RESOURCEDISPLAYTYPE_SERVER = 0x00000002;
        const int RESOURCEDISPLAYTYPE_SHARE = 0x00000003;
        const int RESOURCEDISPLAYTYPE_FILE = 0x00000004;
        const int RESOURCEDISPLAYTYPE_GROUP = 0x00000005;
        const int RESOURCEUSAGE_CONNECTABLE = 0x00000001;
        const int RESOURCEUSAGE_CONTAINER = 0x00000002;
        const int CONNECT_INTERACTIVE_2 = 0x00000008;
        const int CONNECT_PROMPT_2 = 0x00000010;
        const int CONNECT_REDIRECT_2 = 0x00000080;
        const int CONNECT_UPDATE_PROFILE_2 = 0x00000001;
        const int CONNECT_COMMANDLINE_2 = 0x00000800;
        const int CONNECT_CMD_SAVECRED_2 = 0x00001000;
        const int CONNECT_LOCALDRIVE = 0x00000100;

        #endregion

        #region API Handling Errors

        const int NO_ERROR = 0;
        const int ERROR_ACCESS_DENIED = 5;
        const int ERROR_ALREADY_ASSIGNED = 85;
        const int ERROR_BAD_DEVICE = 1200;
        const int ERROR_BAD_NET_NAME = 67;
        const int ERROR_BAD_PROVIDER = 1204;
        const int ERROR_CANCELLED = 1223;
        const int ERROR_EXTENDED_ERROR = 1208;
        const int ERROR_INVALID_ADDRESS = 487;
        const int ERROR_INVALID_PARAMETER = 87;
        const int ERROR_INVALID_PASSWORD = 1216;
        const int ERROR_MORE_DATA = 234;
        const int ERROR_NO_MORE_ITEMS = 259;
        const int ERROR_NO_NET_OR_BAD_PATH = 1203;
        const int ERROR_NO_NETWORK = 1222;
        const int ERROR_BAD_PROFILE = 1206;
        const int ERROR_CANNOT_OPEN_PROFILE = 1205;
        const int ERROR_DEVICE_IN_USE = 2404;
        const int ERROR_NOT_CONNECTED = 2250;
        const int ERROR_OPEN_FILES = 2401;

        private static ErrorClass[] ERROR_LIST = new ErrorClass[] {
            new ErrorClass(ERROR_ACCESS_DENIED, "Error: Access Denied"),
            new ErrorClass(ERROR_ALREADY_ASSIGNED, "Error: Already Assigned"),
            new ErrorClass(ERROR_BAD_DEVICE, "Error: Bad Device"),
            new ErrorClass(ERROR_BAD_NET_NAME, "Error: Bad Net Name"),
            new ErrorClass(ERROR_BAD_PROVIDER, "Error: Bad Provider"),
            new ErrorClass(ERROR_CANCELLED, "Error: Cancelled"),
            new ErrorClass(ERROR_EXTENDED_ERROR, "Error: Extended Error"),
            new ErrorClass(ERROR_INVALID_ADDRESS, "Error: Invalid Address"),
            new ErrorClass(ERROR_INVALID_PARAMETER, "Error: Invalid Parameter"),
            new ErrorClass(ERROR_INVALID_PASSWORD, "Error: Invalid Password"),
            new ErrorClass(ERROR_MORE_DATA, "Error: More Data"),
            new ErrorClass(ERROR_NO_MORE_ITEMS, "Error: No More Items"),
            new ErrorClass(ERROR_NO_NET_OR_BAD_PATH, "Error: No Net Or Bad Path"),
            new ErrorClass(ERROR_NO_NETWORK, "Error: No Network"),
            new ErrorClass(ERROR_BAD_PROFILE, "Error: Bad Profile"),
            new ErrorClass(ERROR_CANNOT_OPEN_PROFILE, "Error: Cannot Open Profile"),
            new ErrorClass(ERROR_DEVICE_IN_USE, "Error: Device In Use"),
            new ErrorClass(ERROR_EXTENDED_ERROR, "Error: Extended Error"),
            new ErrorClass(ERROR_NOT_CONNECTED, "Error: Not Connected"),
            new ErrorClass(ERROR_OPEN_FILES, "Error: Open Files"),
        };

        private static string GetErrorForNumber(int errorNumber)
        {
            foreach (ErrorClass errorItem in ERROR_LIST)
            {
                if (errorItem.number == errorNumber) return errorItem.message;
            }
            return "Error: Unknown, " + errorNumber;
        }

        private struct ErrorClass
        {
            public int number;
            public string message;

            public ErrorClass(int number, string message)
            {
                this.number = number;
                this.message = message;
            }
        }

        #endregion

        #region API Methods

        [DllImport("Mpr.dll")]
        private static extern int WNetUseConnection(
            IntPtr hwndOwner,
            NETRESOURCE lpNetResource,
            string lpPassword,
            string lpUserID,
            int dwFlags,
            string lpAccessName,
            string lpBufferSize,
            string lpResult
            );

        [DllImport("Mpr.dll")]
        private static extern int WNetCancelConnection2(
            string lpName,
            int dwFlags,
            bool fForce
            );

        [StructLayout(LayoutKind.Sequential)]
        private class NETRESOURCE
        {
            public int dwScope = 0;
            public int dwType = 0;
            public int dwDisplayType = 0;
            public int dwUsage = 0;
            public string lpLocalName = "";
            public string lpRemoteName = "";
            public string lpComment = "";
            public string lpProvider = "";
        }

        #endregion

    }
}
