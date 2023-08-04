using PgpCore;
using System.IO;
using System.Threading.Tasks;
using System;
using Serilog;

namespace nordelta.cobra.webapi.Helpers
{
    public class PgpCoreManager
    {
        public static void EncryptFile(string inFilePath, string encryptFile, string publicKeyFilePath)
        {
            try
            {
                FileInfo publicKey = new FileInfo(publicKeyFilePath);
                FileInfo inputFile = new FileInfo(inFilePath);
                FileInfo encryptedFile = new FileInfo(encryptFile);

                EncryptionKeys encryptionKeys = new EncryptionKeys(publicKey);
                using PGP pgp = new PGP(encryptionKeys);
                {
                    var inFile = new FileInfo(inFilePath);
                    pgp.EncryptFile(inputFile, encryptedFile);
                };
            }
            catch (Exception e)
            {
                Log.Error(e, "EncryptFile: Error al momento de encriptar archivo: {filePath}", inFilePath);
            }
        }

        public static void DecryptFile(string inFilePath, string decryptFile, string privateKeyFilePath, string password)
        {
            try
            {
                FileInfo privateKey = new FileInfo(privateKeyFilePath);
                EncryptionKeys encryptionKeys = new EncryptionKeys(privateKey, password);

                FileInfo inputFile = new FileInfo(inFilePath);
                FileInfo decryptedFile = new FileInfo(decryptFile);

                PGP pgp = new PGP(encryptionKeys);
                pgp.DecryptFile(inputFile, decryptedFile);
            }
            catch (Exception e)
            {
                Log.Error(e, "DecryptFile: Error al momento de desencriptar archivo: {filePath}", inFilePath);
            }
        }

        public static async Task DecryptAllFiles(string inFolderPath, string outFolderPath, string privateKeyFilePath, string password, string fileNameEncrypted = null)
        {
            try
            {
                var files = new DirectoryInfo(inFolderPath).GetFiles();

                foreach (var file in files)
                {
                    var fileFile = !string.IsNullOrEmpty(fileNameEncrypted) ? fileNameEncrypted + "-" + DateTime.Now.ToString("yyyyMMdd") + "-" + DateTime.Now.ToString("hhmmss") + ".txt" :
                                   Path.GetFileNameWithoutExtension(file.Name) + ".txt";
                    DecryptFile(file.FullName, Path.Combine(outFolderPath, fileFile), privateKeyFilePath, password);
                    if (!string.IsNullOrEmpty(fileNameEncrypted)) await Task.Delay(1000);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "DecryptAllFiles: Error al momento de desencriptar archivos de la carpeta: {folderPath}", inFolderPath);
            }
        }

        public static async Task EncryptAllFiles(string inFolderPath, string outFolderPath, string publicKeyFilePath, string fileNameEncrypted = null)
        {
            try
            {
                var files = new DirectoryInfo(inFolderPath).GetFiles();

                foreach (var file in files)
                {
                    var fileName = !string.IsNullOrEmpty(fileNameEncrypted) ? fileNameEncrypted + "-" + DateTime.Now.ToString("yyyyMMdd") + "-" + DateTime.Now.ToString("hhmmss") + ".PGP" :
                                   Path.GetFileNameWithoutExtension(file.Name) + ".PGP";
                    EncryptFile(file.FullName, Path.Combine(outFolderPath, fileName), publicKeyFilePath);
                    if (!string.IsNullOrEmpty(fileNameEncrypted)) await Task.Delay(1000);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "EncryptAllFiles: Error al momento de encriptar archivos de la carpeta: {folderPath}", inFolderPath);
            }
        }
    }
}
