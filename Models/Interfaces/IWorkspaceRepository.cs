using CoWorkManager.Models;
using System.Collections.Generic;

namespace CoWorkManager.Models.Interfaces
{
    public interface IWorkspaceRepository
    {
        IEnumerable<Workspace> GetAll();
        Workspace? GetById(int id);
        void Add(Workspace workspace);
        void Update(Workspace workspace);
        void Delete(int id);
    }
}
