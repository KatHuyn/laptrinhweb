using laptrinhweb.Data;
using laptrinhweb.Models.DTO;
using laptrinhweb.Models.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace laptrinhweb.Repositories
{
    public class SQLBookRepository : IBookRepository
    {
        private readonly AppDbContext _dbContext;
        public SQLBookRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<BookWithAuthorAndPublisherDTO> GetAllBooks(string? filterOn = null, string? filterQuery = null, string? sortBy = null, bool isAscending = true, int pageNumber = 1, int
pageSize = 1000)
        {
            var allBooks = _dbContext.Books.Select(Books => new BookWithAuthorAndPublisherDTO()
            {
                Id = Books.Id,
                Title = Books.Title,
                Description = Books.Description,
                IsRead = Books.IsRead,
                DateRead = Books.IsRead ? Books.DateRead.Value : null,
                Rate = Books.IsRead ? Books.Rate.Value : null,
                Genre = Books.Genre,
                CoverUrl = Books.CoverUrl,
                PublisherName = Books.Publisher.Name,
                AuthorNames = Books.Book_Authors.Select(n => n.Author.FullName).ToList()
            }).AsQueryable();
            //filtering 
            if (string.IsNullOrWhiteSpace(filterOn) == false && string.IsNullOrWhiteSpace(filterQuery) == false)
            {
                if (filterOn.Equals("title", StringComparison.OrdinalIgnoreCase))
                {
                    allBooks = allBooks.Where(x => x.Title.ToLower().Contains(filterQuery.ToLower()));
                }
            }
            //sorting 
            if (string.IsNullOrWhiteSpace(sortBy) == false)
            {
                if (sortBy.Equals("title", StringComparison.OrdinalIgnoreCase))
                {
                    allBooks = isAscending ? allBooks.OrderBy(x => x.Title) : allBooks.OrderByDescending(x => x.Title);
                }
            }
            //pagination 
            var skipResults = (pageNumber - 1) * pageSize;
            return allBooks.Skip(skipResults).Take(pageSize).ToList();

        }
        public BookWithAuthorAndPublisherDTO GetBookById(int id)
        {
            var bookWithDomain = _dbContext.Books.Where(n => n.Id == id);
            //Map Domain Model to DTOs 
            var bookWithIdDTO = bookWithDomain.Select(book => new BookWithAuthorAndPublisherDTO()
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description,
                IsRead = book.IsRead,
                DateRead = book.DateRead,
                Rate = book.Rate,
                Genre = book.Genre,
                CoverUrl = book.CoverUrl,
                PublisherName = book.Publisher.Name,
                AuthorNames = book.Book_Authors.Select(n => n.Author.FullName).ToList()
            }).FirstOrDefault();
            return bookWithIdDTO;
        }
        public AddBookRequestDTO AddBook(AddBookRequestDTO addBookRequestDTO)
        {
            // Kiểm tra PublisherID có tồn tại không
            var publisherExists = _dbContext.Publishers.Any(p => p.Id == addBookRequestDTO.PublisherID);
            if (!publisherExists)
            {
                throw new ArgumentException("Publisher ID does not exist.");
            }

            // Mỗi sách phải có ít nhất 1 tác giả.
            if (addBookRequestDTO.AuthorIds == null || addBookRequestDTO.AuthorIds.Count == 0)
            {
                throw new ArgumentException("A book must have at least one author.");
            }

            // Kiểm tra tên sách có bị trùng với cùng một nhà xuất bản không
            var bookTitleExists = _dbContext.Books.Any(b => b.Title == addBookRequestDTO.Title && b.PublisherID == addBookRequestDTO.PublisherID);
            if (bookTitleExists)
            {
                throw new InvalidOperationException("A book with the same title already exists for this publisher.");
            }

            // Kiểm tra giới hạn xuất bản của Nhà xuất bản trong 1 năm (100 cuốn)
            var currentYear = DateTime.Now.Year;
            var publishedBooksThisYear = _dbContext.Books
                .Count(b => b.PublisherID == addBookRequestDTO.PublisherID && b.DateAdded.Year == currentYear);
            if (publishedBooksThisYear >= 100)
            {
                throw new InvalidOperationException($"Publisher with ID {addBookRequestDTO.PublisherID} đã đạt giới hạn xuất bản 100 cuốn trong năm nay.");
            }

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

            _dbContext.Books.Add(bookDomainModel);
            _dbContext.SaveChanges();

            foreach (var id in addBookRequestDTO.AuthorIds)
            {
                // Kiểm tra tác giả có tồn tại không
                var authorExists = _dbContext.Authors.Any(a => a.Id == id);
                if (!authorExists)
                {
                    throw new ArgumentException($"Author with ID {id} does not exist.");
                }

                // Kiểm tra số lượng sách của tác giả (10 cuốn)
                var bookCount = _dbContext.Books_Authors.Count(ba => ba.AuthorId == id);
                if (bookCount >= 10)
                {
                    throw new InvalidOperationException($"Author with ID {id} has already written the maximum number of books (10).");
                }

                // Kiểm tra mối quan hệ đã tồn tại không
                var existingRelationship = _dbContext.Books_Authors.Any(ba => ba.BookId == bookDomainModel.Id && ba.AuthorId == id);
                if (existingRelationship)
                {
                    throw new InvalidOperationException($"Author with ID {id} is already assigned to this book.");
                }

                var bookAuthor = new Book_Author()
                {
                    BookId = bookDomainModel.Id,
                    AuthorId = id
                };
                _dbContext.Books_Authors.Add(bookAuthor);
            }

            _dbContext.SaveChanges();
            return addBookRequestDTO;
        }

        public AddBookRequestDTO? UpdateBookById(int id, AddBookRequestDTO bookDTO)
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
            }

            var authorDomain = _dbContext.Books_Authors.Where(a => a.BookId == id).ToList();
            if (authorDomain != null)
            {
                _dbContext.Books_Authors.RemoveRange(authorDomain);
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
            return bookDTO;
        }
        public Book? DeleteBookById(int id)
        {
            var bookDomain = _dbContext.Books.FirstOrDefault(n => n.Id == id);
            if (bookDomain != null)
            {
                _dbContext.Books.Remove(bookDomain);
                _dbContext.SaveChanges();
            }
            return bookDomain;
        }
    }
}

