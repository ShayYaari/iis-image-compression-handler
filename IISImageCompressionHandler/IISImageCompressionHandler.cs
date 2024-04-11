using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web;

namespace IISImageCompressionHandler
{
    public class IISImageCompressionHandler : IHttpHandler
    {
        private const string IMAGE_QUALITY_HEADER_NAME = "Image-Quality";
        private readonly string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

        public bool IsReusable { get { return true; } }

        public void ProcessRequest(HttpContext context)
        {
            HttpRequest request = context.Request;

            string requestUrl = request.Url.AbsolutePath;
            if (!IsImageAsset(requestUrl))
            {
                SetNotFoundResponse(context);
                return;
            }

            string rootDirectory = HttpContext.Current.Server.MapPath("~");
            string relativePath = request.Path.TrimStart('/');

            string imagePath = Path.Combine(rootDirectory, relativePath.Replace('/', '\\'));
            if (!File.Exists(imagePath))
            {
                if (relativePath.IndexOf("/") < 0)
                {
                    SetNotFoundResponse(context);
                    return;
                }

                relativePath = relativePath.Substring(relativePath.IndexOf("/") + 1);
                imagePath = Path.Combine(rootDirectory, relativePath.Replace('/', '\\'));
                if (!File.Exists(imagePath))
                {
                    SetNotFoundResponse(context);
                    return;
                }
            }

            foreach (string key in request.Headers.AllKeys)
            {
                if (!key.Equals(IMAGE_QUALITY_HEADER_NAME))
                {
                    continue;
                }

                string quality = context.Request.Headers.Get(key);

                try
                {
                    int result = int.Parse(quality);
                }
                catch (FormatException)
                {
                    SetNotFoundResponse(context);
                }

                string compressedTempFilePath;
                if (!TryReduceImageQuality(context, imagePath, quality, out compressedTempFilePath))
                {
                    SetNotFoundResponse(context);
                    if (compressedTempFilePath.Length > 0)
                    {
                        File.Delete(compressedTempFilePath);
                    }
                    return;
                }

                context.Response.ContentType = MimeMapping.GetMimeMapping(imagePath);
                context.Response.BinaryWrite(File.ReadAllBytes(compressedTempFilePath));

                File.Delete(compressedTempFilePath);
                return;
            }

            context.Response.ContentType = MimeMapping.GetMimeMapping(imagePath);
            context.Response.WriteFile(imagePath);
            return;
        }

        private bool TryReduceImageQuality(HttpContext context, string imagePath, string quality, out string compressedTempFilePath)
        {
            compressedTempFilePath = Path.GetTempFileName();

            string magickCommand = $"magick convert {imagePath} -quality {quality} {compressedTempFilePath}";

            var processInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = $"/c {magickCommand}",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true
            };
            Process process = new Process
            {
                StartInfo = processInfo
            };

            StringBuilder stdErr = new StringBuilder();
            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    stdErr.AppendLine(e.Data);
                }
            };
            process.Start();
            process.BeginErrorReadLine();
            process.WaitForExit();

            string stdErrMessage = stdErr.ToString();
            if (stdErrMessage.Length > 0)
            {
                context.Response.AppendToLog($"IISImageCompressionHandler ERROR: Failed to reduce image size. CMD: {magickCommand}");
            }
            return stdErrMessage.Length < 1;
        }

        private void SetNotFoundResponse(HttpContext context)
        {
            context.Response.StatusCode = 404;
            context.Response.StatusDescription = "Not Found";
        }

        private bool IsImageAsset(string url)
        {
            foreach (var extension in this.imageExtensions)
            {
                if (url.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
