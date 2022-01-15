namespace MyAzureWebStorageApi.Models
{
    public class UserCreationModel
    {
        public string? Adress { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? LastName { get; set; }
        public int? Age { get; set; }
        public IFormFile? UserImage { get; set; }
    }
}
