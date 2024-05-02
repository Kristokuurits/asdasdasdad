using ITB2203Application.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace ITB2203Application.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionsController : ControllerBase
    {
        private readonly DataContext _context;

        public SessionsController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Session>> GetSessions(string? auditoriumname = null, DateTime? periodstart = null, DateTime? periodend = null)
        {
            var query = _context.Sessions.AsQueryable();


            if (auditoriumname != null)
                query = query.Where(x => x.AuditoriumName != null && x.AuditoriumName.ToUpper().Contains(auditoriumname.ToUpper()));
            if (periodstart != null)
                query = query.Where(x => x.StartTime >= periodstart);
            if (periodend != null)
                query = query.Where(x => x.StartTime <= periodend);

            return query.ToList();
        }

        [HttpGet("{id}")]
        public ActionResult<Session> GetSession(int id)
        {
            var session = _context.Sessions.Find(id);

            if (session == null)
            {
                return NotFound();
            }

            return Ok(session);
        }

        [HttpPut("{id}")]
        public IActionResult PutSession(int id, Session session)
        {
            if (id != session.Id)
            {
                return BadRequest();
            }

            _context.Entry(session).State = EntityState.Modified;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Sessions.Any(e => e.Id == id))
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

        [HttpPost]
        public ActionResult<Session> PostSession(Session session)
        {

            var existingMovie = _context.Movies.Any(m => m.Id == session.MovieId);
            if (!existingMovie)
            {
                return NotFound("Movie not found.");
            }


            if (string.IsNullOrEmpty(session.AuditoriumName))
            {
                return BadRequest("Auditorium name is required.");
            }


            if (session.StartTime > DateTime.Now)
            {
                return BadRequest("Start time cannot be in the past.");
            }


            var conflictingSession = _context.Sessions.FirstOrDefault(s => s.AuditoriumName == session.AuditoriumName && s.StartTime == session.StartTime);
            if (conflictingSession != null)
            {
                return Conflict("Another session is already scheduled for the same auditorium at the same time.");
            }

            _context.Sessions.Add(session);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetSession), new { Id = session.Id }, session);
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteSession(int id)
        {
            var session = _context.Sessions.Find(id);
            if (session == null)
            {
                return NotFound();
            }

            _context.Sessions.Remove(session);
            _context.SaveChanges();

            return Ok();
        }
    }
}