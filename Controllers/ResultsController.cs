using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduSync.Data;
using EduSync.Models;
using EduSync.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduSync.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly EduSyncDbContext _context;

        public ResultsController(EduSyncDbContext context)
        {
            _context = context;
        }

        // POST: api/results
        [HttpPost]
        public async Task<IActionResult> CreateResult([FromBody] ResultForCreationDto resultForCreationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var assessment = await _context.Assessments.FindAsync(resultForCreationDto.AssessmentId);
            if (assessment == null)
            {
                return BadRequest($"Assessment with ID {resultForCreationDto.AssessmentId} not found.");
            }

            var user = await _context.Users.FindAsync(resultForCreationDto.UserId);
            if (user == null)
            {
                return BadRequest($"User with ID {resultForCreationDto.UserId} not found.");
            }

            if (!string.Equals(user.Role, "Student", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest($"User with ID {resultForCreationDto.UserId} is not a student and cannot submit results.");
            }

            if (resultForCreationDto.Score > assessment.MaxScore)
            {
                return BadRequest($"Score ({resultForCreationDto.Score}) cannot exceed the maximum score ({assessment.MaxScore}) for this assessment.");
            }

            var result = new Result
            {
                ResultId = Guid.NewGuid(),
                AssessmentId = resultForCreationDto.AssessmentId,
                UserId = resultForCreationDto.UserId,
                Score = resultForCreationDto.Score,
                AttemptDate = DateTime.UtcNow,
                Published = false // Default to not published
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            var resultToReturn = new ResultDto
            {
                ResultId = result.ResultId,
                AssessmentId = result.AssessmentId,
                AssessmentTitle = assessment.Title,
                UserId = result.UserId,
                UserName = user.Name,
                Score = result.Score,
                AttemptDate = result.AttemptDate,
                Published = result.Published
            };

            return CreatedAtAction(nameof(GetResultById), new { id = result.ResultId }, resultToReturn);
        }

        // GET: api/results/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ResultDto>> GetResultById(Guid id)
        {
            var result = await _context.Results
                .Include(r => r.Assessment)
                .Include(r => r.User)
                .Where(r => r.ResultId == id)
                .Select(r => new ResultDto
                {
                    ResultId = r.ResultId,
                    AssessmentId = r.AssessmentId,
                    AssessmentTitle = r.Assessment.Title,
                    UserId = r.UserId,
                    UserName = r.User.Name,
                    Score = r.Score,
                    AttemptDate = r.AttemptDate,
                    Published = r.Published
                })
                .FirstOrDefaultAsync();

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        // GET: api/results/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ResultDto>>> GetResultsForUser(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }

            // Only return published results to students
            var results = await _context.Results
                .Where(r => r.UserId == userId && r.Published)
                .Include(r => r.Assessment)
                .Include(r => r.User)
                .Select(r => new ResultDto
                {
                    ResultId = r.ResultId,
                    AssessmentId = r.AssessmentId,
                    AssessmentTitle = r.Assessment.Title,
                    UserId = r.UserId,
                    UserName = r.User.Name,
                    Score = r.Score,
                    AttemptDate = r.AttemptDate,
                    Published = r.Published
                })
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/results/assessment/{assessmentId}
        [HttpGet("assessment/{assessmentId}")]
        public async Task<ActionResult<IEnumerable<ResultDto>>> GetResultsForAssessment(Guid assessmentId)
        {
            var assessment = await _context.Assessments.FindAsync(assessmentId);
            if (assessment == null)
            {
                return NotFound($"Assessment with ID {assessmentId} not found.");
            }

            var results = await _context.Results
                .Where(r => r.AssessmentId == assessmentId)
                .Include(r => r.Assessment)
                .Include(r => r.User)
                .Select(r => new ResultDto
                {
                    ResultId = r.ResultId,
                    AssessmentId = r.AssessmentId,
                    AssessmentTitle = r.Assessment.Title,
                    UserId = r.UserId,
                    UserName = r.User.Name,
                    Score = r.Score,
                    AttemptDate = r.AttemptDate,
                    Published = r.Published
                })
                .ToListAsync();

            return Ok(results);
        }

        // POST: api/results/publish/{assessmentId}
        [HttpPost("publish/{assessmentId}")]
        public async Task<IActionResult> PublishResults(Guid assessmentId)
        {
            var results = await _context.Results.Where(r => r.AssessmentId == assessmentId).ToListAsync();
            if (results == null || results.Count == 0)
                return NotFound("No results found for this assessment.");

            foreach (var result in results)
            {
                result.Published = true;
            }
            await _context.SaveChangesAsync();

            return Ok("Results published successfully.");
        }

        // POST: api/results/unpublish/{assessmentId}
        [HttpPost("unpublish/{assessmentId}")]
        public async Task<IActionResult> UnpublishResults(Guid assessmentId)
        {
            var results = await _context.Results.Where(r => r.AssessmentId == assessmentId).ToListAsync();
            if (results == null || results.Count == 0)
                return NotFound("No results found for this assessment.");

            foreach (var result in results)
            {
                result.Published = false;
            }
            await _context.SaveChangesAsync();

            return Ok("Results unpublished successfully.");
        }
    }
}
