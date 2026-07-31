using CodegateTest.Models.CodegateTest.Models;
using CodegateTest.Repositories.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CodegateTest.Areas.Admin
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area(SD.ADMIN_AREA)]
    [Authorize(Roles = SD.ADMIN_ROLE)]
    public class ReviewsController : ControllerBase
    {
        private readonly IRepository<Review> _reviewRepository;
       

        public ReviewsController(IRepository<Review> reviewRepository)
        {
            _reviewRepository = reviewRepository;
       
        }

        [HttpGet]
        [Authorize(Roles = SD.ADMIN_ROLE)]
        public async Task<IActionResult> GetAll([FromQuery] ReviewStatus? status)
        {
            if (status.HasValue)
            {
                var reviews = await _reviewRepository.GetAsync(
                    r => r.ReviewStatus == status.Value
                );

                return Ok(new APIResponce
                {
                    StatusCode = StatusCodes.Status200OK,
                    Data = reviews
                });
            }

            var allReviews = await _reviewRepository.GetAsync();

            return Ok(new APIResponce
            {
                StatusCode = StatusCodes.Status200OK,
                Data = allReviews
            });
        }

        [HttpGet("{id}")]
        [Authorize(Roles = SD.ADMIN_ROLE)]
        public async Task<IActionResult> Get(int id)
        {
            var review = await _reviewRepository.GetOneAsync(
                r => r.Id == id
            );

            if (review is null)
            {
                return NotFound(new APIResponce
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = ["Review not found"]
                });
            }

            return Ok(new APIResponce
            {
                StatusCode = StatusCodes.Status200OK,
                Data = review
            });
        }


        [HttpPut("{id}/Approve")]
        [Authorize(Roles = SD.ADMIN_ROLE)]
        public async Task<IActionResult> Approve(int id)
        {
            var review = await _reviewRepository.GetOneAsync(
                r => r.Id == id
            );

            if (review is null)
            {
                return NotFound(new APIResponce
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = ["Review not found"]
                });
            }

            review.ReviewStatus = ReviewStatus.Approved;

            _reviewRepository.Update(review);

            await _reviewRepository.CommitAsync();

            return Ok(new APIResponce
            {
                StatusCode = StatusCodes.Status200OK,
                Message = ["Review approved successfully"]
            });
        }


        [HttpPut("{id}/Reject")]
        [Authorize(Roles = SD.ADMIN_ROLE)]
        public async Task<IActionResult> Reject(int id)
        {
            var review = await _reviewRepository.GetOneAsync(
                r => r.Id == id
            );

            if (review is null)
            {
                return NotFound(new APIResponce
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = ["Review not found"]
                });
            }

            review.ReviewStatus = ReviewStatus.Rejected;

            _reviewRepository.Update(review);

            await _reviewRepository.CommitAsync();

            return Ok(new APIResponce
            {
                StatusCode = StatusCodes.Status200OK,
                Message = ["Review rejected successfully"]
            });
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = SD.ADMIN_ROLE)]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _reviewRepository.GetOneAsync(
                r => r.Id == id
            );

            if (review is null)
            {
                return NotFound(new APIResponce
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = ["Review not found"]
                });
            }

            _reviewRepository.Delete(review);

            await _reviewRepository.CommitAsync();

            return Ok(new APIResponce
            {
                StatusCode = StatusCodes.Status200OK,
                Message = ["Review deleted successfully"]
            });
        }
    }
}
