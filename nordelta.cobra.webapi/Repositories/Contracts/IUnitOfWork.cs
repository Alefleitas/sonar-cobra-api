using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Repositories.Contracts
{
    public interface IUnitOfWork: IDisposable
    {
        void RunWithExecutionStrategy(Action action, CancellationToken cancellationToken = default(CancellationToken));
        Task RunWithExecutionStrategyAsync(Func<Task> action, CancellationToken cancellationToken = default(CancellationToken));
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
        int SaveChanges();
        bool HasActiveTransaction { get; }
        IDbContextTransaction GetCurrentTransaction();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitAsync(IDbContextTransaction transaction);
        IDbContextTransaction BeginTransaction();
        void Commit(IDbContextTransaction transaction);
    }
}
