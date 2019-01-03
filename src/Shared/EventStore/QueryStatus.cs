using System;

namespace Shared.EventStore
{
    public class QueryStatus
    {
        /// <summary>
        /// Gets or sets the progress.
        /// </summary>
        /// <value>
        /// The progress.
        /// </value>
        public String Progress { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public String Status { get; set; }
    }
}