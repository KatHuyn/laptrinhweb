using laptrinhweb.Data;
using laptrinhweb.Models.Domain;
using laptrinhweb.Models.DTO;
using laptrinhweb.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace laptrinhweb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublishersController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IPublisherRepository _publisherRepository;

        public PublishersController(AppDbContext dbContext, IPublisherRepository publisherRepository)
        {
            _dbContext = dbContext;
            _publisherRepository = publisherRepository;
        }

        [HttpGet("get-all-publisher")]
        public IActionResult GetAllPublisher()
        {
            var allPublishers = _publisherRepository.GetAllPublishers();
            return Ok(allPublishers);
        }

        [HttpGet("get-publisher-by-id/{id}")]
        public IActionResult GetPublisherById(int id)
        {
            var publisherWithId = _publisherRepository.GetPublisherById(id);
            return Ok(publisherWithId);
        }

        [HttpPost("add-publisher")]
        public ActionResult AddPublisher([FromBody] AddPublisherRequestDTO addPublisherRequestDTO)
        {
            try
            {
                var publisherAdd = _publisherRepository.AddPublisher(addPublisherRequestDTO);
                return Ok(publisherAdd);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("Conflict", ex.Message);
                return Conflict(ModelState); // Trả về 409 Conflict
            }
        }

        [HttpPut("update-publisher-by-id/{id}")]
        public IActionResult UpdatePublisherById(int id, [FromBody] PublisherNoIdDTO publisherDTO)
        {
            var publisherUpdate = _publisherRepository.UpdatePublisherById(id, publisherDTO);
            return Ok(publisherUpdate);
        }

        [HttpDelete("delete-publisher-by-id/{id}")]
        public ActionResult DeletePublisherById(int id)
        {
            try
            {
                var publisherDelete = _publisherRepository.DeletePublisherById(id);
                if (publisherDelete == null)
                {
                    return NotFound($"Không tìm thấy Nhà xuất bản với ID {id}.");
                }
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("Publisher", ex.Message);
                return BadRequest(ModelState); // Trả về 400 Bad Request
            }
        }
    }
}