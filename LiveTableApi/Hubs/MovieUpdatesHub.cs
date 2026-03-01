using Microsoft.AspNetCore.SignalR;

namespace LiveTableApi.Hubs
{
    public class MovieUpdatesHub : Hub
    {
        private readonly IHubContext<MovieUpdatesHub> _hubContext;

        public MovieUpdatesHub(IHubContext<MovieUpdatesHub> hubContext) 
        {
            _hubContext = hubContext;
        }
        public async Task SendMovieUpdate(int movieId)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMovieUpdate", movieId);
        }
    }
}
