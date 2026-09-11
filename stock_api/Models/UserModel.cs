using System.ComponentModel.DataAnnotations;

namespace stock_api.Models
{
    public class UserModel
    {
        [Key]
        public int userId { get; set; }

        public string username { get; set; }
        public string password { get; set; }
    }
}
