using System;
using System.IO;
using System.Runtime.InteropServices;

namespace NetworkUtil
{
    /// <summary>
    /// Classe responsável pelo mapeamento de unidades de rede. Utilize os métodos estáticos.
    /// </summary>
    public class SharedContentMapping
    {

        #region Public methods

        /// <summary>
        /// Static method with main parameters
        /// </summary>
        /// <param name="unitDrive">Sample: X:</param>
        /// <param name="pathNetwork">Sample: \\myserver</param>
        /// <param name="username">Sample: domain\billgates</param>
        /// <param name="password"></param>
        public static ResultOperation MapDrive(string unitDrive, string pathNetwork, string username, string password)
        {
            ResultOperation result = new ResultOperation();
            result.ProcessedOK = true;

            try
            {
                // Verificando se os parâmetros de mapeamento foram mencionados no Web.config
                if (!string.IsNullOrEmpty(unitDrive) && !string.IsNullOrEmpty(pathNetwork)) // && !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    // Verificando se o mapeamento já existe, pois se já existir não precisará mais proceder com o mapeamento
                    DirectoryInfo rootFolder = new DirectoryInfo(unitDrive + (unitDrive.EndsWith(":") ? "" : ":") + @"\");
                    if (!CommonInternal.IsAccessableFolder(rootFolder))
                    {
                        // Utilizando a API de mapeamento
                        SharedContentMapping networkExtendedDrive = new SharedContentMapping();
                        networkExtendedDrive.Force = true;
                        networkExtendedDrive.Persistent = true;
                        networkExtendedDrive.LocalDrive = unitDrive + (unitDrive.EndsWith(":") ? "" : ":");
                        networkExtendedDrive.PromptForCredentials = false;
                        networkExtendedDrive.ShareName = pathNetwork;
                        networkExtendedDrive.SaveCredentials = true;
                        networkExtendedDrive.MapDrive(username, password);
                        networkExtendedDrive = null;
                    }
                }
                else
                {
                    result.ProcessedOK = false;
                    result.Message = "Drive destination and network path are necessary for mapping drive.";
                }
            }
            catch (Exception ex)
            {
                result.ProcessedOK = false;
                result.Exception = ex;
            }

            return result;
        }

        #region Wrapper methods

        /// <summary>
        /// Map network drive without credentials
        /// </summary>
        public void MapDrive() { MapDriveInternal(null, null); }

        /// <summary>
        /// Map network drive using password
        /// </summary>
        /// <param name="pPassword"></param>
        public void MapDrive(string pPassword) { MapDriveInternal(null, pPassword); }

        /// <summary>
        /// Map network drive using username and password
        /// </summary>
        /// <param name="pUsername"></param>
        /// <param name="pPassword"></param>
        public void MapDrive(string pUsername, string pPassword) { MapDriveInternal(pUsername, pPassword); }

        /// <summary>
        /// Unmap network drive
        /// </summary>
        public void UnMapDrive() { UnMapDriveInternal(this.Force); }

        /// <summary>
        /// Check / restore persistent network drive
        /// </summary>
        public void RestoreDrives() { RestoreDriveInternal(); }

        #endregion

        #endregion

        #region Methods

        #region Properties

        /// <summary>
        /// Option to save credentials are reconnection...
        /// </summary>
        private bool SaveCredentials { get; set; }

        /// <summary>
        /// Option to reconnect drive after log off / reboot ...
        /// </summary>
        private bool Persistent { get; set; }

        /// <summary>
        /// Option to force connection if drive is already mapped...
        /// or force disconnection if network path is not responding...
        /// </summary>
        private bool Force { get; set; }

        /// <summary>
        /// Option to prompt for user credintals when mapping a drive
        /// </summary>
        private bool PromptForCredentials { get; set; }

        /// <summary>
        /// Drive to be used in mapping / unmapping...
        /// </summary>
        private string LocalDrive { get; set; }

        /// <summary>
        /// Share address to map drive to.
        /// </summary>
        private string ShareName { get; set; }

        #endregion

        /// <summary>
        /// Execute the mapping
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        private void MapDriveInternal(string username, string password)
        {
            if (string.IsNullOrEmpty(username)) { username = null; }
            if (string.IsNullOrEmpty(password)) { password = null; }

            //create struct data
            structNetResource stuctNet = new structNetResource();
            stuctNet.iScope = 2;
            stuctNet.iType = RESOURCETYPE_DISK;
            stuctNet.iDisplayType = 3;
            stuctNet.iUsage = 1;
            stuctNet.sRemoteName = this.ShareName;
            stuctNet.sLocalName = this.LocalDrive;

            // Preparing flags using properties values
            int iFlags = 0;
            if (this.SaveCredentials) { iFlags += CONNECT_CMD_SAVECRED; }
            if (this.Persistent) { iFlags += CONNECT_UPDATE_PROFILE; }
            if (this.PromptForCredentials) { iFlags += CONNECT_INTERACTIVE + CONNECT_PROMPT; }

            // If force, unmap ready for new connection
            if (this.Force) { try { UnMapDriveInternal(true); } catch { } }

            // Start mapping and return the result
            string usernameFull = username;
            int result = WNetAddConnection2A(ref stuctNet, password, username, iFlags);
            if (result > 0)
            {
                throw new System.ComponentModel.Win32Exception(result);
            }
        }

        /// <summary>
        /// Unmap network drive	
        /// </summary>
        /// <param name="force"></param>
        private void UnMapDriveInternal(bool force)
        {
            // Start unmapping and return the result
            int iFlags = 0;
            if (this.Persistent) { iFlags += CONNECT_UPDATE_PROFILE; }

            int result = WNetCancelConnection2A(this.LocalDrive, iFlags, Convert.ToInt32(force));
            if (result != 0) result = WNetCancelConnection2A(this.ShareName, iFlags, Convert.ToInt32(force));  // Disconnect if localname was null
            if (result > 0) { throw new System.ComponentModel.Win32Exception(result); }
        }

        /// <summary>
        /// Check / Restore a network drive 
        /// </summary>
        private void RestoreDriveInternal()
        {
            // Start restore and return
            int result = WNetRestoreConnectionW(0, null);
            if (result > 0) { throw new System.ComponentModel.Win32Exception(result); }
        }

        #endregion

        #region Necessary resources to use API

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

        #endregion

    }
}
