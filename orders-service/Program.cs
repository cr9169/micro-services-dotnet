using DotNetEnv;

// The first step in creating an ASP.NET Core application. The builder provides the basic infrastructure for the application and contains all the services and configuration.
var builder = WebApplication.CreateBuilder(args);

/* 
1. Finds the .env file in the project folder
2. Reads the file
3. Breaks each line into a key and value
4. Adds each pair as an environment variable to the system (in the memory of the current process) 
*/
Env.Load();

// Adds all environment variables to the application configuration system. This allows you to access them via builder.Configuration.
builder.Configuration.AddEnvironmentVariables();

/* Doesn't have to create a constructor in this case in AppConfig. We are "Object Initializer Syntax",
   it's a shorthand and convenient way to create and initialize an object. */
var appConfig = new AppConfig {
    DbConnectionUrl = builder.Configuration["DB_CONNECTION_URL"] ?? "Server=localhost;Database=DefaultDb;",
    JwtSecret       = builder.Configuration["JWT_SECRET"]       ?? "DefaultSecretKey",
    IpAddress       = builder.Configuration["IP_ADDRESS"]       ?? "127.0.0.1"
};

// Registering an AppConfig instance as a Singleton instance, so it can be injected anywhere in the system.
/* That means: 
   1. Take the appConfig object.
   2. Register it in the app's DI container.
   3. Every time someone requests AppConfig, return the exact same instance. */
builder.Services.AddSingleton(appConfig);

var app = builder.Build();

app.Run();
