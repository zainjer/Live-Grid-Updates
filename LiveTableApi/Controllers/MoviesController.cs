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
        public async Task<ActionResult<IEnumerable<MovieDto>>> GetMovies(int pageSize = 500, int pageCount = 1)
        {
            if (pageCount < 1) throw new ArgumentOutOfRangeException(nameof(pageCount));
            if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize));

            var result = await _context.Movies.Include(x=> x.Genres).Skip(((pageCount-1) * pageSize)).Take(pageSize).ToListAsync();

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
            return moviesDto;

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
        public async Task<IActionResult> PutMovie(int id, Movie movie)
        {
            if (id != movie.Id)
            {
                return BadRequest();
            }

            _context.Entry(movie).State = EntityState.Modified;

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

            return NoContent();
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
