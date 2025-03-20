using BusinessLayer.Interface;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Service
{
    public class AddressBookBL : IAddressBookBL
    {
        IAddressBookRL _addressRL;
        private readonly RabbitMQService _rabbitMQService;


        public AddressBookBL(IAddressBookRL addressRL, RabbitMQService rabbitMQService)
        {
            _addressRL = addressRL;
            _rabbitMQService = rabbitMQService;
        }
        public List<AddressBookEntity> GetAllContacts()
        {
            return _addressRL.GetAllContacts();
        }
        public AddressBookEntity GetContactById(int id)
        {
            return _addressRL.GetContactById(id);
        }
        public AddressBookEntity AddContact(AddressBookEntity contact)
        {
            // return _addressRL.AddContact(contact);


            // rabbitmq se contact add hone lka event publish krna 
            var addedContact = _addressRL.AddContact(contact);

            // RabbitMQ me event publish karo
            var message = $"New contact added: {addedContact.Name}, {addedContact.Email}";
            _rabbitMQService.PublishMessage("ContactAddedQueue", message);

            return addedContact;
        }
        public AddressBookEntity UpdateContact( int id, AddressBookEntity contact)
        {
            return _addressRL.UpdateContact(id , contact);
        }

        public bool DeleteContact(int id)
        {
            return _addressRL.DeleteContact(id);
        }
    }
}
