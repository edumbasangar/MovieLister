using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MovieProcessor.Extensions;
using MovieProcessor.Interfaces;

namespace MovieListing.Controllers
{
    [Route("api/[controller]")]
    public class MovieDataController : Controller
    {
        private readonly ServiceCollection _serviceCollection = new ServiceCollection();
        private readonly IConfigurationRoot _configuration;
        private readonly IServiceProvider _serviceProvider;

        public MovieDataController()
        {
            _configuration = GetConfiguration();
            _serviceCollection.ConfigureMovieProcessor(_configuration);
            _serviceProvider = _serviceCollection.BuildServiceProvider();
        }
        
        [HttpGet("[action]")]
        public IEnumerable<MovieListing> MovieListingDetails()
        {
            var appService = _serviceProvider.GetService<IShowMovieListing>();

            return appService.GetShows().Result.Select(index => new MovieListing
            {
                Id = index.ID.ToString(),
                Description = index.Metascore,
                Name = index.Title,
                Price = index.Price
            });
        }

        public class MovieListing
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Price { get; set; }
            public string Description { get; set; }
        }

        private IConfigurationRoot GetConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
        }

    }
}
