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
        #region Constants

        private const string MESSAGE_ERROR_FOLDER_ISNT_VALID = "Folder selected isn't valid network path.";

        #endregion

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
            ResultOperation resultOperation = new ResultOperation()
            { 
                ProcessedOK = true
            };
            
            try
            {
                if (!string.IsNullOrEmpty(path) && path.Trim().StartsWith(@"\\") && path.Trim().Length >= 3)
                {
                    // Verificando se o mapeamento já existe, pois se já existir não precisará mais proceder com o mapeamento
                    DirectoryInfo rootFolder = new DirectoryInfo(path);
                    if (!CommonInternal.IsAccessableFolder(rootFolder))
                    {
                        // Utilizando a API de acesso a pastas de rede
                        string messageResult = ConnectToRemoteInternal(path, username, password);
                        resultOperation.Message = messageResult;

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
                    resultOperation.ProcessedOK = false;
                    resultOperation.Message = MESSAGE_ERROR_FOLDER_ISNT_VALID;
                }
            }
            catch (Exception ex)
            {
                resultOperation.ProcessedOK = false;
                resultOperation.Exception = ex;
                resultOperation.Message = ex.Message;
            }

            return resultOperation;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="remoteUNC"></param>
        /// <returns>Empty if the operation was executed without errors.</returns>
        public static string DisconnectRemote(string remoteUNC)
        {
            int returnValue = WNetCancelConnection2(remoteUNC, CONNECT_UPDATE_PROFILE_2, false);
            if (returnValue == NO_ERROR) 
                return null;
            
            return GetErrorForNumber(returnValue);
        }

        #endregion
    }
}
