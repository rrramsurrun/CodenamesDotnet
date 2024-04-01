using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codenames.Models;
using Codenames.Services;
using Codenames.Websocket;

namespace Codenames.Extensions
{
  public static class CodenamesExtensions
  {

    public static IServiceCollection AddCodenamesServices(this IServiceCollection services,
    IConfiguration config)
    {
      services.AddScoped<ISocketHandler, SocketHandler>();
      services.Configure<GameStoreDatabaseSettings>(config.GetSection("GameStoreDatabase"));
      services.AddSingleton<GameService>();
      return services;
    }
  }
}