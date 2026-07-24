using CodegateTest.Repositories.IRepositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CodegateTest.Areas.Admin
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area(SD.ADMIN_AREA)]
    public class ContactsController : ControllerBase
    {
        private readonly IRepository<Contact> _contactRepository;

        public ContactsController(IRepository<Contact> contactRepository)
        {
            _contactRepository = contactRepository;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAll(int page = 1)
        {
            var contacts = await _contactRepository.GetAsync();

            var totalMessagesCount = contacts.Count();

            int pageSize = 5;

            if (page <= 0)
                page = 1;

            var totalPages = (int)Math.Ceiling(
                totalMessagesCount / (double)pageSize
            );

            var contactQuery = contacts
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new ContactResponce
            {
                items = contactQuery,
                Page = page,
                TotalMessagesCount = totalMessagesCount,
                TotalPages = totalPages
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
          var contact = await _contactRepository.GetOneAsync(e=>e.Id ==id );
            if(contact is null)
            {
                return BadRequest(new APIResponce
                {
                    Message = ["No Contact "]
                });
            }
            return Ok(contact);
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
    int id,
    UpdateContactRequest updateContactRequest)
        {
            var contact = await _contactRepository.GetOneAsync(
                e => e.Id == id
            );

            if (contact is null)
            {
                return BadRequest(new APIResponce
                {
                    Message = ["Contact not found"]
                });
            }

            contact.SenderName =
        updateContactRequest.SenderName ?? contact.SenderName;

            contact.Email =
                updateContactRequest.Email ?? contact.Email;

            contact.Phone =
                updateContactRequest.Phone ?? contact.Phone;

            contact.Subject =
                updateContactRequest.Subject ?? contact.Subject;

            contact.Message =
                updateContactRequest.Message ?? contact.Message;

            _contactRepository.Update(contact);
            await _contactRepository.CommitAsync();

            return Ok(new APIResponce
            {
                Message = ["Contact updated successfully"]
            });
        }



        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var contact = await _contactRepository.GetOneAsync(
                e => e.Id == id
            );

            if (contact is null)
            {
                return BadRequest(new APIResponce
                {
                    Message = ["Contact not found"]
                });
            }

            contact.IsRead = true;

            _contactRepository.Update(contact);
            await _contactRepository.CommitAsync();

            return Ok(new APIResponce
            {
                Message = ["Contact marked as read successfully"]
            });
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var contact = await _contactRepository.GetOneAsync(
                e => e.Id == id
            );

            if (contact is null)
            {
                return BadRequest(new APIResponce
                {
                    Message = ["Contact not found"]
                });
            }

             _contactRepository.Delete(contact);
            await _contactRepository.CommitAsync();

            return Ok(new APIResponce
            {
                Message = ["Contact deleted successfully"]
            });
        }

    }
}
