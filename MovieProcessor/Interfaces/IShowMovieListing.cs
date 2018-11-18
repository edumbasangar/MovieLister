using MovieProcessor.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MovieProcessor.Interfaces
{
    public interface IShowMovieListing
    {
        Task<List<MovieDetail>> GetShows();
    }
}
