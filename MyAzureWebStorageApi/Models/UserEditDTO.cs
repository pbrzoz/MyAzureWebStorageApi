namespace MyAzureWebStorageApi.Models
{
    public class UserEditDTO
    {
        public int Id { get; set; }
        public string? Adress { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? LastName { get; set; }
        public int? Age { get; set; }
        public IFormFile? UserImage { get; set; }
    }
}
