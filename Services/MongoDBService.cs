using SeeSharpMovies.Models;
using MongoDB.Driver;
using MongoDB.Driver.Search;

namespace SeeSharpMovies.Services;

public class MongoDBService : IMongoDBService
{
    private readonly MongoClient _client;
    private readonly IMongoDatabase _mongoDatabase;
    private readonly IMongoCollection<Movie> _movies;

    public MongoDBService(MongoDBSettings settings)
    {
        _client = new MongoClient(settings.AtlasURI);
        _mongoDatabase = _client.GetDatabase(settings.DatabaseName);
        _movies = _mongoDatabase.GetCollection<Movie>("movies");
    }

    public MongoDBService()
    {
           
    }
    public IEnumerable<Movie> GetAllMovies()
    {
        var movies = _movies.Find(movie => true).ToList();

        return movies;
    }

    public IEnumerable<Movie> GetMoviesPerPage(int pageNumber, int pageSize)
    {
        var movies = _movies.Find(movie => true)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize).ToList();

        return movies;
    }

    public Movie? GetMovieById(string id)
    {
        var movie = _movies.Find(movie => movie.Id == id).FirstOrDefault();

        return movie;
    }

    public IEnumerable<Movie> SearchMovieByTitle(string title)
    {
        SearchFuzzyOptions fuzzyOptions = new SearchFuzzyOptions
        {
            MaxEdits = 1,
            PrefixLength = 1,
            MaxExpansions = 256
        };

        var movies = 
            _movies
                .Aggregate()
                .Search(
                    Builders<Movie>.Search.Autocomplete(movie => movie.Title,title, fuzzy: fuzzyOptions), 
                    indexName: "mflix_movies")
                .Project<Movie>(
                    Builders<Movie>.Projection.Exclude(movie => movie.Id))
                .ToList();

        return movies;
    }
}
