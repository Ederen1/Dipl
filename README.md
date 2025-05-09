# Web application

A web application for secure file sharing and requesting. Allows users to send files to others, request files, and protect shared content with passwords.

## Prerequisites

- .NET 9.0 SDK

## Building the Project

To build the project:

```bash
cd Dipl
dotnet build
```

## Running the Application

To run the application:

```bash
cd Dipl
dotnet run --project Dipl.Web
```

The application can be accessed at `https://localhost:7228` by default.

## Configuration

There are defaults present in `appsettings.Development.json` for most options.

In `appsettings.json` (or `appsettings.Development.json`) allows for configuration of:

- Database connection strings
- Email settings for notifications
- File storage settings
- Visibility and possibility to edit when files were uploaded
- Allowed domains and emails for access control

There are two configuration parameters that need to be specified:
- `Authentication.Microsoft.ClientId`
- `Authentication.Microsoft.ClientSecret`

They can be obtained from Microsoft Azure app registration.

It is not recommended to use `appsettings.json` to set these parameters in production. Instead, use environment variables or a secrets manager.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
