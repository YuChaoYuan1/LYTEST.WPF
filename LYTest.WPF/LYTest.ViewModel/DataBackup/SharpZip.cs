using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.ViewModel.DataBackup
{
    public class SharpZip
    {
        private const long BUFFER_SIZE = 4096;

        /// <summary>
        /// 压缩文件
        /// </summary>
        /// <param name="fileNames"></param>
        /// <param name="zipFileName"></param>
        public static void CompressFiles(List<string> fileNames, string zipFileName)
        {
            foreach (string file in fileNames)
            {
                CompressFile(zipFileName, file);
            }
        }

        public static  void CompressDirectory(string sourcePath,string zipPath)
        {
            try
            {
                ZipFile.CreateFromDirectory(sourcePath, zipPath);

                Console.WriteLine("文件夹已成功压缩为：" + zipPath);
            }
            catch (Exception)
            {
            }
            // 要压缩的文件夹路径  
            //string sourceDirectory = @"D:\自动备份工作\电表\test\Resource\2023-12-21";
            //// 压缩后的文件路径  
            //string destinationFile = @"C:\Users\lenovo\Desktop\新建文件夹\123.zip";

            // 创建压缩文件  

        }

        /// <summary>
        /// 压缩文件
        /// </summary>
        /// <param name="zipFilename"></param>
        /// <param name="fileToAdd"></param>
        public static void CompressFile(string zipFilename, string fileToAdd)
        {
            using (Package zip = System.IO.Packaging.Package.Open(zipFilename, FileMode.OpenOrCreate))
            {
                string destFilename = " .\\ " + Path.GetFileName(fileToAdd);
                Uri uri = PackUriHelper.CreatePartUri(new Uri(destFilename, UriKind.Relative));
                if (zip.PartExists(uri))
                {
                    zip.DeletePart(uri);
                }
                PackagePart part = zip.CreatePart(uri, "", CompressionOption.Normal);
                using (FileStream fileStream = new FileStream(fileToAdd, FileMode.Open, FileAccess.Read))
                {
                    using (Stream dest = part.GetStream())
                    {
                        CopyStream(fileStream, dest);
                    }
                }
            }
        }

        /// <summary>
        /// 解压文件
        /// </summary>
        /// <param name="zipFilename"></param>
        /// <param name="outPath"></param>
        public static void DecompressFile(string zipFilename, string outPath)
        {
            using (Package zip = System.IO.Packaging.Package.Open(zipFilename, FileMode.Open))
            {
                foreach (PackagePart part in zip.GetParts())
                {
                    string outFileName = Path.Combine(outPath, part.Uri.OriginalString.Substring(1));
                    using (System.IO.FileStream outFileStream = new System.IO.FileStream(outFileName, FileMode.Create))
                    {
                        using (Stream inFileStream = part.GetStream())
                        {
                            CopyStream(inFileStream, outFileStream);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 复制流
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="outputStream"></param>
        private static void CopyStream(System.IO.Stream inputStream, System.IO.Stream outputStream)
        {
            long bufferSize = inputStream.Length < BUFFER_SIZE ? inputStream.Length : BUFFER_SIZE;
            byte[] buffer = new byte[bufferSize];
            int bytesRead = 0;
            long bytesWritten = 0;
            while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                outputStream.Write(buffer, 0, bytesRead);
                bytesWritten += bufferSize;
            }
        }
    }
}
