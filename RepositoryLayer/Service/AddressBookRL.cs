using RepositoryLayer.Context;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Service
{
    public class AddressBookRL : IAddressBookRL
    {
       private readonly AppDbContext _context;

        public AddressBookRL(AppDbContext context)
        {
            _context = context;
        }


        // Get all contact
        public List<AddressBookEntity> GetAllContacts()
        {
            return _context.AddressBooks.ToList();
        }

        public AddressBookEntity GetContactById(int id)
        {
            return _context.AddressBooks.FirstOrDefault(g => g.Id == id);
        }
        public AddressBookEntity AddContact(AddressBookEntity contact)
        {
            _context.AddressBooks.Add(contact);
            _context.SaveChanges();
            return contact;
        }
        public AddressBookEntity UpdateContact( int id, AddressBookEntity contact)
        {

             _context.AddressBooks.Update(contact);
            _context.SaveChanges();
            return contact;
        }
        public bool DeleteContact(int id)
        {
            var contact = _context.AddressBooks.FirstOrDefault(c => c.Id == id);
            if (contact != null)
            {
                _context.AddressBooks.Remove(contact);
                _context.SaveChanges();
                return true;
            }
            return false;
        }


    }
}
