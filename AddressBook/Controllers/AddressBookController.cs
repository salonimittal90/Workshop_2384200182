using AutoMapper;
using BusinessLayer.Interface;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.Model;
using RepositoryLayer.Entity;

namespace AddressBook.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AddressBookController : ControllerBase
    {
        private readonly IAddressBookBL _addressBL;
        private readonly IMapper _mapper; //  Add AutoMapper

        // Constructor Dependency Injection 
        public AddressBookController(IAddressBookBL addressBL , IMapper mapper)
        {
            _addressBL = addressBL;
            _mapper = mapper;
        }

      

        // GET: Fetch all contacts
        [HttpGet]
        public ActionResult<List<AddressBookEntity>> GetAllContacts()
        {
            return _addressBL.GetAllContacts().ToList(); //  Simple synchronous approach
        }

        //  GET: Get contact by ID
        [HttpGet("{id}")]
        public ActionResult<AddressBookEntity> GetContactById(int id)
        {
            var contact = _addressBL.GetContactById(id);
            return contact;
        }

        // POST: Add a new contact
        //without auto mapper
        /* [HttpPost]
         public ActionResult<AddressBookEntity> AddContact(AddressBookEntity contact)
         {
             var result = _addressBL.AddContact(contact);
             return result;
         }*/

        // DTO validation k liye 
        [HttpPost("DTO")]
        public ActionResult<AddressBookEntity> AddContact([FromBody] AddressBookDTO contact)
        {
            var validator = new AddressBookValidator();
            var validationResult = validator.Validate(contact);
            
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors); // ?? Agar data invalid hai toh error return hoga
            }

            //  *AutoMapper ka use karke DTO ko Entity me convert karo*
            var entity = _mapper.Map<AddressBookEntity>(contact);

            var result = _addressBL.AddContact(entity);
            return CreatedAtAction(nameof(GetContactById), new { id = result.Id }, result);
        }

        //  PUT: Update an existing contact
        [HttpPut("{id}")]
        /* public IActionResult UpdateContact( int id, AddressBookEntity updatedContact)
         {

             var existingContact = _addressBL.UpdateContact( updatedContact);
             return Ok(existingContact);
         }*/


        public IActionResult UpdateContact(int id, [FromBody] AddressBookDTO contactDTO)
        {
            // Validate DTO using FluentValidation
            var validator = new AddressBookValidator();
            var validationResult = validator.Validate(contactDTO);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            //  Get the existing contact
            var existingContact = _addressBL.GetContactById(id);
            if (existingContact == null)
            {
                return NotFound("Contact not found!");
            }

            // mapping  DTO to  Entity 
            _mapper.Map(contactDTO, existingContact);

            // 
            var result = _addressBL.UpdateContact(id, existingContact);

            return Ok(result);
        }

        //  DELETE: Remove a contact
        [HttpDelete("{id}")]
        public IActionResult DeleteContact(int id)
        {
            var contact = _addressBL.DeleteContact(id);
            return NoContent();
        }

       

    }
}
