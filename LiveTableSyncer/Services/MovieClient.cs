using LiveTableSyncer.Models;
using RestSharp;
using RestSharp.Authenticators;

namespace LiveTableSyncer.Services;

public class MovieClient : IMovieClient, IDisposable
{
    readonly RestClient _client;
    public MovieClient() { 
        var options = new RestClientOptions(Constants.SERVER_URL+"/api");
        _client = new RestClient(options);
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    public async Task<MoviesCollectionPageDto> GetMovies(int top, int skip, string orderBy="")
    {

        var response = await _client.GetAsync<MoviesCollectionPageDto>($"movies?top={top}&skip={skip}&orderBy={orderBy}");

        return response ?? throw new Exception("Failed to fetch movies.");
    }

    public async Task<MovieDto> UpdateMovie(MovieDto movie)
    {
        var request = new RestRequest()
        {
            Method = Method.Put,
            Resource = $"movies/{movie.Id}",
            RequestFormat = DataFormat.Json,
            
        };
        request.AddBody(movie);
        //Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(request));
        var response = await _client.PutAsync<MovieDto>(request) ?? throw new Exception("Response nahi aya from Update");
        return response;
    }

    public async Task<MovieDto?> GetMovie(int movieId)
    {
        var response = await _client.GetAsync<MovieDto>($"movies/{movieId}");
        return response;
    }


}

public interface IMovieClient
{
    Task<MoviesCollectionPageDto> GetMovies(int top, int skip, string orderBy = "");
    Task<MovieDto> UpdateMovie(MovieDto movie);
    Task<MovieDto?> GetMovie(int movieId);

}

public static class Constants
{
    public static string SERVER_URL = "http://localhost:5219";
}
