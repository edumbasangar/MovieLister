using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using MovieProcessor;
using MovieProcessor.Entities;
using Polly;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MovieProcessorTests
{
    public class MovieListingTests
    {
        private readonly Mock<ILogger<MovieProcessorHttpClient>> _mockLogger;

        public MovieListingTests()
        {
            _mockLogger = new Mock<ILogger<MovieProcessorHttpClient>>();

        }


        [Fact]
        public async Task HttpClientTest_HappyPath()
        {
            //Arrange
            var fakeResponse = new Mock<MovieList>();

            var options = Options.Create(new MovieProcessorSettings
            {
                MovieDetailFallbackRelativeURL = "api/cinemaworld/movie/",
                MovieDetailRelativeURL = "api/cinemaworld/movie/",
                MovieListFallbackRelativeURL = "api/cinemaworld/movies",
                MovieListRelativeURL = "api/cinemaworld/movies",
                BaseURL = "http://webjetapitest.azurewebsites.net/",
                AccessToken = "sjd1HfkjU83ksdsm3802k"
            });

            var httpMessageHandler = new Mock<HttpMessageHandler>();
            httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,//This is where it should fail each time called
                    Content = new ObjectContent<MovieList>(new MovieList(), new JsonMediaTypeFormatter())
                }));

            var httpClient = new HttpClient(httpMessageHandler.Object);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));

            IAsyncPolicy<HttpResponseMessage> mockPolicy = Policy.NoOpAsync<HttpResponseMessage>();

            var client = new MovieProcessorHttpClient(httpClient, _mockLogger.Object, options);

            //Act
            var result = await client.GetLatestMovieListing();

            //Assert
            Assert.IsType<MovieList>(result);
        }

        [Fact]
        public async Task HttpClientTest_InternalServerError()
        {
            //Arrange
            var fakeResponse = new Mock<MovieList>();

            var options = Options.Create(new MovieProcessorSettings
            {
                MovieDetailFallbackRelativeURL = "api/cinemaworld/movie/",
                MovieDetailRelativeURL = "api/cinemaworld/movie/",
                MovieListFallbackRelativeURL = "api/cinemaworld/movies",
                MovieListRelativeURL = "api/cinemaworld/movies",
                BaseURL = "http://webjetapitest.azurewebsites.net/",
                AccessToken = "sjd1HfkjU83ksdsm3802k"
            });

            var httpMessageHandler = new Mock<HttpMessageHandler>();
            httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,//This is where it should fail each time called
                    Content = new ObjectContent<MovieList>(new MovieList(), new JsonMediaTypeFormatter())
                }));

            var httpClient = new HttpClient(httpMessageHandler.Object);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));

            IAsyncPolicy<HttpResponseMessage> mockPolicy = Policy.NoOpAsync<HttpResponseMessage>();

            var client = new MovieProcessorHttpClient(httpClient, _mockLogger.Object, options);

            Assert.True(client.GetLatestMovieListing().IsFaulted);
        }

        [Fact]
        public async Task HttpClientTest_GetMovieDetail()
        {
            //Arrange
            var fakeResponse = new Mock<MovieDetail>();
            var fakeMovie = new Mock<Movie>();

            var options = Options.Create(new MovieProcessorSettings
            {
                MovieDetailFallbackRelativeURL = "api/cinemaworld/movie/",
                MovieDetailRelativeURL = "api/cinemaworld/movie/",
                MovieListFallbackRelativeURL = "api/cinemaworld/movies",
                MovieListRelativeURL = "api/cinemaworld/movies",
                BaseURL = "http://webjetapitest.azurewebsites.net/",
                AccessToken = "sjd1HfkjU83ksdsm3802k"
            });

            var httpMessageHandler = new Mock<HttpMessageHandler>();
            httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new ObjectContent<MovieDetail>(new MovieDetail(), new JsonMediaTypeFormatter())
                }));

            var httpClient = new HttpClient(httpMessageHandler.Object);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));

            IAsyncPolicy<HttpResponseMessage> mockPolicy = Policy.NoOpAsync<HttpResponseMessage>();

            var client = new MovieProcessorHttpClient(httpClient, _mockLogger.Object, options);

            //Act
            var result = await client.GetMovieDetail(fakeMovie.Object);

            //Assert
            Assert.IsType<MovieDetail>(result);
        }

        [Fact]
        public async Task HttpClientTest_GetMovieDetail_ReturnsException()
        {
            //Arrange
            var fakeResponse = new Mock<MovieDetail>();
            var fakeMovie = new Mock<Movie>();

            var options = Options.Create(new MovieProcessorSettings
            {
                MovieDetailFallbackRelativeURL = "api/cinemaworld/movie/",
                MovieDetailRelativeURL = "api/cinemaworld/movie/",
                MovieListFallbackRelativeURL = "api/cinemaworld/movies",
                MovieListRelativeURL = "api/cinemaworld/movies",
                BaseURL = "http://webjetapitest.azurewebsites.net/",
                AccessToken = "sjd1HfkjU83ksdsm3802k"
            });

            var httpMessageHandler = new Mock<HttpMessageHandler>();
            httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new ObjectContent<MovieDetail>(new MovieDetail(), new JsonMediaTypeFormatter())
                }));

            var httpClient = new HttpClient(httpMessageHandler.Object);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));

            IAsyncPolicy<HttpResponseMessage> mockPolicy = Policy.NoOpAsync<HttpResponseMessage>();

            var client = new MovieProcessorHttpClient(httpClient, _mockLogger.Object, options);

            Assert.True(client.GetMovieDetail(fakeMovie.Object).IsFaulted);
        }
    }
}
