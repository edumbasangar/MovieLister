using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MovieProcessor.Interfaces;

namespace MovieProcessor.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection ConfigureMovieProcessor(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.Configure<MovieProcessorSettings>(configuration.GetSection("MovieProcessor:MovieLister"));
            services.AddMemoryCache();
            services.AddOptions();
            services.AddSingleton<IShowMovieListing, ShowMovieListing>();
            services.AddHttpClient<MovieProcessorHttpClient>();

            return services;
        }
    }
}
