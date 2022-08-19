using Microsoft.AspNetCore.Http;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using SeatConstructor.Models.DB;

namespace SeatConstructor.Controllers
{
    public class FileUploader
    {
        public static async Task<bool> TryUploadFile(IFormFile file, string rootPath, string directory)
        {
            try
            {
                string filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                filename = EnsureCorrectFilename(filename);

                using (FileStream output = File.Create(GetPathAndFilename(rootPath, directory, filename)))
                    await file.CopyToAsync(output);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> TryUploadFile(IFormFile file, string rootPath, string directory, string ending)
        {
            try
            {
                string filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                filename = EnsureCorrectFilename(filename) + ending;

                using (FileStream output = File.Create(GetPathAndFilename(rootPath, directory, filename)))
                    await file.CopyToAsync(output);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void Remove(string path)
        {
            File.Delete(path);
        }

        private static string EnsureCorrectFilename(string filename)
        {
            if (filename.Contains("\\"))
                filename = filename.Substring(filename.LastIndexOf("\\") + 1);

            return filename;
        }

        private static string GetPathAndFilename(string rootPath, string directory, string filename)
        {
            string path = rootPath + directory + filename;
            return path;
        }
    }

}
