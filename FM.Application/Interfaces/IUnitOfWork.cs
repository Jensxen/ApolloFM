using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FM.Application.Interfaces
{
    public interface IUnitOfWork
    {
        void Commit();
        void Rollback();
        void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Serializable);

        Task CommitAsync();
        Task RollbackAsync();
        Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.Serializable);
    }
}
