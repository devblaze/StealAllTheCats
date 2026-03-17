using Microsoft.Extensions.Configuration;
using StealAllTheCats.Common.Dtos;

namespace StealAllTheCats.Common;

public static class Configuration
{
    public static CatApiConfig CatApiConfig(this IConfiguration configuration) =>
        configuration.GetSection("CatApi").Get<CatApiConfig>() ?? new CatApiConfig();
}