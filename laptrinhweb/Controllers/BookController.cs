using laptrinhweb.Data;
using laptrinhweb.Models.Domain;
using laptrinhweb.Models.DTO;
using laptrinhweb.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.RegularExpressions;
using laptrinhweb.CustomActionFilter;
using Microsoft.AspNetCore.Authorization;

namespace WebAPI_simple.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BooksController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IBookRepository _bookRepository;

        public BooksController(AppDbContext dbContext, IBookRepository bookRepository)
        {
            _dbContext = dbContext;
            _bookRepository = bookRepository;
        }

        [HttpGet("get-all-books")]
        public IActionResult GetAll([FromQuery]String? filterOn, [FromQuery]String? filterQuery, [FromQuery] string? sortBy, [FromQuery] bool isAscending, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
        {
            // su dung reposity pattern  Quer
            var allBooks = _bookRepository.GetAllBooks(filterOn, filterQuery, sortBy, isAscending, pageNumber,pageSize);
            return Ok(allBooks);
        }

        [HttpGet]
        [Route("get-book-by-id/{id}")]
        public IActionResult GetBookById([FromRoute] int id)
        {
            var bookWithIdDTO = _bookRepository.GetBookById(id);
            return Ok(bookWithIdDTO);
        }
        private bool ValidateTitleWithoutSpecialChars(string title) 
        {
            // Cho phép chữ cái, số và khoảng trắng
            string pattern = @"^[a-zA-Z0-9\s]*$";
            return Regex.IsMatch(title, pattern);
        }
        [HttpPost("add-book")]
        [ValidateModel]
        public IActionResult AddBook([FromBody] AddBookRequestDTO addBookRequestDTO)
        {
            //Kiểm tra ký tự đặc biệt
            if (!ValidateTitleWithoutSpecialChars(addBookRequestDTO.Title))
            {
                ModelState.AddModelError("Title", "Title không được chứa ký tự đặc biệt.");
                return BadRequest(ModelState);
            }

            try
            {
                var bookAdd = _bookRepository.AddBook(addBookRequestDTO);
                return Ok(bookAdd);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("Validation", ex.Message);
                return BadRequest(ModelState); // Trả về 400 Bad Request
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("Conflict", ex.Message);
                return Conflict(ModelState); // Trả về 409 Conflict
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi server không xác định.");
            }
        }

        [HttpPut("update-book-by-id/{id}")]
        public IActionResult UpdateBookById(int id, [FromBody] AddBookRequestDTO bookDTO)
        {
            var updateBook = _bookRepository.UpdateBookById(id, bookDTO);
            return Ok(updateBook);
        }
        [HttpDelete("delete-book-by-id/{id}")]
        public IActionResult DeleteBookById(int id)
        {
            var deleteBook = _bookRepository.DeleteBookById(id);
            return Ok(deleteBook);
        }
    }
}