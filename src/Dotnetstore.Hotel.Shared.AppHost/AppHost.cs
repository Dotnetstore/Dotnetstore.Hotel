using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

// Integration tests (Aspire.Hosting.Testing) boot this same AppHost in-process. They must not share the
// persistent dev data volume below, or every test run leaves throwaway accounts/hotels in the real dev database.
var isIntegrationTest = builder.Configuration.GetValue<bool>("IsIntegrationTest");

var postgresBuilder = builder.AddPostgres("postgres");
if (!isIntegrationTest)
{
    postgresBuilder = postgresBuilder.WithDataVolume();
}

var postgres = postgresBuilder;
var hotelDb = postgres.AddDatabase("hoteldb");
var identityDb = postgres.AddDatabase("identitydb");

var jwtSigningKey = builder.AddParameter("jwt-signing-key", secret: true);

var apiHotels = builder.AddProject<Projects.Dotnetstore_Hotel_Api_Hotels>("apihotels")
    .WithReference(hotelDb)
    .WaitFor(hotelDb)
    .WithEnvironment("Jwt__SigningKey", jwtSigningKey);

var apiUsers = builder.AddProject<Projects.Dotnetstore_Hotel_Api_Users>("apiusers")
    .WithReference(identityDb)
    .WaitFor(identityDb)
    .WithEnvironment("Jwt__SigningKey", jwtSigningKey);

builder.AddProject<Projects.Dotnetstore_Hotel_Ui_Web>("ui")
    .WithReference(apiHotels)
    .WithReference(apiUsers)
    .WaitFor(apiHotels)
    .WaitFor(apiUsers)
    .WithExternalHttpEndpoints();

builder.Build().Run();
