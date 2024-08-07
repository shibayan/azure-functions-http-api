﻿using System;
using System.IO;
using System.Net.Http;
using System.Security.Claims;
using System.Text;

using Azure.WebJobs.Extensions.HttpApi.Internal;

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

namespace Azure.WebJobs.Extensions.HttpApi.Core;

public abstract class HttpFunctionImpl(IHttpContextAccessor httpContextAccessor)
{
    private IUrlHelper _url;
    private IObjectModelValidator _objectModelValidator;
    private ProblemDetailsFactory _problemDetailsFactory;

    private const string DefaultContentType = "application/octet-stream";

    private static readonly PhysicalFileProvider s_fileProvider = new(FunctionAppEnvironment.RootPath);
    private static readonly FileExtensionContentTypeProvider s_contentTypeProvider = new();

    protected HttpContext HttpContext => httpContextAccessor.HttpContext;
    protected HttpRequest Request => HttpContext?.Request;
    protected HttpResponse Response => HttpContext?.Response;
    protected ClaimsPrincipal User => HttpContext?.User;
    protected ModelStateDictionary ModelState { get; } = new();
    protected bool IsAuthenticationEnabled => FunctionAppEnvironment.IsAuthenticationEnabled;

    protected IUrlHelper Url
    {
        get
        {
            if (_url is null)
            {
                var factory = HttpContext?.RequestServices.GetRequiredService<IUrlHelperFactory>();

                _url = factory?.GetUrlHelper(new ActionContext(HttpContext, HttpContext.GetRouteData(), new ActionDescriptor()));
            }

            return _url;
        }
    }

    protected IObjectModelValidator ObjectValidator
        => _objectModelValidator ??= HttpContext?.RequestServices.GetRequiredService<IObjectModelValidator>();

    protected ProblemDetailsFactory ProblemDetailsFactory
        => _problemDetailsFactory ??= HttpContext?.RequestServices.GetRequiredService<ProblemDetailsFactory>();

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
        => File(virtualPath, s_contentTypeProvider.TryGetContentType(virtualPath, out var contentType) ? contentType : DefaultContentType);

    protected VirtualFileResult File(string virtualPath, string contentType)
        => File(virtualPath, contentType, null);

    protected VirtualFileResult File(string virtualPath, string contentType, string fileDownloadName)
        => new(virtualPath, contentType) { FileDownloadName = fileDownloadName, FileProvider = s_fileProvider };

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

    protected BadRequestObjectResult ValidationProblem(ValidationProblemDetails descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        return new BadRequestObjectResult(descriptor);
    }

    protected ObjectResult ValidationProblem(ModelStateDictionary modelState) => ValidationProblem(modelStateDictionary: modelState);

    protected ObjectResult ValidationProblem(string detail = null, string instance = null, int? statusCode = null, string title = null, string type = null, ModelStateDictionary modelStateDictionary = null)
    {
        var validationProblem = ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, modelStateDictionary ?? ModelState, statusCode, title, type, detail, instance);

        return new ObjectResult(validationProblem) { StatusCode = validationProblem.Status };
    }

    protected CreatedResult Created(string uri, object value)
    {
        ArgumentNullException.ThrowIfNull(uri);

        return new CreatedResult(uri, value);
    }

    protected CreatedResult Created(Uri uri, object value)
    {
        ArgumentNullException.ThrowIfNull(uri);

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
        ArgumentNullException.ThrowIfNull(uri);

        return new AcceptedResult(uri, null);
    }

    protected AcceptedResult Accepted(string uri) => new(uri, null);

    protected AcceptedResult Accepted(Uri uri, object value)
    {
        ArgumentNullException.ThrowIfNull(uri);

        return new AcceptedResult(uri, value);
    }

    protected AcceptedResult Accepted(string uri, object value) => new(uri, value);

    protected AcceptedResult AcceptedAtFunction(string functionName) => AcceptedAtFunction(functionName, null, null);
    protected AcceptedResult AcceptedAtFunction(string functionName, object value) => AcceptedAtFunction(functionName, null, value);

    protected AcceptedResult AcceptedAtFunction(string functionName, object routeValues, object value)
        => Accepted(Url.Link(functionName, routeValues), value);

    protected StatusCodeResult Forbid() => StatusCode(StatusCodes.Status403Forbidden);
    protected ObjectResult Forbid(object value) => StatusCode(StatusCodes.Status403Forbidden, value);

    protected ProxyResult Proxy(string backendUri, Action<HttpRequestMessage> beforeSend = null, Action<HttpResponseMessage> afterSend = null)
    {
        ArgumentNullException.ThrowIfNull(backendUri);

        return new ProxyResult(backendUri) { BeforeSend = beforeSend, AfterSend = afterSend };
    }

    protected RemoteStaticAppResult RemoteStaticApp(string backendUri, string fallbackExclude = null)
    {
        ArgumentNullException.ThrowIfNull(backendUri);

        return new RemoteStaticAppResult(backendUri) { FallbackExclude = fallbackExclude };
    }

    protected LocalStaticAppResult LocalStaticApp(string defaultFile = "index.html", string fallbackPath = "404.html", string fallbackExclude = null)
    {
        return new LocalStaticAppResult { DefaultFile = defaultFile, FallbackPath = fallbackPath, FallbackExclude = fallbackExclude };
    }

    #endregion

    protected bool TryValidateModel(object model)
    {
        var actionContext = new ActionContext(HttpContext, HttpContext.GetRouteData(), new ActionDescriptor(), ModelState);

        ObjectValidator.Validate(actionContext, null, string.Empty, model);

        return ModelState.IsValid;
    }
}
