using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using FM.Infrastructure.Database;
using FM.Application.Interfaces;
using System.Data.Common;
using System.Data;

namespace FM.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApolloContext _context;
    private IDbContextTransaction _transaction;

    public UnitOfWork(ApolloContext context)
    {
        _context = context;
    }

    public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Serializable)
    {
        _transaction = _context.Database.BeginTransaction(isolationLevel);
    }

    public void Commit()
    {
        try
        {
            _context.SaveChanges();
            _transaction?.Commit();
        }
        catch
        {
            Rollback();
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public void Rollback()
    {
        _transaction?.Rollback();
        _transaction?.Dispose();
        _transaction = null;
    }

    public async Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.Serializable)
    {
        _transaction = await _context.Database.BeginTransactionAsync(isolationLevel);
    }

    public async Task CommitAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
