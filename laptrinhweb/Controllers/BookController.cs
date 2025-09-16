using laptrinhweb.Data;
using laptrinhweb.Models.DTO;
using laptrinhweb.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Thêm dòng này vào

namespace laptrinhweb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public BookController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Action AddBook (request HTTP Post)
        [HttpPost("add-book")]
        public ActionResult AddBook([FromBody] AddBookRequestDTO addBookRequestDTO)
        {
            //map DTO to Domain Model
            var bookDomainModel = new Book
            {
                Title = addBookRequestDTO.Title,
                Description = addBookRequestDTO.Description,
                IsRead = addBookRequestDTO.IsRead,
                DateRead = addBookRequestDTO.DateRead,
                Rate = addBookRequestDTO.Rate,
                Genre = addBookRequestDTO.Genre,
                CoverUrl = addBookRequestDTO.CoverUrl,
                DateAdded = addBookRequestDTO.DateAdded,
                PublisherID = addBookRequestDTO.PublisherID
            };
            //Use Domain Model to create Book
            _dbContext.Books.Add(bookDomainModel);
            _dbContext.SaveChanges();

            foreach (var id in addBookRequestDTO.AuthorIds)
            {
                var _book_author = new Book_Author()
                {
                    BookId = bookDomainModel.Id,
                    AuthorId = id
                };
                _dbContext.Books_Authors.Add(_book_author);
            }
            _dbContext.SaveChanges();

            return Ok();
        }

        // Action GetAllBooks (request HTTP Get)
        [HttpGet("get-all-books")]
        public IActionResult GetAll()
        {
            // 2Lấy dữ liệu từ Database - Domain Model, sử dụng Include để tải các mối quan hệ
            var allBooksDomain = _dbContext.Books
                .Include(b => b.Publisher)
                .Include(b => b.Book_Authors)
                .ThenInclude(ba => ba.Author)
                .ToList();

            //Map domain models to DTOS
            var allBooksDTO = allBooksDomain.Select(book => new BookWithAuthorAndPublisherDTO
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description,
                IsRead = book.IsRead,
                DateRead = book.IsRead ? book.DateRead : null,
                Rate = book.IsRead ? book.Rate : null,
                Genre = book.Genre,
                CoverUrl = book.CoverUrl,
                DateAdded = book.DateAdded,
                PublisherName = book.Publisher.Name,
                AuthorNames = book.Book_Authors.Select(n => n.Author.FullName).ToList()
            }).ToList();

            //return DTOS
            return Ok(allBooksDTO);
        }

        // Action GetBookByld (request HTTP Get)
        [HttpGet]
        [Route("get-book-by-id/{id}")]
        public IActionResult GetBookById([FromRoute] int id)
        {
            // 3Lấy dữ liệu từ Database - Domain Model, sử dụng Include để tải các mối quan hệ
            var bookWithDomain = _dbContext.Books
                .Include(b => b.Publisher)
                .Include(b => b.Book_Authors)
                .ThenInclude(ba => ba.Author)
                .FirstOrDefault(n => n.Id == id);

            if (bookWithDomain == null)
            {
                return NotFound();
            }

            //Map Domain Model to DTOS
            var bookWithIdDTO = new BookWithAuthorAndPublisherDTO()
            {
                Id = bookWithDomain.Id,
                Title = bookWithDomain.Title,
                Description = bookWithDomain.Description,
                IsRead = bookWithDomain.IsRead,
                DateRead = bookWithDomain.DateRead,
                Rate = bookWithDomain.Rate,
                Genre = bookWithDomain.Genre,
                CoverUrl = bookWithDomain.CoverUrl,
                PublisherName = bookWithDomain.Publisher.Name,
                AuthorNames = bookWithDomain.Book_Authors.Select(n => n.Author.FullName).ToList()
            };

            return Ok(bookWithIdDTO);
        }

        // Action UpdateBookByld (request HTTP Put)
        [HttpPut("update-book-by-id/{id}")]
        public IActionResult UpdateBookById(int id, [FromBody] AddBookRequestDTO bookDTO)
        {
            var bookDomain = _dbContext.Books.FirstOrDefault(n => n.Id == id);
            if (bookDomain != null)
            {
                bookDomain.Title = bookDTO.Title;
                bookDomain.Description = bookDTO.Description;
                bookDomain.IsRead = bookDTO.IsRead;
                bookDomain.DateRead = bookDTO.DateRead;
                bookDomain.Rate = bookDTO.Rate;
                bookDomain.Genre = bookDTO.Genre;
                bookDomain.CoverUrl = bookDTO.CoverUrl;
                bookDomain.DateAdded = bookDTO.DateAdded;
                bookDomain.PublisherID = bookDTO.PublisherID;
                _dbContext.SaveChanges();

                var authorDomain = _dbContext.Books_Authors.Where(a => a.BookId == id).ToList();
                if (authorDomain != null)
                {
                    _dbContext.Books_Authors.RemoveRange(authorDomain);
                    _dbContext.SaveChanges();
                }

                foreach (var authorid in bookDTO.AuthorIds)
                {
                    var _book_author = new Book_Author()
                    {
                        BookId = id,
                        AuthorId = authorid
                    };
                    _dbContext.Books_Authors.Add(_book_author);
                }
                _dbContext.SaveChanges();
            }
            return Ok(bookDTO);
        }

        // Action DeleteByld (request HTTP Delete)
        [HttpDelete("delete-book-by-id/{id}")]
        public IActionResult DeleteBookById(int id)
        {
            var bookDomain = _dbContext.Books.FirstOrDefault(n => n.Id == id);
            if (bookDomain != null)
            {
                _dbContext.Books.Remove(bookDomain);
                _dbContext.SaveChanges();
            }
            return Ok();
        }
    }
}