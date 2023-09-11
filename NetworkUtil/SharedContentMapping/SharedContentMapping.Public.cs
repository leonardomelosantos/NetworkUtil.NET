using System;
using System.IO;

namespace NetworkUtil
{
    /// <summary>
    /// Classe responsável pelo mapeamento de unidades de rede. Utilize os métodos estáticos.
    /// </summary>
    public partial class SharedContentMapping
    {
        /// <summary>
        /// Static method with main parameters
        /// </summary>
        /// <param name="unitDrive">Sample: X:</param>
        /// <param name="pathNetwork">Sample: \\myserver</param>
        /// <param name="username">Sample: domain\billgates</param>
        /// <param name="password"></param>
        public static ResultOperation MapDrive(string unitDrive, string pathNetwork, string username, string password)
        {
            ResultOperation result = new ResultOperation
            {
                ProcessedOK = true
            };

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
                        SharedContentMapping networkExtendedDrive = new SharedContentMapping
                        {
                            Force = true,
                            Persistent = true,
                            LocalDrive = unitDrive + (unitDrive.EndsWith(":") ? string.Empty : ":"),
                            PromptForCredentials = false,
                            ShareName = pathNetwork,
                            SaveCredentials = true
                        };
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

    }
}
