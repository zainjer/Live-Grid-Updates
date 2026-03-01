using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LiveTableApi.Data;
using LiveTableApi.Data.Entities;
using Bogus;
using LiveTableSyncer.Models;

namespace LiveTableApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly Faker<Movie> _movieFaker;

        public MoviesController(AppDbContext context)
        {
            _context = context;
            var genres = _context.Genres;
            _movieFaker = new Faker<Movie>()
                .RuleFor(m => m.Title, f => $"{f.Name.FirstName()}'s {f.Commerce.ProductName()} from {f.Company.CompanyName()} ")
                .RuleFor(m => m.Description, f => f.Lorem.Paragraph())
                .RuleFor(m => m.Author, f => f.Person.FullName)
                .RuleFor(m => m.Created, f => f.Date.Past())
                .RuleFor(m => m.Updated, f => f.Date.Recent())
                .RuleFor(m => m.Genres, f => f.PickRandom(genres, f.Random.Int(1, 3)).ToList());
        }

        // GET: api/Movies
        [HttpGet]
        public async Task<ActionResult<MoviesCollectionPageDto>> GetMovies(int top = 500, int skip = 0, string orderBy = "")
        {
            if (top < 1) throw new ArgumentOutOfRangeException(nameof(top));


            var queryable = _context.Movies.Include(x => x.Genres).AsQueryable();


            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                orderBy = orderBy.Trim().ToLower();
                if (orderBy.Contains("created", StringComparison.OrdinalIgnoreCase))
                {
                    queryable = orderBy.Contains("asc") ? queryable.OrderBy(m => m.Created) : queryable.OrderByDescending(m => m.Created);
                }
                else if (orderBy.Contains("updated", StringComparison.OrdinalIgnoreCase))
                {
                    queryable = orderBy.Contains("asc") ? queryable.OrderBy(m => m.Updated) : queryable.OrderByDescending(m => m.Updated);
                }
                else if (orderBy.Contains("title", StringComparison.OrdinalIgnoreCase))
                {
                    queryable = orderBy.Contains("asc") ? queryable.OrderBy(m => m.Title) : queryable.OrderByDescending(m => m.Title);
                }
                else if (orderBy.Contains("author", StringComparison.OrdinalIgnoreCase))
                {
                    queryable = orderBy.Contains("asc") ? queryable.OrderBy(m => m.Author) : queryable.OrderByDescending(m => m.Author);
                }
                else if (orderBy.Contains("id", StringComparison.OrdinalIgnoreCase))
                {
                    queryable = orderBy.Contains("asc") ? queryable.OrderBy(m => m.Id) : queryable.OrderByDescending(m => m.Id);
                }
                else
                {
                    queryable = queryable.OrderBy(m => m.Id);
                }
            }
            else { queryable = queryable.OrderBy(m => m.Id); }

            var result = await queryable.Skip(skip).Take(top).ToListAsync();

            var moviesDto = new List<MovieDto>() { };
            foreach (var movie in result)
            {
                moviesDto.Add(new MovieDto()
                {
                    Id = movie.Id,
                    Title = movie.Title,
                    Author = movie.Author,
                    Description = movie.Description,
                    Created = movie.Created,
                    Updated = movie.Updated,
                    Genres = movie.Genres.Select(x => x.Name).ToList()
                });
            }

            var count = await _context.Movies.CountAsync();
            var collectionPageDto = new MoviesCollectionPageDto(moviesDto, count);
            return collectionPageDto;

        }

        // GET: api/Movies/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MovieDto>> GetMovie(int id)
        {
            var movie = await _context.Movies
                .AsNoTracking()
                .Include(x => x.Genres)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            return new MovieDto
            {
                Id = movie.Id,
                Title = movie.Title,
                Description = movie.Description,
                Author = movie.Author,
                Created = movie.Created,
                Updated = movie.Updated,
                Genres = movie.Genres.Select(g => g.Name).ToList()
            };
        }

        // PUT: api/Movies/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<ActionResult<MovieDto>> PutMovie(int id, MovieDto movieDto)
        {
            if (id != movieDto.Id)
            {
                return BadRequest();
            }
            var movieEntity = await _context.Movies.Include(x => x.Genres).FirstOrDefaultAsync(x => x.Id == id);

            if (movieEntity == null)
            {
                return NotFound();
            }
            movieEntity.Title = movieDto.Title;
            movieEntity.Description = movieDto.Description;
            movieEntity.Author = movieDto.Author;
            movieEntity.Updated = movieDto.Updated = DateTime.UtcNow;


            _context.Entry(movieEntity).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MovieExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return movieDto;
        }

        // POST: api/Movies
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Movie>> PostMovie(Movie movie)
        {
            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMovie", new { id = movie.Id });
        }

        [HttpGet("bogus")]
        public async Task<ActionResult> PostBogusMovie()
        {
            var movie = _movieFaker.Generate();
            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetMovie", new { id = movie.Id }, movie);
        }


        // DELETE: api/Movies/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }
    }
}
