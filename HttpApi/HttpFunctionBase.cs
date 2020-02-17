using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Net.Http.Headers;

namespace HttpApi
{
    public abstract class HttpFunctionBase
    {
        protected HttpFunctionBase(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private readonly IHttpContextAccessor _httpContextAccessor;

        protected HttpContext HttpContext => _httpContextAccessor.HttpContext;
        protected HttpRequest Request => HttpContext?.Request;
        protected HttpResponse Response => HttpContext?.Response;
        protected ClaimsPrincipal User => HttpContext?.User;
        protected ModelStateDictionary ModelState { get; } = new ModelStateDictionary();

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
        protected ContentResult Content(string content, MediaTypeHeaderValue contentType) => new ContentResult { Content = content, ContentType = contentType?.ToString() };

        protected NoContentResult NoContent() => new NoContentResult();

        protected OkResult Ok() => new OkResult();
        protected OkObjectResult Ok(object value) => new OkObjectResult(value);

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

        protected ForbidResult Forbid() => new ForbidResult();

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