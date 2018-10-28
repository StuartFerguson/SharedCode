using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Shared.EventStore
{
    public interface IProjectionManager
    {
        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="projectionName">Name of the projection.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<T> GetState<T>(String projectionName, CancellationToken cancellationToken);

        /// <summary>
        /// Runs the transient query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryName">Name of the query.</param>
        /// <param name="queryContents">The query contents.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<T> RunTransientQuery<T>(String queryName, String queryContents, CancellationToken cancellationToken);

        /// <summary>
        /// Runs the transient query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fullFilePath">The full file path.</param>
        /// <param name="queryReplacements">The query replacements.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<T> RunTransientQuery<T>(String fullFilePath, Dictionary<String, String> queryReplacements,
            CancellationToken cancellationToken);

        /// <summary>
        /// Gets the state of the partition.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="projectionName">Name of the projection.</param>
        /// <param name="partitionId">The partition identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<T> GetPartitionState<T>(String projectionName, String partitionId, CancellationToken cancellationToken);
    }
}
