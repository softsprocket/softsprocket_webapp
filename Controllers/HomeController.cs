using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;

namespace softsprocket_webapp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly Dictionary<string, string> _contentTypes;

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
        }

        [HttpGet("{*filePath}")]
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
}
