using planner_client_package.Entities;

namespace planner_file_service.Core.IService
{
    public interface IFileUploaderService
    {
        Task<ServiceResponse<string>> UploadFileAsync(string directoryPath, Stream stream, string[] supportedExtensions);
        Task<ServiceResponse<Stream>> GetStreamAsync(string directoryPath, string filename);
    }
}