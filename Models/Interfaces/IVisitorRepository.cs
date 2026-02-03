using CoWorkManager.Models;
using System.Collections.Generic;

namespace CoWorkManager.Models.Interfaces
{
    public interface IVisitorRepository
    {
        IEnumerable<Visitor> GetAll();
        Visitor? GetById(int id);
        void Add(Visitor visitor);
        void Update(Visitor visitor);
        void Delete(int id);
        IEnumerable<Visitor> GetByBookingId(int bookingId);
    }
}
