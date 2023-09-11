using System;
using System.IO;

namespace NetworkUtil
{
    internal static class CommonInternal
    {
        private const string FULL_DATETIME_FORMAT = "ddMMyyyyhhmmss";
        private const string TXT_FILE_EXTENSION = ".txt";

        /// <summary>
        /// Método que indica se uma pasta está ou não acessível.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        internal static bool IsAccessableFolder(DirectoryInfo folder)
        {
            try
            {
                if (!folder.Exists)
                {
                    return false;
                }

                folder.Refresh();
                int qtdFiles = folder.GetFiles("*", SearchOption.AllDirectories).Length;
                int qtdSubfolders = folder.GetDirectories("*", SearchOption.AllDirectories).Length;
                if ((qtdFiles + qtdSubfolders) <= 0)
                {
                    string tempFileName = folder.FullName + "\\Dummy_" + DateTime.Now.ToString(FULL_DATETIME_FORMAT) + TXT_FILE_EXTENSION;
                    FileInfo novoArquivo = new FileInfo(tempFileName);
                    FileStream fs = novoArquivo.Create();
                    fs.Close();
                    novoArquivo.Delete();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
