using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MyTestApiClient.Models;

namespace MyTestApiClient
{
    public interface IMyTestApiClient
    {
        /// <summary>
        /// Get test data
        /// </summary>
        Task<object> GetTest(CancellationToken cancellationToken = default);

    }
}
