using LiveTableSyncer.Models;
using RestSharp;
using RestSharp.Authenticators;

namespace LiveTableSyncer.Services;

public class MovieClient : IMovieClient, IDisposable
{
    readonly RestClient _client;
    public MovieClient() { 
        var options = new RestClientOptions("http://localhost:5219/api");
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


}

public interface IMovieClient
{
    Task<MoviesCollectionPageDto> GetMovies(int top, int skip, string orderBy = "");
    Task<MovieDto> UpdateMovie(MovieDto movie);

}
