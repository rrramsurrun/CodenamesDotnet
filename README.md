This is a .NET rebuild of an previous NodeJS project that served as the backend of a websocket-based implementation of the word game 'Codenames'.

The code in this repo is called in Program.cs using the following:

    builder.Services.AddCodenamesServices(builder.Configuration);

    var app = builder.Build();

    var webSocketOptions = new WebSocketOptions()
    {
      KeepAliveInterval = TimeSpan.FromMinutes(2),
    };

    app.UseWebSockets(webSocketOptions);

    app.MapControllers();
