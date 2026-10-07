using CodegateTest.Repositories.IRepositories;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodegateTest
{
    [Route("api/[controller]")]
    [ApiController]

    public class ContactsController : ControllerBase
    {
        private readonly IRepository<Contact> _contactRepository;

        public ContactsController(IRepository<Contact> contactRepository)
        {
            _contactRepository = contactRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateContactRequest createContactRequest)
        {

           var contact = createContactRequest.Adapt<Contact>();
           await _contactRepository.CreateAsync(contact);
           await _contactRepository.CommitAsync();

            return StatusCode(StatusCodes.Status201Created, new APIResponce
            {
                StatusCode = 201,
                Message = ["Contact message created successfully"]
            });
        }

    }
}

