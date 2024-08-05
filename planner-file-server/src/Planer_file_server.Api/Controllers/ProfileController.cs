using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planer_file_server.Core;
using Planer_file_server.Core.Enums;
using Planer_file_server.Core.IService;
using Swashbuckle.AspNetCore.Annotations;

namespace Planer_file_server.Api.Controllers
{
    [ApiController]
    [Route("api/profile")]
    public class ProfileController : ControllerBase
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

        public ProfileController(
            IFileUploaderService fileUploaderService,
            INotifyService notifyService,
            IJwtService jwtService)
        {
            _fileUploaderService = fileUploaderService;
            _notifyService = notifyService;
            _jwtService = jwtService;
        }

        [HttpPost("upload"), Authorize]
        [SwaggerOperation(Summary = "Загрузка изображения профиля", Description = "Загрузка изображения профиля")]
        [SwaggerResponse(StatusCodes.Status200OK, "Изображение профиля загружено")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Пользователь не авторизован")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Некорректные данные")]
        [SwaggerResponse(StatusCodes.Status415UnsupportedMediaType, "Некорректный формат файла")]
        public async Task<IActionResult> Upload(
            [SwaggerParameter(Description = "Файл изображения", Required = true)] IFormFile file,
            [FromHeader(Name = "Authorization")] string token)
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var uploadResult = await _fileUploaderService.UploadFileAsync(Constants.LocalPathToStorages, file.OpenReadStream(), _supportedImageExtensions);
            if (!uploadResult.IsSuccess)
                return StatusCode((int)uploadResult.StatusCode);

            var body = new
            {
                tokenPayload.AccountId,
                FileName = uploadResult.Body
            };
            _notifyService.Publish(body, ContentUploaded.ProfileImage);
            return Ok();
        }

        [HttpGet("download/{filename}")]
        [SwaggerOperation(Summary = "Скачивание изображения профиля", Description = "Скачивание изображения профиля")]
        [SwaggerResponse(StatusCodes.Status200OK, "Изображение профиля скачано", Type = typeof(FileStreamResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Изображение профиля не найдено")]
        public async Task<IActionResult> Download(string filename)
        {
            var response = await _fileUploaderService.GetStreamAsync(Constants.LocalPathToStorages, filename);
            if (!response.IsSuccess)
                return StatusCode((int)response.StatusCode);

            return File(response.Body, "application/octet-stream", filename);
        }
    }
}