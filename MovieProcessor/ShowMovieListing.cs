using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MovieProcessor.Entities;
using MovieProcessor.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MovieProcessor
{
    public class ShowMovieListing : IShowMovieListing
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger _logger;
        private MovieProcessorHttpClient _client;

        public ShowMovieListing(IMemoryCache memoryCache,
            ILogger<ShowMovieListing> logger,
            MovieProcessorHttpClient client)
        {
            _client = client;
            _logger = logger;
            _cache = memoryCache;
        }

        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);

        public async Task<List<MovieDetail>> GetShows()
        {
            Func<MovieDetail, decimal> orderClause = c => Decimal.Parse(c.Price);

            var cacheKey = "moviesList";
            List<MovieDetail> shows = null;

            if (_cache.TryGetValue(cacheKey, out shows))
            {
                return shows.OrderBy(orderClause).ToList();
            }

            await semaphoreSlim.WaitAsync();
            try
            {
                if (_cache.TryGetValue(cacheKey, out shows))
                {
                    return shows.OrderBy(orderClause).ToList();
                }

                var movieList = await _client.GetLatestMovieListing();
                var eachMovieDetail = movieList.Movies.Select(eachMovie => _client.GetLatestMovieDetailListing(eachMovie)).ToList();
                var completedMovieDetail = await Task.WhenAll(eachMovieDetail);


                var cacheExpirationOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.Now.AddHours(4),
                    Priority = CacheItemPriority.Normal
                };

                _cache.Set(cacheKey, completedMovieDetail.ToList(), cacheExpirationOptions);
                return completedMovieDetail.OrderBy(orderClause).ToList();
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error,e.Message);
                throw;
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
    }
}
