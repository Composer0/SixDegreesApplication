// This is used to create functions and parameters for our controllers to use.
// The first I in the document name is a naming convention used to identify when a specific file is an interface.
namespace ContactPro.Services.Interfaces
{
    public interface IImageService
    {
        public Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file);
        public string ConvertByteArrayToFile(byte[] fileData, string extension);
    }
}
