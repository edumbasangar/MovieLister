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
            //Func<MovieDetail, bool> whereClause = c => c. is false;

            var cacheKey = "moviesList";
            List<MovieDetail> shows = null;

            if (_cache.TryGetValue(cacheKey, out shows))
            {
                //return shows.Where(whereClause).ToList();
                return shows.ToList();
            }

            await semaphoreSlim.WaitAsync();
            try
            {
                if (_cache.TryGetValue(cacheKey, out shows))
                {
                    //return shows.Where(whereClause).ToList();
                    return shows.ToList();
                }

                var movieList = await _client.GetLatestMovieListing();
                var eachMovieDetail = movieList.Movies.Select(eachMovie => _client.GetMovieDetail(eachMovie)).ToList();
                var completedMovieDetail = await Task.WhenAll(eachMovieDetail);


                var cacheExpirationOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.Now.AddHours(4),
                    Priority = CacheItemPriority.Normal
                };

                _cache.Set(cacheKey, completedMovieDetail.ToList(), cacheExpirationOptions);
                return completedMovieDetail.ToList();
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
