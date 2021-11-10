using System;
using System.IO;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

using Azure.WebJobs.Extensions.HttpApi.Internal;
using Azure.WebJobs.Extensions.HttpApi.Proxy;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;

namespace Azure.WebJobs.Extensions.HttpApi
{
    public abstract class HttpFunctionBase
    {
        protected HttpFunctionBase(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private readonly IHttpContextAccessor _httpContextAccessor;

        private IUrlHelper _url;
        private IObjectModelValidator _objectModelValidator;
        private ProblemDetailsFactory _problemDetailsFactory;

        private const string DefaultContentType = "application/octet-stream";

        private static readonly PhysicalFileProvider _fileProvider = new(FunctionEnvironment.RootPath);
        private static readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

        protected HttpContext HttpContext => _httpContextAccessor.HttpContext;
        protected HttpRequest Request => HttpContext?.Request;
        protected HttpResponse Response => HttpContext?.Response;
        protected ClaimsPrincipal User => HttpContext?.User;
        protected ModelStateDictionary ModelState { get; } = new ModelStateDictionary();

        protected IUrlHelper Url
        {
            get
            {
                if (_url is null)
                {
                    var factory = HttpContext?.RequestServices?.GetRequiredService<IUrlHelperFactory>();

                    _url = factory?.GetUrlHelper(new ActionContext(HttpContext, HttpContext.GetRouteData(), new ActionDescriptor()));
                }

                return _url;
            }
        }

        protected IObjectModelValidator ObjectValidator
            => _objectModelValidator ??= HttpContext?.RequestServices?.GetRequiredService<IObjectModelValidator>();

        protected ProblemDetailsFactory ProblemDetailsFactory
            => _problemDetailsFactory ??= HttpContext?.RequestServices?.GetRequiredService<ProblemDetailsFactory>();

        #region IActionResult helpers

        protected StatusCodeResult StatusCode(int statusCode) => new(statusCode);
        protected ObjectResult StatusCode(int statusCode, object value) => new(value) { StatusCode = statusCode };

        protected ContentResult Content(string content) => Content(content, (MediaTypeHeaderValue)null);
        protected ContentResult Content(string content, string contentType) => Content(content, MediaTypeHeaderValue.Parse(contentType));

        protected ContentResult Content(string content, string contentType, Encoding contentEncoding)
        {
            var mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(contentType);
            mediaTypeHeaderValue.Encoding = contentEncoding ?? mediaTypeHeaderValue.Encoding;
            return Content(content, mediaTypeHeaderValue);
        }

        protected ContentResult Content(string content, MediaTypeHeaderValue contentType)
            => new() { Content = content, ContentType = contentType?.ToString() };

        protected NoContentResult NoContent() => new();

        protected OkResult Ok() => new();
        protected OkObjectResult Ok(object value) => new(value);

        protected FileContentResult File(byte[] fileContents, string contentType)
            => File(fileContents, contentType, null);

        protected FileContentResult File(byte[] fileContents, string contentType, string fileDownloadName)
            => new(fileContents, contentType) { FileDownloadName = fileDownloadName };

        protected FileStreamResult File(Stream fileStream, string contentType)
            => File(fileStream, contentType, null);

        protected FileStreamResult File(Stream fileStream, string contentType, string fileDownloadName)
            => new(fileStream, contentType) { FileDownloadName = fileDownloadName };

        protected VirtualFileResult File(string virtualPath)
            => File(virtualPath, _contentTypeProvider.TryGetContentType(virtualPath, out var contentType) ? contentType : DefaultContentType);

        protected VirtualFileResult File(string virtualPath, string contentType)
            => File(virtualPath, contentType, null);

        protected VirtualFileResult File(string virtualPath, string contentType, string fileDownloadName)
            => new(virtualPath, contentType) { FileDownloadName = fileDownloadName, FileProvider = _fileProvider };

        protected UnauthorizedResult Unauthorized() => new();
        protected UnauthorizedObjectResult Unauthorized(object value) => new(value);

        protected NotFoundResult NotFound() => new();
        protected NotFoundObjectResult NotFound(object value) => new(value);

        protected BadRequestResult BadRequest() => new();
        protected BadRequestObjectResult BadRequest(object error) => new(error);
        protected BadRequestObjectResult BadRequest(ModelStateDictionary modelState) => new(modelState);

        protected ConflictResult Conflict() => new();
        protected ConflictObjectResult Conflict(object error) => new(error);
        protected ConflictObjectResult Conflict(ModelStateDictionary modelState) => new(modelState);

        protected ObjectResult Problem(string detail = null, string instance = null, int? statusCode = null, string title = null, string type = null)
        {
            var problemDetails = ProblemDetailsFactory.CreateProblemDetails(HttpContext, statusCode ?? 500, title, type, detail, instance);

            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        protected ActionResult ValidationProblem(ValidationProblemDetails descriptor)
        {
            if (descriptor is null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            return new BadRequestObjectResult(descriptor);
        }

        protected ActionResult ValidationProblem(ModelStateDictionary modelState) => ValidationProblem(modelStateDictionary: modelState);

        protected ObjectResult ValidationProblem(string detail = null, string instance = null, int? statusCode = null, string title = null, string type = null, ModelStateDictionary modelStateDictionary = null)
        {
            var validationProblem = ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, modelStateDictionary ?? ModelState, statusCode, title, type, detail, instance);

            return new ObjectResult(validationProblem) { StatusCode = validationProblem.Status };
        }

        protected CreatedResult Created(string uri, object value)
        {
            if (uri is null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            return new CreatedResult(uri, value);
        }

        protected CreatedResult Created(Uri uri, object value)
        {
            if (uri is null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            return new CreatedResult(uri, value);
        }

        protected CreatedResult CreatedAtFunction(string functionName) => CreatedAtFunction(functionName, null, null);
        protected CreatedResult CreatedAtFunction(string functionName, object value) => CreatedAtFunction(functionName, null, value);

        protected CreatedResult CreatedAtFunction(string functionName, object routeValues, object value)
            => Created(Url.Link(functionName, routeValues), value);

        protected AcceptedResult Accepted() => new();
        protected AcceptedResult Accepted(object value) => new(location: null, value);

        protected AcceptedResult Accepted(Uri uri)
        {
            if (uri is null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            return new AcceptedResult(uri, null);
        }

        protected AcceptedResult Accepted(string uri) => new(uri, null);

        protected AcceptedResult Accepted(Uri uri, object value)
        {
            if (uri is null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            return new AcceptedResult(uri, value);
        }

        protected AcceptedResult Accepted(string uri, object value) => new(uri, value);

        protected AcceptedResult AcceptedAtFunction(string functionName) => AcceptedAtFunction(functionName, null, null);
        protected AcceptedResult AcceptedAtFunction(string functionName, object value) => AcceptedAtFunction(functionName, null, value);

        protected AcceptedResult AcceptedAtFunction(string functionName, object routeValues, object value)
            => Accepted(Url.Link(functionName, routeValues), value);

        protected StatusCodeResult Forbid() => StatusCode(StatusCodes.Status403Forbidden);
        protected ObjectResult Forbid(object value) => StatusCode(StatusCodes.Status403Forbidden, value);

        protected IActionResult Proxy(string backendUri, Action<HttpRequestMessage> before = null, Action<HttpResponseMessage> after = null)
        {
            if (backendUri is null)
            {
                throw new ArgumentNullException(nameof(backendUri));
            }

            return new ProxyResult(backendUri) { Before = before, After = after };
        }

        protected IActionResult ProxySpa(string backendUri, string fallbackExclude = null)
        {
            if (backendUri is null)
            {
                throw new ArgumentNullException(nameof(backendUri));
            }

            return new ProxySpaResult(backendUri) { FallbackExclude = fallbackExclude };
        }

        protected IActionResult ServeSpa(string virtualPath, string contentRoot = "wwwroot", string defaultFile = "index.html", string fallbackPath = "404.html", string fallbackExclude = null)
        {
            if (contentRoot is null)
            {
                throw new ArgumentNullException(nameof(contentRoot));
            }

            if (virtualPath is null)
            {
                virtualPath = "/";
            }
            else if (!virtualPath.StartsWith("/"))
            {
                virtualPath = "/" + virtualPath;
            }

            var subDir = _fileProvider.GetDirectoryContents(contentRoot + virtualPath);

            if (subDir.Exists)
            {
                virtualPath += defaultFile;
            }

            var fileInfo = _fileProvider.GetFileInfo(contentRoot + virtualPath);

            if (!fileInfo.Exists)
            {
                if (!string.IsNullOrEmpty(fallbackPath) && (string.IsNullOrEmpty(fallbackExclude) || !Regex.IsMatch(virtualPath, fallbackExclude)))
                {
                    virtualPath = "/" + fallbackPath;

                    var fallbackFileInfo = _fileProvider.GetFileInfo(contentRoot + virtualPath);

                    if (!fallbackFileInfo.Exists)
                    {
                        return NotFound();
                    }
                }
                else
                {
                    return NotFound();
                }
            }

            if (!_contentTypeProvider.TryGetContentType(contentRoot + virtualPath, out var contentType))
            {
                contentType = DefaultContentType;
            }

            return new VirtualFileResult(contentRoot + virtualPath, contentType) { FileProvider = _fileProvider };
        }

        #endregion

        protected bool TryValidateModel(object model)
        {
            var actionContext = new ActionContext(HttpContext, HttpContext.GetRouteData(), new ActionDescriptor(), ModelState);

            ObjectValidator.Validate(actionContext, null, string.Empty, model);

            return ModelState.IsValid;
        }
    }
}
