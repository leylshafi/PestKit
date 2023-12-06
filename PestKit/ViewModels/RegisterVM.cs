using PestKit.Enumerations;
using System.ComponentModel.DataAnnotations;

namespace PestKit.ViewModels
{
    public class RegisterVM
    {
        public IFormFile? ProfilePhoto { get; set; }
        [Required]
        public string Name { get; set; }
        public string Surname { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public Gender Gender { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))] 
        public string ConfirmPassword { get; set; }
    }
}
