var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").WithDataVolume();
var hotelDb = postgres.AddDatabase("hoteldb");
var identityDb = postgres.AddDatabase("identitydb");

var jwtSigningKey = builder.AddParameter("jwt-signing-key", secret: true);

builder.AddProject<Projects.Dotnetstore_Hotel_Api_Hotels>("apihotels")
    .WithReference(hotelDb)
    .WaitFor(hotelDb);

builder.AddProject<Projects.Dotnetstore_Hotel_Api_Users>("apiusers")
    .WithReference(identityDb)
    .WaitFor(identityDb)
    .WithEnvironment("Jwt__SigningKey", jwtSigningKey);

builder.Build().Run();
