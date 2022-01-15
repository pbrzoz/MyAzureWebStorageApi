namespace MyAzureWebStorageApi.Models
{
    public partial class User
    {
        public int Id { get; set; }
        public string? Adress { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? LastName { get; set; }
        public int? Age { get; set; }
        public string? UserImage { get; set; }
    }
}
