var builder = WebApplication.CreateBuilder(args);

// YARP: carga routes + clusters desde la sección "ReverseProxy" de la configuración.
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// El Gateway solo enruta: no valida identidad ni compone respuestas (lección 4.9).
app.MapReverseProxy();

app.Run();
