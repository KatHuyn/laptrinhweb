using laptrinhweb.Data;
using laptrinhweb.Models.Domain;
using laptrinhweb.Models.DTO;
using System;
using System.Linq;

namespace laptrinhweb.Repositories
{
    public class SQLPublisherRepository : IPublisherRepository
    {
        private readonly AppDbContext _dbContext;
        public SQLPublisherRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<PublisherDTO> GetAllPublishers()
        {
            var allPublishersDomain = _dbContext.Publishers.ToList();

            var allPublisherDTO = new List<PublisherDTO>();
            foreach (var publisherDomain in allPublishersDomain)
            {
                allPublisherDTO.Add(new PublisherDTO()
                {
                    Id = publisherDomain.Id,
                    Name = publisherDomain.Name
                });
            }
            return allPublisherDTO;
        }

        public PublisherNoIdDTO GetPublisherById(int id)
        {
            var publisherWithIdDomain = _dbContext.Publishers.FirstOrDefault(x => x.Id == id);
            if (publisherWithIdDomain != null)
            {
                var publisherNoIdDTO = new PublisherNoIdDTO
                {
                    Name = publisherWithIdDomain.Name,
                };
                return publisherNoIdDTO;
            }
            return null;
        }
        public AddPublisherRequestDTO AddPublisher(AddPublisherRequestDTO addPublisherRequestDTO)
        {
            // Kiểm tra tên nhà xuất bản có bị trùng không
            var existingPublisher = _dbContext.Publishers.Any(p => p.Name == addPublisherRequestDTO.Name);
            if (existingPublisher)
            {
                throw new InvalidOperationException("Tên Nhà xuất bản đã tồn tại."); // Bắt lỗi 409 ở Controller
            }

            var publisherDomainModel = new Publisher
            {
                Name = addPublisherRequestDTO.Name,
            };

            _dbContext.Publishers.Add(publisherDomainModel);
            _dbContext.SaveChanges();
            return addPublisherRequestDTO;
        }

        // Thay thế DeletePublisherById
        public Publisher? DeletePublisherById(int id)
        {
            var publisherDomain = _dbContext.Publishers.FirstOrDefault(n => n.Id == id);
            if (publisherDomain != null)
            {
                // Kiểm tra các sách tham chiếu tới Publisher này
                var hasBooks = _dbContext.Books.Any(b => b.PublisherID == id);
                if (hasBooks)
                {
                    throw new InvalidOperationException("Không thể xóa Nhà xuất bản có sách liên kết."); // Bắt lỗi 400 ở Controller
                }

                _dbContext.Publishers.Remove(publisherDomain);
                _dbContext.SaveChanges();
            }
            return publisherDomain;
        }

        public PublisherNoIdDTO UpdatePublisherById(int id, PublisherNoIdDTO publisherNoIdDTO)
        {
            var publisherDomain = _dbContext.Publishers.FirstOrDefault(n => n.Id == id);
            if (publisherDomain != null)
            {
                publisherDomain.Name = publisherNoIdDTO.Name;
                _dbContext.SaveChanges();
                return publisherNoIdDTO; // Trả về đối tượng sau khi cập nhật
            }
            return null;
        }

    
    }
}