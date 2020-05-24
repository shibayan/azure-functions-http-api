using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
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
        private ProblemDetailsFactory _problemDetailsFactory;

        private static readonly IFileProvider _functionFileProvider = new PhysicalFileProvider(FunctionEnvironment.RootPath);

        protected HttpContext HttpContext => _httpContextAccessor.HttpContext;
        protected HttpRequest Request => HttpContext?.Request;
        protected HttpResponse Response => HttpContext?.Response;
        protected ClaimsPrincipal User => HttpContext?.User;
        protected ModelStateDictionary ModelState { get; } = new ModelStateDictionary();

        protected IUrlHelper Url
        {
            get
            {
                if (_url == null)
                {
                    var factory = HttpContext?.RequestServices?.GetRequiredService<IUrlHelperFactory>();

                    _url = factory?.GetUrlHelper(new ActionContext(HttpContext, HttpContext.GetRouteData(), new ActionDescriptor()));
                }

                return _url;
            }
        }
        public ProblemDetailsFactory ProblemDetailsFactory
            => _problemDetailsFactory ??= HttpContext?.RequestServices?.GetRequiredService<ProblemDetailsFactory>();

        #region IActionResult helpers

        protected StatusCodeResult StatusCode(int statusCode) => new StatusCodeResult(statusCode);
        protected ObjectResult StatusCode(int statusCode, object value) => new ObjectResult(value) { StatusCode = statusCode };

        protected ContentResult Content(string content) => Content(content, (MediaTypeHeaderValue)null);
        protected ContentResult Content(string content, string contentType) => Content(content, MediaTypeHeaderValue.Parse(contentType));

        protected ContentResult Content(string content, string contentType, Encoding contentEncoding)
        {
            var mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(contentType);
            mediaTypeHeaderValue.Encoding = contentEncoding ?? mediaTypeHeaderValue.Encoding;
            return Content(content, mediaTypeHeaderValue);
        }

        protected ContentResult Content(string content, MediaTypeHeaderValue contentType)
            => new ContentResult { Content = content, ContentType = contentType?.ToString() };

        protected NoContentResult NoContent() => new NoContentResult();

        protected OkResult Ok() => new OkResult();
        protected OkObjectResult Ok(object value) => new OkObjectResult(value);

        protected FileContentResult File(byte[] fileContents, string contentType)
            => File(fileContents, contentType, null);

        protected FileContentResult File(byte[] fileContents, string contentType, string fileDownloadName)
            => new FileContentResult(fileContents, contentType) { FileDownloadName = fileDownloadName };

        protected FileStreamResult File(Stream fileStream, string contentType)
            => File(fileStream, contentType, null);

        protected FileStreamResult File(Stream fileStream, string contentType, string fileDownloadName)
            => new FileStreamResult(fileStream, contentType) { FileDownloadName = fileDownloadName };

        protected VirtualFileResult File(string virtualPath, string contentType)
            => File(virtualPath, contentType, null);

        protected VirtualFileResult File(string virtualPath, string contentType, string fileDownloadName)
            => new VirtualFileResult(virtualPath, contentType) { FileDownloadName = fileDownloadName, FileProvider = _functionFileProvider };

        protected UnauthorizedResult Unauthorized() => new UnauthorizedResult();
        protected UnauthorizedObjectResult Unauthorized(object value) => new UnauthorizedObjectResult(value);

        protected NotFoundResult NotFound() => new NotFoundResult();
        protected NotFoundObjectResult NotFound(object value) => new NotFoundObjectResult(value);

        protected BadRequestResult BadRequest() => new BadRequestResult();
        protected BadRequestObjectResult BadRequest(object error) => new BadRequestObjectResult(error);
        protected BadRequestObjectResult BadRequest(ModelStateDictionary modelState) => new BadRequestObjectResult(modelState);

        protected ConflictResult Conflict() => new ConflictResult();
        protected ConflictObjectResult Conflict(object error) => new ConflictObjectResult(error);
        protected ConflictObjectResult Conflict(ModelStateDictionary modelState) => new ConflictObjectResult(modelState);

        protected ObjectResult Problem(string detail = null, string instance = null, int? statusCode = null, string title = null, string type = null)
        {
            var problemDetails = ProblemDetailsFactory.CreateProblemDetails(HttpContext, statusCode ?? 500, title, type, detail, instance);

            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        protected ActionResult ValidationProblem(ValidationProblemDetails descriptor)
        {
            if (descriptor == null)
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
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            return new CreatedResult(uri, value);
        }

        protected CreatedResult Created(Uri uri, object value)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            return new CreatedResult(uri, value);
        }

        protected CreatedResult CreatedAtFunction(string functionName) => CreatedAtFunction(functionName, null, null);
        protected CreatedResult CreatedAtFunction(string functionName, object value) => CreatedAtFunction(functionName, null, value);

        protected CreatedResult CreatedAtFunction(string functionName, object routeValues, object value)
            => Created(Url.Link(functionName, routeValues), value);

        protected AcceptedResult Accepted() => new AcceptedResult();
        protected AcceptedResult Accepted(object value) => new AcceptedResult(location: null, value);

        protected AcceptedResult Accepted(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            return new AcceptedResult(uri, null);
        }

        protected AcceptedResult Accepted(string uri) => new AcceptedResult(uri, null);

        protected AcceptedResult Accepted(Uri uri, object value)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            return new AcceptedResult(uri, value);
        }

        protected AcceptedResult Accepted(string uri, object value) => new AcceptedResult(uri, value);

        protected AcceptedResult AcceptedAtFunction(string functionName) => AcceptedAtFunction(functionName, null, null);
        protected AcceptedResult AcceptedAtFunction(string functionName, object value) => AcceptedAtFunction(functionName, null, value);

        protected AcceptedResult AcceptedAtFunction(string functionName, object routeValues, object value)
            => Accepted(Url.Link(functionName, routeValues), value);

        protected StatusCodeResult Forbid() => StatusCode(StatusCodes.Status403Forbidden);
        protected ObjectResult Forbid(object value) => StatusCode(StatusCodes.Status403Forbidden, value);

        #endregion

        protected bool TryValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();

            Validator.TryValidateObject(model, new ValidationContext(model), validationResults, true);

            foreach (var validationResult in validationResults)
            {
                foreach (var memberName in validationResult.MemberNames)
                {
                    ModelState.AddModelError(memberName, validationResult.ErrorMessage);
                }
            }

            return ModelState.IsValid;
        }
    }
}
