/* namespace MyWebApp.Models
{
    using System.ComponentModel.DataAnnotations;

    public class User
    {
        public int UserID { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
 */