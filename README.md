# An ASP.NET backend implementation of Codenames

This is a .NET rebuild of a [previous NodeJS project](https://github.com/rrramsurrun/portfolio/tree/main/server/src/codenames) that served as the backend of a websocket-based implementation of the word game 'Codenames'.

The code in this repo is called in Program.cs using the following:

    builder.Services.AddCodenamesServices(builder.Configuration);

    var app = builder.Build();

    var webSocketOptions = new WebSocketOptions()
    {
      KeepAliveInterval = TimeSpan.FromMinutes(2),
    };

    app.UseWebSockets(webSocketOptions);

    app.UseStaticFiles(new StaticFileOptions{
    FileProvider = new PhysicalFileProvider(
    Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "dist")),
    RequestPath = "/codenames",
    });

    app.MapControllers();
