using CodegateTest.Models.CodegateTest.Models;
using CodegateTest.Repositories.IRepositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CodegateTest.Areas.Student
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area(SD.STUDENT_AREA)]
    public class ReviewsController : ControllerBase
    {
        private readonly IRepository<Review> _reviewRepository;
        private readonly IRepository<Course> _courseRepository;

        public ReviewsController(IRepository<Review> reviewRepository,
             IRepository<Course> courseRepository)
        {
            _reviewRepository = reviewRepository;
            _courseRepository = courseRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
    CreateReviewRequest request)
        {
            var studentId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            if (studentId is null)
            {
                return Unauthorized();
            }

            var course = await _courseRepository.GetOneAsync(
                c => c.Id == request.CourseId
            );

            if (course is null)
            {
                return NotFound(new APIResponce
                {
                    Message = ["Course not found"]
                });
            }

            var existingReview =
                await _reviewRepository.GetOneAsync(
                    r =>
                        r.StudentId == studentId &&
                        r.CourseId == request.CourseId
                );

            if (existingReview is not null)
            {
                return BadRequest(new APIResponce
                {
                    Message =
                    [
                        "You have already reviewed this course"
                    ]
                });
            }

            var review = new Review
            {
                StudentId = studentId,
                CourseId = request.CourseId,
                Feedback = request.Feedback,
                Rating = request.Rating,
                ReviewStatus = ReviewStatus.Pending
            };

            await _reviewRepository.CreateAsync(review);

            await _reviewRepository.CommitAsync();

            return Ok(new APIResponce
            {
                StatusCode = 201,
                Message = ["Review created successfully"]
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
    int id,
    UpdateReviewRequest updateReviewRequest)
        {
            var studentId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            if (studentId is null)
            {
                return Unauthorized();
            }

            var review =
                await _reviewRepository.GetOneAsync(
                    r => r.Id == id
                );

            if (review is null)
            {
                return NotFound(new APIResponce
                {
                    Message = ["Review not found"]
                });
            }

            if (review.StudentId != studentId)
            {
                return Forbid();
            }

            review.Feedback = updateReviewRequest.Feedback;
            review.Rating = updateReviewRequest.Rating;


            review.ReviewStatus = ReviewStatus.Pending;

            review.UpdatedAt = DateTime.UtcNow;

            _reviewRepository.Update(review);

            await _reviewRepository.CommitAsync();

            return Ok(new APIResponce
            {
                StatusCode = 200,
                Message = ["Review updated successfully"]
            });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var studentId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            if (studentId is null)
            {
                return Unauthorized();
            }

            var review =
                await _reviewRepository.GetOneAsync(
                    r => r.Id == id
                );

            if (review is null)
            {
                return NotFound(new APIResponce
                {
                    Message = ["Review not found"]
                });
            }

            if (review.StudentId != studentId)
            {
                return Forbid();
            }

            _reviewRepository.Delete(review);

            await _reviewRepository.CommitAsync();

            return Ok(new APIResponce
            {
                StatusCode = 200,
                Message = ["Review deleted successfully"]
            });
        }


        [HttpGet("Course/{courseId}")]
        public async Task<IActionResult> GetCourseReviews(int courseId)
        {
            var course = await _courseRepository.GetOneAsync(e => e.Id == courseId);

            if (course is null)
            {
                return NotFound(new APIResponce
                {
                    StatusCode = 404,
                    Message = ["Course not found"]
                });
            }

            var reviews = await _reviewRepository.GetAsync(
    e => e.CourseId == courseId &&
         e.ReviewStatus == ReviewStatus.Approved,
            includes: [e => e.Student]
);

            var data = reviews.Select(e => new
            {
                e.Id,
                e.Rating,
                e.Feedback,
                StudentName = $"{e.Student.Fname} {e.Student.Lname}",
                StudentImage = e.Student.ProfileImageUrl,
                e.CreatedAt
            });

            return Ok(new APIResponce
            {
                StatusCode = 200,
                Data = data
            });
        }



    }
}
