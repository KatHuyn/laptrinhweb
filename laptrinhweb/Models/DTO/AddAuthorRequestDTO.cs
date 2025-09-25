using System.ComponentModel.DataAnnotations;
namespace laptrinhweb.Models.DTO
{
    public class AddAuthorRequestDTO
    {
        [Required]
        [MinLength(3, ErrorMessage = "Tên tác giả phải có ít nhất 3 ký tự.")]
        public string FullName { set; get; }
    }
}