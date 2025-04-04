
# softsprocket_webapp

A cross-platform C# web application that serves static files over HTTP and HTTPS. The application dynamically resolves content types, supports configurable web root directories, and allows HTTPS configuration with certificates.

## Features
- Serves static files from a configurable web root directory.
- Dynamically resolves MIME types using a `contenttypes.json` file.
- Supports HTTPS with a configurable `.pfx` certificate.
- Cross-platform compatibility (Linux, macOS, Windows).

## Prerequisites
- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or later installed.
- OpenSSL (for generating self-signed certificates, if needed).

## Setup Instructions

### 1. Clone the Repository
```bash
git clone <repository-url>
cd softsprocket_webapp
mkdir certs
```

### 2. Configure the Application
- **Web Root**: Update the `WebRoot` value in `appsettings.json` to set the directory for serving static files (default is `wwwroot`).
- **HTTPS Certificate**: Place your `.pfx` certificate in the `certs` directory and update the `Path` and `Password` fields in `appsettings.json` under `Kestrel:Endpoints:Https:Certificate`.

### 3. Generate a Self-Signed Certificate (Optional)
If you don't have a certificate, you can generate one using OpenSSL:
```bash
openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365 -nodes
openssl pkcs12 -export -out certs/localhost.pfx -inkey key.pem -in cert.pem -password pass:yourpassword
```

### 4. Run the Application
```bash
dotnet run
```

### 5. Access the Application
- HTTP: [http://localhost:5000](http://localhost:5000)
- HTTPS: [https://localhost:5001](https://localhost:5001)

## File Structure
```
softsprocket_webapp/
├── appsettings.json          # Application configuration (web root, HTTPS settings)
├── certs/                    # Directory for HTTPS certificates
│   └── localhost.pfx         # HTTPS certificate file
├── Controllers/
│   └── HomeController.cs     # Handles file serving logic
├── wwwroot/                  # Default web root directory
│   └── index.html            # Example static file
├── contenttypes.json         # MIME type mappings for file extensions
├── Program.cs                # Application entry point
└── README.md                 # Documentation
```

## Customization
- **Content Types**: Add or modify MIME type mappings in `contenttypes.json`.
- **Web Root**: Change the `WebRoot` value in `appsettings.json` to serve files from a different directory.
- **HTTPS**: Replace the certificate in the `certs` directory with your own `.pfx` file.

## License
This project is licensed under the MIT License. See the LICENSE file for details.
generate cert:

