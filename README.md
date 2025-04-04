# softsprocket_webapp

A cross-platform C# http server that serves static files over HTTP and HTTPS. The application dynamically resolves content types, supports configurable web root directories, allows HTTPS configuration with optional certificates, and supports proxying requests to external services or commands.

## Features
- Serves static files from a configurable web root directory.
- Dynamically resolves MIME types using a `contenttypes.json` file.
- Supports HTTPS with an optional `.pfx` certificate.
- Configurable HTTP-to-HTTPS redirection (default is disabled).
- Proxies requests to external URLs (`net` proxies) or commands (`pipe` proxies).
- Cross-platform compatibility (Linux, macOS, Windows).

## Prerequisites
- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or later installed.
- OpenSSL (for generating self-signed certificates, if needed).

## Setup Instructions

### 1. Clone the Repository
```bash
git clone <repository-url>
cd softsprocket_webapp
```

### 2. Configure the Application
- **Web Root**: Update the `WebRoot` value in `appsettings.json` to set the directory for serving static files (default is `wwwroot`).
- **HTTPS Certificate**: 
  - If HTTPS is enabled (`"Enabled": true` in `appsettings.json`), provide a `.pfx` certificate file and its password under `Kestrel:Endpoints:Https:Certificate`.
  - If no certificate is provided, the application will still run without HTTPS.
  - Create a `certs` directory to store your certificate:
    ```bash
    mkdir certs
    ```
- **HTTP-to-HTTPS Redirection**: 
  - Set `"RedirectHttpToHttps": true` in `appsettings.json` to enable automatic redirection from HTTP to HTTPS.
  - Defaults to `false`.

### 3. Generate a Self-Signed Certificate (Optional)
If you don't have a certificate, you can generate one using OpenSSL:
```bash
openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365 -nodes
openssl pkcs12 -export -out certs/localhost.pfx -inkey key.pem -in cert.pem -password pass:yourpassword
```

### 4. Define Proxy Configurations
Create a `proxies.json` file in the project directory to define proxy rules. Example:
```json
{
  "net": [
    {
      "source": "/api/external",
      "destination": "https://api.example.com"
    }
  ],
  "pipe": [
    {
      "source": "/api/command",
      "command": "echo"
    }
  ]
}
```

- **`net` Proxies**: Forward requests to an external URL, appending the remaining path and query string.
- **`pipe` Proxies**: Execute a command, passing query arguments via `stdin` and returning the output via `stdout`.

### 5. Run the Application
```bash
dotnet run
```

### 6. Access the Application
- HTTP: [http://localhost:5000](http://localhost:5000)
- HTTPS: [https://localhost:5001](https://localhost:5001) (if enabled)

## File Structure
```
softsprocket_webapp/
├── appsettings.json          # Application configuration (web root, HTTPS settings, redirection)
├── certs/                    # Directory for HTTPS certificates (optional)
│   └── localhost.pfx         # HTTPS certificate file (optional)
├── Controllers/
│   └── HomeController.cs     # Handles file serving and proxy logic
├── wwwroot/                  # Default web root directory
│   └── index.html            # Example static file
├── contenttypes.json         # MIME type mappings for file extensions
├── proxies.json              # Proxy configuration file
├── Program.cs                # Application entry point
└── README.md                 # Documentation
```

## Customization
- **Content Types**: Add or modify MIME type mappings in `contenttypes.json`.
- **Web Root**: Change the `WebRoot` value in `appsettings.json` to serve files from a different directory.
- **HTTPS**: Replace the certificate in the `certs` directory with your own `.pfx` file, or disable HTTPS by setting `"Enabled": false`.
- **HTTP-to-HTTPS Redirection**: Enable or disable redirection by modifying the `"RedirectHttpToHttps"` value in `appsettings.json`.
- **Proxies**: Add or modify proxy rules in `proxies.json`.

## License
This project is licensed under the MIT License. See the LICENSE file for details.