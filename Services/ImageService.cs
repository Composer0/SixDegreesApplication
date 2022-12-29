// This file does not have the I for interface because it implements the interface.

using ContactPro.Services.Interfaces;

namespace ContactPro.Services
{
    public class ImageService : IImageService
    {
        private readonly string[] suffixes = { "Bytes", "KB", "MB", "GB", "TB", "PB" }; // these describe the potential byte arrays.
        private readonly string defaultImage = "img/DefaultContactImage.png";
        public string ConvertByteArrayToFile(byte[] fileData, string extension)
        {
            if (fileData is null) return defaultImage;
            try
            {
                string imageBase64Data = Convert.ToBase64String(fileData); //converts to byte string which can convert into an html image tag.
                return string.Format($"data:{extension};base64,{imageBase64Data}");
            }
            catch(Exception)
            {
                throw;
            }
        }

        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file)
        {
            try
            {
                using MemoryStream memoryStream = new();
                await file.CopyToAsync(memoryStream);
                byte[] byteFile = memoryStream.ToArray();
                return byteFile;
            }
            catch(Exception)
            {
                throw;
            }
        }
    }
}
