using System;
using System.IO;
using System.Runtime.InteropServices;

namespace NetworkUtil
{
    /// <summary>
    /// Classe responsável pelo mapeamento de unidades de rede. Utilize os métodos estáticos.
    /// </summary>
    public partial class SharedContentMapping
    {
        
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

        #region Methods

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
            structNetResource stuctNet = new structNetResource
            {
                iScope = 2,
                iType = RESOURCETYPE_DISK,
                iDisplayType = 3,
                iUsage = 1,
                sRemoteName = this.ShareName,
                sLocalName = this.LocalDrive
            };

            // Preparing flags using properties values
            int iFlags = 0;
            if (this.SaveCredentials) { iFlags += CONNECT_CMD_SAVECRED; }
            if (this.Persistent) { iFlags += CONNECT_UPDATE_PROFILE; }
            if (this.PromptForCredentials) { iFlags += CONNECT_INTERACTIVE + CONNECT_PROMPT; }

            // If force, unmap ready for new connection
            if (this.Force) { try { UnMapDriveInternal(true); } catch { /* Ignoring this try. */ } }

            // Start mapping and return the result
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

    }
}
