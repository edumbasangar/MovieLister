using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MovieProcessor.Entities;
using Polly;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Wrap;

namespace MovieProcessor
{
    public class MovieProcessorHttpClient
    {
        private HttpClient _client;
        private ILogger<MovieProcessorHttpClient> _logger;
        private readonly string _apiKey;
        private readonly FallbackPolicy<Task<MovieList>> _policy;
        private readonly Policy policy;
        private readonly PolicyWrap policyStrategy;
        private readonly PolicyWrap<MovieList> _policyWrap;
        private readonly MovieProcessorSettings _movieProcessorSettings;

        public MovieProcessorHttpClient(HttpClient client, ILogger<MovieProcessorHttpClient> logger, IOptions<MovieProcessorSettings> movieSettings)
        {
            _movieProcessorSettings = movieSettings.Value;
            _client = client;
            _client.BaseAddress = new Uri($"{_movieProcessorSettings.BaseURL}");
            _logger = logger;


            var timeoutPolicy = Policy
                .TimeoutAsync(
                    TimeSpan.FromMilliseconds(40), // _settings.TimeoutWhenCallingApi,
                    Polly.Timeout.TimeoutStrategy.Pessimistic
                );

            var circuitBreaker = Policy
                .Handle<Polly.Timeout.TimeoutRejectedException>()
                .CircuitBreakerAsync(
                    1, // _settings.ConsecutiveExceptionsAllowedBeforeBreaking,
                    TimeSpan.FromSeconds(3) // _settings.DurationOfBreak
                );
            var circuitBreakerWrappingTimeout = circuitBreaker
                .WrapAsync(timeoutPolicy);

            var fallbackCircuitBreaker = Policy<MovieList>
                .Handle<BrokenCircuitException>()
                .FallbackAsync(
                    cancellationToken =>
                    {
                        _logger.Log(LogLevel.Information, "Fallback triggered");
                        return GetFallbackMovies();
                    });

            var _httpRequestPolicy = Policy
                .HandleResult<HttpResponseMessage>(
                    r => r.StatusCode == HttpStatusCode.InternalServerError)
                .WaitAndRetryAsync(3,
                    retryAttempt => TimeSpan.FromSeconds(retryAttempt));

            var policy = Policy<MovieList>
                .Handle<Exception>()
                .Or<HttpRequestException>()
                .FallbackAsync(
                    cancellationToken =>
                    {
                        _logger.Log(LogLevel.Information, "Fallback triggered");
                        return GetFallbackMovies();
                    });

            _policyWrap = policy.WrapAsync(fallbackCircuitBreaker.WrapAsync(circuitBreaker));
        }

        public Task<MovieList> GetLatestMovieListing()
        {
            return _policyWrap.ExecuteAsync(() => GetMovies());
        }

        public async Task<MovieList> GetMovies()
        {
            var episodesUrl = new Uri($"3/list/1200?language=en-US&api_key=d0d5155966b45f3ffe1a79c2b25b7ae9",
                UriKind.Relative);
            var res = await _client.GetAsync(episodesUrl);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadAsAsync<MovieList>();
        }

        public async Task<MovieList> GetFallbackMovies()
        {
            var episodesUrl = new Uri($"3/list/1200?language=en-US&api_key=d0d5155966b45f3ffe1a79c2b25b7ae9",
                UriKind.Relative);
            var res = await _client.GetAsync(episodesUrl);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadAsAsync<MovieList>();
        }

        public async Task<MovieDetail> GetMovieDetail(Movie eachMovie)
        {
            var episodesUrl = new Uri($"3/movie/{eachMovie.ID}?language=en-US&api_key=d0d5155966b45f3ffe1a79c2b25b7ae9",
                UriKind.Relative);
            var res = await _client.GetAsync(episodesUrl);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadAsAsync<MovieDetail>();
        }

        public async Task<MovieDetail> GetFallbackMovieDetail(Movie eachMovie)
        {
            var episodesUrl = new Uri($"3/movie/{eachMovie.ID}?language=en-US&api_key=d0d5155966b45f3ffe1a79c2b25b7ae9",
                UriKind.Relative);
            var res = await _client.GetAsync(episodesUrl);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadAsAsync<MovieDetail>();
        }
    }
}
