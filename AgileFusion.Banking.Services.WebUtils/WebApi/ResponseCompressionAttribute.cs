using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using IDS.Logging;

namespace AgileFusion.Banking.Services.WebUtils.WebApi
{
    /// <summary>
    /// Web api response compression attribute.
    /// </summary>
    public class ResponseCompressionAttribute : ActionFilterAttribute
    {
        private const string ContentEncodingHeader = "Content-Encoding";
        private const string JsonContentType = "application/json";
        private static readonly LogHelper Logger = new LogHelper(LogSystem.CreateTypeContextLogger());
        private static readonly List<IMessageCompressor> Compressors = new List<IMessageCompressor> { new GzipCompressor(), new DeflateCompressor() };

        /// <inheritdoc />
        public override async Task OnActionExecutedAsync(HttpActionExecutedContext actionContext, CancellationToken cancellationToken)
        {
            var acceptEncoding = actionContext.Request.Headers.AcceptEncoding.FirstOrDefault();
            var transferEncoding = actionContext.Response.Headers.TransferEncoding.FirstOrDefault();
            if (acceptEncoding != null && transferEncoding == null)
            {
                var compressor = Compressors.FirstOrDefault(c => c.ContentEncoding.Equals(acceptEncoding.Value, StringComparison.InvariantCultureIgnoreCase));
                if (compressor != null)
                {
                    var content = actionContext.Response.Content as ObjectContent;
                    if (content != null)
                    {
                        var contentType = content.Headers?.ContentType?.MediaType;
                        if (string.IsNullOrEmpty(contentType))
                            contentType = JsonContentType;

                        var bytes = await content.ReadAsByteArrayAsync();
                        var zlibbedContent = bytes != null ? DeflateByte(bytes, compressor) : new byte[0];

                        actionContext.Response = new HttpResponseMessage(actionContext.Response.StatusCode)
                        {
                            Content = new ByteArrayContent(zlibbedContent)
                        };

                        actionContext.Response.Content.Headers.ContentEncoding.Add(compressor.ContentEncoding);
                        actionContext.Response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                        actionContext.Response.Headers.Add("Vary", ContentEncodingHeader);
                    }
                }
            }

            await base.OnActionExecutedAsync(actionContext, cancellationToken);
        }

        private static byte[] DeflateByte(byte[] str, IMessageCompressor compressor)
        {
            using (var output = new MemoryStream())
            {
                using (var compressStream = compressor.CreateCompressor(output))
                {
                    compressStream.Write(str, 0, str.Length);
                }

                return output.ToArray();
            }
        }

        private interface IMessageCompressor
        {
            string ContentEncoding { get; }

            Stream CreateCompressor(Stream output);
        }

        private class GzipCompressor : IMessageCompressor
        {
            private const string ContentType = "gzip";

            public string ContentEncoding => ContentType;

            public Stream CreateCompressor(Stream output)
            {
                return new Ionic.Zlib.GZipStream(output, Ionic.Zlib.CompressionMode.Compress, Ionic.Zlib.CompressionLevel.BestSpeed);
            }
        }

        private class DeflateCompressor : IMessageCompressor
        {
            private const string ContentType = "deflate";

            public string ContentEncoding => ContentType;

            public Stream CreateCompressor(Stream output)
            {
                return new Ionic.Zlib.DeflateStream(output, Ionic.Zlib.CompressionMode.Compress, Ionic.Zlib.CompressionLevel.BestSpeed);
            }
        }
    }
}
