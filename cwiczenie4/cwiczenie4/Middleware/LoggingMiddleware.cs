using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace cwiczenie3.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext httpContext)
        {

            httpContext.Request.EnableBuffering();
            if (httpContext.Request != null)
            {
                string path = httpContext.Request.Path;
                string method = httpContext.Request.Method;
                string queryString = httpContext.Request.QueryString.ToString();
                string bodyString = "";

                using(var reader = new StreamReader(httpContext.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    bodyString = await reader.ReadToEndAsync();
                    httpContext.Request.Body.Position = 0;
                }

                string logFile = "logs.txt";

                string logText = 
                    "LOG: " + Environment.NewLine +
                    method + Environment.NewLine +
                    path + Environment.NewLine +
                    queryString + Environment.NewLine +
                    bodyString + Environment.NewLine +
                    Environment.NewLine;

                if (File.Exists(logFile))
                {
                    File.AppendAllText(logFile, logText);
                }else
                {
                    File.WriteAllText(logFile, logText);
                }
            }

            if (_next != null) await _next(httpContext);
        }
    }
}