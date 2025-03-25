using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using FM.Infrastructure.Database;
using FM.Application.Interfaces;
using System.Data.Common;
using System.Data;

namespace FM.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApolloContext _db;
    private DbTransaction _transaction;
    private bool _disposed;
    private bool _isCommitted;
    public UnitOfWork(ApolloContext db)
    {
        _db = db;
        _isCommitted = false;
    }

    void IUnitOfWork.BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Serializable)
    {
        if (_db.Database.CurrentTransaction == null)
        {
            _transaction = _db.Database.BeginTransaction(isolationLevel).GetDbTransaction();
        }
    }

    void IUnitOfWork.Commit()
    {
        try
        {
            _db.SaveChanges();
            _transaction?.Commit();
            _isCommitted = true;
        }
        catch
        {
            _transaction?.Rollback();
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    void IUnitOfWork.Rollback()
    {
        if (!_isCommitted && _transaction != null)
        {
            _transaction.Rollback();
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (!_isCommitted)
                {
                    _transaction?.Rollback();
                }
                _transaction?.Dispose();
                _db.Dispose();
            }
            _disposed = true;
        }
    }

    async Task IUnitOfWork.BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.Serializable)
    {
        if (_db.Database.CurrentTransaction == null)
        {
            IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync(isolationLevel);
            _transaction = transaction.GetDbTransaction();
        }
    }

    async Task IUnitOfWork.CommitAsync()
    {
        try
        {
            await _db.SaveChangesAsync();
            await _transaction.CommitAsync();
            _isCommitted = true;
        }
        catch
        {
            await _transaction.RollbackAsync();
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    async Task IUnitOfWork.RollbackAsync()
    {
        if (!_isCommitted && _transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

}
