using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codenames.Websocket;

namespace Codenames.Extensions
{
  public static class CodenamesExtensions
  {

    public static IServiceCollection AddCodenamesServices(this IServiceCollection services,
    IConfiguration config)
    {
      services.AddScoped<ICodenamesRepo, CodenamesRepo>();
      return services;
    }
  }
}