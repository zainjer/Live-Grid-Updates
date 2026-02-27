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

    public async Task<List<MovieDto>> GetMovies(int pageSize, int pageCount)
    {
        var response = await _client.GetAsync<List<MovieDto>>($"movies?pageSize={pageSize}&pageCount={pageCount}");

        return response ?? new List<MovieDto>();
    }


}

public interface IMovieClient
{
    Task<List<MovieDto>> GetMovies(int pageSize, int pageCount);

}
