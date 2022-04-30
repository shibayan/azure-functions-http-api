using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Azure.WebJobs.Extensions.HttpApi.Internal;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

namespace Azure.WebJobs.Extensions.HttpApi.Core
{
    internal class LocalStaticAppResultExecutor : IActionResultExecutor<LocalStaticAppResult>
    {
        private const string DefaultContentType = "application/octet-stream";

        private static readonly PhysicalFileProvider s_fileProvider = new(Path.Combine(FunctionEnvironment.RootPath, "wwwroot"));
        private static readonly FileExtensionContentTypeProvider s_contentTypeProvider = new();

        public async Task ExecuteAsync(ActionContext context, LocalStaticAppResult result)
        {
            var (_, value) = context.RouteData.Values.Single();

            var virtualPath = $"/{value}";

            var contents = s_fileProvider.GetDirectoryContents(virtualPath);

            if (contents.Exists)
            {
                virtualPath += virtualPath.EndsWith("/") ? result.DefaultFile : $"/{result.DefaultFile}";
            }

            var response = context.HttpContext.Response;

            var fileInfo = GetFileInformation(virtualPath, result);

            if (!fileInfo.Exists)
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;

                return;
            }

            SetResponseHeaders(response, fileInfo);

            if (!HttpMethods.IsHead(context.HttpContext.Request.Method))
            {
                await response.SendFileAsync(fileInfo, context.HttpContext.RequestAborted);
            }
        }

        private void SetResponseHeaders(HttpResponse response, IFileInfo fileInfo)
        {
            response.ContentType = s_contentTypeProvider.TryGetContentType(fileInfo.Name, out var contentType) ? contentType : DefaultContentType;
            response.ContentLength = fileInfo.Length;

            var typedHeaders = response.GetTypedHeaders();

            typedHeaders.LastModified = fileInfo.LastModified;
        }

        private IFileInfo GetFileInformation(string virtualPath, LocalStaticAppResult result)
        {
            var fileInfo = s_fileProvider.GetFileInfo(virtualPath);

            if (!fileInfo.Exists)
            {
                // Try Fallback
                if (!string.IsNullOrEmpty(result.FallbackPath) && (string.IsNullOrEmpty(result.FallbackExclude) || !Regex.IsMatch(virtualPath, result.FallbackExclude)))
                {
                    return s_fileProvider.GetFileInfo(result.FallbackPath);
                }
            }

            return fileInfo;
        }
    }
}
