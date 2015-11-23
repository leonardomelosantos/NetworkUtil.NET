using System;
using System.IO;

namespace NetworkUtil
{
    public class CommonInternal
    {
        /// <summary>
        /// Método que indica se uma pasta está ou não acessível.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public static bool IsAccessableFolder(DirectoryInfo folder)
        {
            bool result = true;

            try
            {
                if (!folder.Exists)
                {
                    result = false;
                }
                else
                {
                    folder.Refresh();
                    int qtdFiles = folder.GetFiles("*", SearchOption.AllDirectories).Length;
                    int qtdSubfolders = folder.GetDirectories("*", SearchOption.AllDirectories).Length;
                    if ((qtdFiles + qtdSubfolders) <= 0)
                    {
                        string tempFileName = folder.FullName + "\\Dummy_" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".txt";
                        FileInfo novoArquivo = new FileInfo(tempFileName);
                        FileStream fs = novoArquivo.Create();
                        fs.Close();
                        novoArquivo.Delete();
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }
    }
}
