namespace PandaBack.config;

public static class CacheConfig
{

    public static IServiceCollection AddCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            var redisConnection = configuration["REDIS_CONNECTION"] 
                                  ?? configuration.GetConnectionString("Redis") 
                                  ?? "localhost:6379,password=cachepassword";

            options.Configuration = redisConnection;
            options.InstanceName = "PandaCache:";
        });
        
        return services;
    }
}