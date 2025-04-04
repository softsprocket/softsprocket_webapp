using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Net.Http;
using System.Diagnostics;
using System.Threading.Tasks;

namespace softsprocket_webapp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly Dictionary<string, string> _contentTypes;
        private readonly List<NetProxy> _netProxies;
        private readonly List<PipeProxy> _pipeProxies;

        public HomeController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;

            // Load content types from the JSON file
            string contentTypesPath = Path.Combine(Directory.GetCurrentDirectory(), "contenttypes.json");
            if (System.IO.File.Exists(contentTypesPath))
            {
                string json = System.IO.File.ReadAllText(contentTypesPath);
                _contentTypes = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            }
            else
            {
                _contentTypes = new Dictionary<string, string>(); // Fallback to an empty dictionary
            }

            // Load proxies from the JSON file
            string proxiesPath = Path.Combine(Directory.GetCurrentDirectory(), "proxies.json");
            if (System.IO.File.Exists(proxiesPath))
            {
                string json = System.IO.File.ReadAllText(proxiesPath);
                var proxies = JsonSerializer.Deserialize<ProxiesConfig>(json);
                _netProxies = proxies.Net;
                _pipeProxies = proxies.Pipe;
            }
            else
            {
                _netProxies = new List<NetProxy>();
                _pipeProxies = new List<PipeProxy>();
            }
        }

        [HttpGet("{*filePath}")]
        public async Task<IActionResult> HandleRequest(string filePath)
        {
            // Check if the request matches a net proxy
            var netProxy = _netProxies.Find(p => filePath.StartsWith(p.Source.TrimStart('/')));
            if (netProxy != null)
            {
                string remainingPath = filePath.Substring(netProxy.Source.TrimStart('/').Length);
                string destinationUrl = $"{netProxy.Destination}/{remainingPath}{Request.QueryString}";

                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(destinationUrl);
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }

            // Check if the request matches a pipe proxy
            var pipeProxy = _pipeProxies.Find(p => filePath.StartsWith(p.Source.TrimStart('/')));
            if (pipeProxy != null)
            {
                var queryArgs = Request.QueryString.HasValue ? Request.QueryString.Value.TrimStart('?') : string.Empty;

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = pipeProxy.Command,
                        Arguments = queryArgs,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                await process.StandardInput.WriteLineAsync(queryArgs);
                process.StandardInput.Close();

                string output = await process.StandardOutput.ReadToEndAsync();
                process.WaitForExit();

                return Content(output, "application/json");
            }

            // Default to serving static files
            return ServeFile(filePath);
        }

        public IActionResult ServeFile(string filePath)
        {
            // Default to "index.html" if no file is specified
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = "index.html";
            }

            // Combine the web root path with the requested file path
            string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, filePath);

            // Check if the file exists
            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound("<h1>File not found</h1>");
            }

            // Determine the content type based on the file extension
            string contentType = GetContentType(fullPath);

            // Read the file content
            byte[] fileContent = System.IO.File.ReadAllBytes(fullPath);

            // Return the file content with the appropriate content type
            return File(fileContent, contentType);
        }

        private string GetContentType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();

            // Try to get the content type from the loaded dictionary
            if (_contentTypes.TryGetValue(extension, out string contentType))
            {
                return contentType;
            }

            // Default to "application/octet-stream" if the extension is not found
            return "application/octet-stream";
        }
    }

    public class ProxiesConfig
    {
        public List<NetProxy> Net { get; set; }
        public List<PipeProxy> Pipe { get; set; }
    }

    public class NetProxy
    {
        public string Source { get; set; }
        public string Destination { get; set; }
    }

    public class PipeProxy
    {
        public string Source { get; set; }
        public string Command { get; set; }
    }
}