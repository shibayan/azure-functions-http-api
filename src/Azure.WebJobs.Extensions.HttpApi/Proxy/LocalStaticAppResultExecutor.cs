using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Azure.WebJobs.Extensions.HttpApi.Internal;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

namespace Azure.WebJobs.Extensions.HttpApi.Proxy
{
    internal class LocalStaticAppResultExecutor : IActionResultExecutor<LocalStaticAppResult>
    {
        private const string DefaultContentType = "application/octet-stream";

        private static readonly PhysicalFileProvider _fileProvider = new(Path.Combine(FunctionEnvironment.RootPath, "wwwroot"));
        private static readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

        public async Task ExecuteAsync(ActionContext context, LocalStaticAppResult result)
        {
            var virtualPath = result.VirtualPath ?? "/";

            var contents = _fileProvider.GetDirectoryContents(virtualPath);

            if (contents.Exists)
            {
                virtualPath += result.DefaultFile;
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
            response.ContentType = _contentTypeProvider.TryGetContentType(fileInfo.Name, out var contentType) ? contentType : DefaultContentType;
            response.ContentLength = fileInfo.Length;

            var typedHeaders = response.GetTypedHeaders();

            typedHeaders.LastModified = fileInfo.LastModified;
        }

        private IFileInfo GetFileInformation(string virtualPath, LocalStaticAppResult result)
        {
            var fileInfo = _fileProvider.GetFileInfo(virtualPath);

            if (!fileInfo.Exists)
            {
                // Try Fallback
                if (!string.IsNullOrEmpty(result.FallbackPath) && (string.IsNullOrEmpty(result.FallbackExclude) || !Regex.IsMatch(virtualPath, result.FallbackExclude)))
                {
                    return _fileProvider.GetFileInfo(result.FallbackPath);
                }
            }

            return fileInfo;
        }
    }
}
