using System.Net;
using MimeDetective;
using Planer_file_server.Core.Entities.Response;
using Planer_file_server.Core.IService;

namespace Planer_file_server.App.Service
{
    public class LocalFileUploaderService : IFileUploaderService
    {
        private readonly ContentInspector _contentInspector;

        public LocalFileUploaderService(ContentInspector contentInspector)
        {
            _contentInspector = contentInspector;
        }

        public async Task<ServiceResponse<string>> UploadFileAsync(string directoryPath, Stream stream, string[] supportedExtensions)
        {
            try
            {
                if (stream == null || stream.Length == 0)
                    return new ServiceResponse<string>()
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false
                    };

                var result = _contentInspector.Inspect(stream);
                var fileExtension = result.MaxBy(e => e.Points)?.Definition.File.Extensions.First();
                if (fileExtension == null || !supportedExtensions.Contains(fileExtension))
                    return new ServiceResponse<string>()
                    {
                        StatusCode = HttpStatusCode.UnsupportedMediaType,
                        IsSuccess = false
                    };

                string filename = $"{Guid.NewGuid()}.{fileExtension}";
                string fullPathToFile = Path.Combine(directoryPath, filename);

                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                using var file = File.Create(fullPathToFile);
                if (file == null)
                    return new ServiceResponse<string>()
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false
                    };

                stream.Seek(0, SeekOrigin.Begin);
                await stream.CopyToAsync(file);
                return new ServiceResponse<string>()
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = true,
                    Body = filename
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<string>()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false
                };
            }
        }

        public async Task<ServiceResponse<Stream>> GetStreamAsync(string directoryPath, string filename)
        {
            string fullPathToFile = Path.Combine(directoryPath, filename);
            if (!File.Exists(fullPathToFile))
                return new ServiceResponse<Stream>()
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false
                };

            Stream fileStream = File.OpenRead(fullPathToFile);

            return new ServiceResponse<Stream>()
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = fileStream
            };
        }

    }
}