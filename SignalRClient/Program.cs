using Microsoft.AspNetCore.SignalR.Client;

Console.Write("JWT: ");
var token = Console.ReadLine();

var connection = new HubConnectionBuilder()
    .WithUrl("https://localhost:5000/hubs/notifications", options =>
    {
        options.AccessTokenProvider = () => Task.FromResult<string?>(token);
        options.HttpMessageHandlerFactory = handler =>
        {
            if (handler is HttpClientHandler clientHandler)
            {
                clientHandler.ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }

            return handler;
        };
    })
    .WithAutomaticReconnect()
    .Build();

connection.On<object>("AdoptionStatusChanged", data =>
{
    Console.WriteLine("AdoptionStatusChanged: " + data);
});

connection.On<object>("AdoptionRequestCreated", data =>
{
    Console.WriteLine("AdoptionRequestCreated: " + data);
});

connection.On<object>("PetCreated", data =>
{
    Console.WriteLine("PetCreated: " + data);
});

await connection.StartAsync();
Console.WriteLine("Conectado. Presiona Enter para salir.");
Console.ReadLine();
