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

        public AddressBookBL(IAddressBookRL addressRL)
        {
            _addressRL = addressRL;
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
            return _addressRL.AddContact(contact);
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
