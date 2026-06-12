using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using planner_client_package.Entities;
using planner_file_service.Core;
using planner_file_service.Core.IService;
using planner_server_package.Events.Enums;
using planner_server_package.RabbitMQ;
using Swashbuckle.AspNetCore.Annotations;
using System.IO.Compression;
using System.Security.Cryptography;

namespace planner_file_service.Api.Controllers
{
    [ApiController]
    [Route("api/design")]
    public class DesignController : ControllerBase
    {
        private readonly IFileUploaderService _fileUploaderService;
        private readonly IPublisherService _publisherService;
        private readonly IJwtService _jwtService;

        private readonly string _storagePath;

        public DesignController(
            IFileUploaderService fileUploaderService,
            IPublisherService notifyService,
            IJwtService jwtService)
        {
            _fileUploaderService = fileUploaderService;
            _publisherService = notifyService;
            _jwtService = jwtService;

            _storagePath = Constants.LocalPathToStorages;
        }

        [HttpGet("themes/metadata")]
        [SwaggerOperation(Summary = "Получение метаданных тем", Description = "Возвращает список всех тем с прямыми ссылками и хэшами")]
        [SwaggerResponse(StatusCodes.Status200OK, "Метаданные получены", Type = typeof(ThemesMetadataResponse))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Темы не найдены")]
        public async Task<ActionResult<ThemesMetadataResponse>> GetThemesMetadata()
        {
            if (!Directory.Exists(_storagePath))
                return NotFound(new { error = "Storage directory not found" });

            Console.WriteLine(_storagePath);

            // Ищем все файлы тем
            var themeFiles = Directory.GetFiles(_storagePath, "theme-*.flat.json");

            if (themeFiles.Length == 0)
                return NotFound(new { error = "No theme files found" });

            var filesMetadata = new List<FileMetadata>();
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

            foreach (var filePath in themeFiles)
            {
                var fileName = Path.GetFileName(filePath);
                var fileInfo = new FileInfo(filePath);

                // Вычисляем MD5 хэш файла
                string md5Hash = await CalculateMD5Async(filePath);

                filesMetadata.Add(new FileMetadata(
                    Id: Path.GetFileNameWithoutExtension(fileName).Replace("theme-", ""),
                    Name: fileName,
                    Url: $"{baseUrl}/api/themes/download/{Uri.EscapeDataString(fileName)}",
                    Md5: md5Hash,
                    Size: fileInfo.Length,
                    LastModified: fileInfo.LastWriteTimeUtc
                ));
            }

            return Ok(new ThemesMetadataResponse(
                TotalFiles: filesMetadata.Count,
                Files: filesMetadata,
                GeneratedAt: DateTime.UtcNow,
                Version: "1.0.0"
            ));
        }

        [HttpGet("download/{fileName}")]
        [SwaggerOperation(Summary = "Скачивание темы", Description = "Скачивает конкретный файл темы")]
        [SwaggerResponse(StatusCodes.Status200OK, "Файл скачан", Type = typeof(FileStreamResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Файл не найден")]
        public async Task<IActionResult> DownloadTheme(string fileName)
        {
            // Защита от path traversal атак
            fileName = Path.GetFileName(fileName);
            var filePath = Path.Combine(_storagePath, fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound(new { error = "File not found" });

            var fileInfo = new FileInfo(filePath);

            // Поддержка частичной загрузки (Range)
            Response.Headers.Add("Accept-Ranges", "bytes");
            Response.Headers.Add("Content-MD5", Convert.ToBase64String(await CalculateMD5BytesAsync(filePath)));

            return File(
                System.IO.File.OpenRead(filePath),
                "application/json",
                fileName,
                true // enable range processing
            );
        }

        // Скачивание всех тем одним архивом (дополнительно)
        [HttpGet("download-all")]
        [SwaggerOperation(Summary = "Скачивание всех тем архивом", Description = "Скачивает все темы одним ZIP архивом")]
        [SwaggerResponse(StatusCodes.Status200OK, "Архив скачан", Type = typeof(FileStreamResult))]
        public async Task<IActionResult> DownloadAllThemes()
        {
            var themeFiles = Directory.GetFiles(_storagePath, "theme-*.flat.json");

            if (themeFiles.Length == 0)
                return NotFound(new { error = "No theme files found" });

            var memoryStream = new MemoryStream();

            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var filePath in themeFiles)
                {
                    var fileName = Path.GetFileName(filePath);
                    var entry = archive.CreateEntry(fileName);

                    using (var entryStream = entry.Open())
                    using (var fileStream = System.IO.File.OpenRead(filePath))
                    {
                        await fileStream.CopyToAsync(entryStream);
                    }
                }
            }

            memoryStream.Position = 0;
            string filename = $"themes_{DateTime.Now:yyyyMMdd_HHmmss}.zip";

            return File(memoryStream, "application/zip", filename);
        }

        private async Task<string> CalculateMD5Async(string filePath)
        {
            using (var md5 = MD5.Create())
            using (var stream = System.IO.File.OpenRead(filePath))
            {
                var hash = await md5.ComputeHashAsync(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        private async Task<byte[]> CalculateMD5BytesAsync(string filePath)
        {
            using (var md5 = MD5.Create())
            using (var stream = System.IO.File.OpenRead(filePath))
            {
                return await md5.ComputeHashAsync(stream);
            }
        }
    }
}