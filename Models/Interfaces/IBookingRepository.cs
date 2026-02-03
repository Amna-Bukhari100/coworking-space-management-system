using CoWorkManager.Models;
using System;
using System.Collections.Generic;

namespace CoWorkManager.Models.Interfaces
{
    public interface IBookingRepository
    {
        IEnumerable<Booking> GetAll();
        Booking? GetById(int id);
        void Add(Booking booking);
        void Update(Booking booking);
        void Delete(int id);

        bool IsWorkspaceOccupied(int workspaceId, DateTime start, DateTime end);
    }
}
