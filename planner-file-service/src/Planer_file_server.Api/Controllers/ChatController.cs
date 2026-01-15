using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planer_file_server.Core;
using Planer_file_server.Core.Enums;
using Planer_file_server.Core.IService;
using Swashbuckle.AspNetCore.Annotations;

namespace Planer_file_server.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class ChatController : ControllerBase
    {
        private readonly IFileUploaderService _fileUploaderService;
        private readonly INotifyService _notifyService;
        private readonly IJwtService _jwtService;
        private readonly string[] _supportedImageExtensions = new string[]
        {
            "gif",
            "jpg",
            "jpeg",
            "jfif",
            "png",
            "svg"
        };

        public ChatController(
            IFileUploaderService fileUploaderService,
            INotifyService notifyService,
            IJwtService jwtService)
        {
            _fileUploaderService = fileUploaderService;
            _notifyService = notifyService;
            _jwtService = jwtService;
        }

        [HttpPost("chat/upload"), Authorize]
        [SwaggerOperation(Summary = "Загрузка файла чата", Description = "Загрузка файла чата")]
        [SwaggerResponse(StatusCodes.Status200OK, "Файл чата загружен")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректные данные")]
        [SwaggerResponse(StatusCodes.Status415UnsupportedMediaType, "Некорректный формат файла")]
        public async Task<IActionResult> UploadAttachment(
            [FromHeader(Name = "Authorization")] string token,
            [Required, FromQuery] Guid chatId,
            [SwaggerParameter(Description = "Файл изображения", Required = true)] IFormFile file)
        {
            var accountId = _jwtService.GetTokenPayload(token).AccountId;
            var uploadResult = await _fileUploaderService.UploadFileAsync(Constants.LocalPathToStorages, file.OpenReadStream(), _supportedImageExtensions);
            if (!uploadResult.IsSuccess)
                return StatusCode((int)uploadResult.StatusCode);

            var body = new
            {
                ChatId = chatId,
                FileName = uploadResult.Body,
                AccountId = accountId
            };
            _notifyService.Publish(body, ContentUploaded.ChatAttachment);
            return Ok();
        }

        [HttpGet("chat/download/{filename}")]
        [SwaggerOperation(Summary = "Скачивание файла чата", Description = "Скачивание файла чата")]
        [SwaggerResponse(StatusCodes.Status200OK, "Файл чата скачан", Type = typeof(FileStreamResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Файл чата не найден")]
        public async Task<IActionResult> DownloadAttachment(string filename)
        {
            var response = await _fileUploaderService.GetStreamAsync(Constants.LocalPathToStorages, filename);
            if (!response.IsSuccess)
                return StatusCode((int)response.StatusCode);

            return File(response.Body, "application/octet-stream", filename);
        }

        [HttpPost("chatIcon/upload"), Authorize]
        [SwaggerOperation(Summary = "Загрузка иконки чата", Description = "Загрузка иконки чата")]
        [SwaggerResponse(StatusCodes.Status200OK, "Иконка чата загружена")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректные данные")]
        [SwaggerResponse(StatusCodes.Status415UnsupportedMediaType, "Некорректный формат файла")]
        public async Task<IActionResult> UploadIcon(
            [Required, FromQuery] Guid chatId,
            [SwaggerParameter(Description = "Файл изображения", Required = true)] IFormFile file)
        {
            var uploadResult = await _fileUploaderService.UploadFileAsync(Constants.LocalPathToStorages, file.OpenReadStream(), _supportedImageExtensions);
            if (!uploadResult.IsSuccess)
                return StatusCode((int)uploadResult.StatusCode);

            var body = new
            {
                ChatId = chatId,
                FileName = uploadResult.Body
            };
            _notifyService.Publish(body, ContentUploaded.ChatImage);
            return Ok();
        }


        [HttpGet("chatIcon/download/{filename}")]
        [SwaggerOperation(Summary = "Скачивание иконки чата", Description = "Скачивание иконки чата")]
        [SwaggerResponse(StatusCodes.Status200OK, "Иконка чата скачана", Type = typeof(FileStreamResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Иконка чата не найдена")]
        public async Task<IActionResult> DownloadIcon(string filename)
        {
            var response = await _fileUploaderService.GetStreamAsync(Constants.LocalPathToStorages, filename);
            if (!response.IsSuccess)
                return StatusCode((int)response.StatusCode);

            return File(response.Body, "application/octet-stream", filename);
        }
    }
}