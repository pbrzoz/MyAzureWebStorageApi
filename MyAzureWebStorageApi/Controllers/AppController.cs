using AutoMapper;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyAzureWebStorageApi.Models;

namespace MyAzureWebStorageApi.Controllers
{
    [ApiController]
    public class AppController : ControllerBase
    {
        private readonly string accountName, accountKey, blobstoragecon, containerName, blobEndpoint;

        private readonly AzureTestContext _context;
        private readonly ILogger<AppController> _logger;

        private readonly BlobServiceClient blobClient;
        private readonly BlobContainerClient container;
        private readonly IMapper _mapper;
        public AppController(ILogger<AppController> logger, IConfiguration configuration, IMapper mapper, AzureTestContext context)
        {
            //Configuration
            accountName = configuration.GetSection("Azure").GetValue<string>("AccountName");
            accountKey = configuration.GetSection("Azure").GetValue<string>("AccountKey");
            blobstoragecon = configuration.GetSection("Azure").GetValue<string>("BlobStorageConnectionString");
            containerName = configuration.GetSection("Azure").GetValue<string>("ContainerName");
            blobEndpoint = configuration.GetSection("Azure").GetValue<string>("BlobEndpoint");

            _logger = logger;
            _context = context;
            _mapper = mapper;
            blobClient = new BlobServiceClient(blobstoragecon);
            container = blobClient.GetBlobContainerClient(containerName);
        }

        [HttpGet(nameof(GetUsers))]
        public async Task<IEnumerable<UserDTO>> GetUsers()
        {
            User[] users = await _context.Users.ToArrayAsync();
            return _mapper.Map<User[], UserDTO[]>(users);
        }

        [HttpPut(nameof(UpdateUser))]
        public async Task<IActionResult> UpdateUser([FromForm] UserEditDTO editUser)
        {
            User? user = await _context.Users.SingleOrDefaultAsync(x => x.Id == editUser.Id);
            if (user == null)
            {
                return NotFound();
            }

            user.UserName = editUser.UserName;
            user.Email = editUser.Email;
            user.Adress = editUser.Adress;
            user.Age = editUser.Age;
            user.LastName = editUser.LastName;

            if (editUser.UserImage != null)
            {
                if (user.UserImage != null)
                {
                    BlobClient oldBlob = container.GetBlobClient(user.UserImage.Split("/")[4]);
                    await oldBlob.DeleteIfExistsAsync();
                }

                string name = DateTimeOffset.Now.ToUnixTimeSeconds() + "_" + editUser.UserImage.FileName;
                BlobClient blobcl = container.GetBlobClient(name);
                await blobcl.UploadAsync(editUser.UserImage.OpenReadStream());
                user.UserImage = $"{blobEndpoint}/{container.Name}/{name}";
            }
            else
            {
                if (user.UserImage != null)
                {
                    BlobClient oldBlob = container.GetBlobClient(user.UserImage.Split("/")[4]);
                    await oldBlob.DeleteIfExistsAsync();
                }
                user.UserImage = null;
            }

            await _context.SaveChangesAsync();
            return Ok($"Updated user. ID:{editUser.Id}");
        }

        [HttpGet(nameof(Get)+"/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            User? user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                if (user.UserImage != null)
                {                    
                    user.UserImage = GetSasForBlob(user.UserImage).ToString();
                }

                return Ok(user);
            }
            return NotFound();
        }

        [HttpPost(nameof(AddUser))]
        public async Task<IActionResult> AddUser([FromForm] UserCreationModel user)
        {
            User userMapped = _mapper.Map<UserCreationModel, User>(user);
            if (user.UserImage != null)
            {
                string name = DateTimeOffset.Now.ToUnixTimeSeconds() + "_" + user.UserImage.FileName;
                BlobClient blobcl = container.GetBlobClient(name);
                await blobcl.UploadAsync(user.UserImage.OpenReadStream());
                userMapped.UserImage = $"{blobEndpoint}/{container.Name}/{name}";
            }
            else
            {
                userMapped.UserImage = null;
            }

            _context.Add(userMapped);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = userMapped.Id }, userMapped);
        }

        private Uri GetSasForBlob(string blobname, BlobAccountSasPermissions permissions = BlobAccountSasPermissions.Read)
        {
            DateTimeOffset timeNow = DateTimeOffset.UtcNow;
            StorageSharedKeyCredential credential = new StorageSharedKeyCredential(accountName, accountKey);
            BlobSasBuilder sas = new BlobSasBuilder
            {
                BlobName = blobname,
                BlobContainerName = container.Name,
                StartsOn = timeNow,
                ExpiresOn = timeNow.AddHours(2),
                Resource = "b",
                Protocol = SasProtocol.Https
            };
            sas.SetPermissions(permissions);

            BlobUriBuilder sasUri = new BlobUriBuilder(new Uri(blobname));
            sasUri.Sas = sas.ToSasQueryParameters(credential);
            return sasUri.ToUri();
        }
    }
}