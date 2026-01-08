using ApiPetFoundation.Application;
using ApiPetFoundation.Infrastructure;
using ApiPetFoundation.Infrastructure.Identity;
using ApiPetFoundation.Soap.Api.Services;
using Microsoft.AspNetCore.Identity;
using SoapCore;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("https://localhost:5003");

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowGateway", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddSoapCore();
builder.Services.AddScoped<IPetSoapService, PetSoapService>();
builder.Services.AddHealthChecks();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await IdentitySeed.SeedRoles(roleManager);
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowGateway");

app.UseAuthentication();
app.UseAuthorization();

#pragma warning disable ASP0014
app.UseEndpoints(endpoints =>
{
    endpoints.UseSoapEndpoint<IPetSoapService>(
        "/soap/pets.svc",
        new[]
        {
            new SoapEncoderOptions
            {
                WriteEncoding = System.Text.Encoding.UTF8,
                MessageVersion = System.ServiceModel.Channels.MessageVersion.Soap11
            },
            new SoapEncoderOptions
            {
                WriteEncoding = System.Text.Encoding.UTF8,
                MessageVersion = System.ServiceModel.Channels.MessageVersion.Soap12WSAddressing10
            }
        },
        SoapSerializer.XmlSerializer);
});
#pragma warning restore ASP0014

app.MapHealthChecks("/health");

await app.RunAsync();
