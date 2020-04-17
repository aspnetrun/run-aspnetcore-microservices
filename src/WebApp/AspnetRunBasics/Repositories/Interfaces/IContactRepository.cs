using AspnetRunBasics.Entities;
using System.Threading.Tasks;

namespace AspnetRunBasics.Repositories
{
    public interface IContactRepository
    {
        Task<Contact> SendMessage(Contact contact);
        Task<Contact> Subscribe(string address);
    }
}
