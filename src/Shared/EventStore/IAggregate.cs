﻿using System;
using System.Collections.Generic;
using Shared.EventSourcing;

namespace Shared.EventStore
{
    public interface IAggregate
    {
        /// <summary>
        /// Gets or sets the aggregate identifier.
        /// </summary>
        /// <value>The aggregate identifier.</value>
        Guid AggregateId { get; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        Int32 Version { get; }
        
        /// <summary>
        /// Applies the specified historic event.
        /// </summary>
        /// <param name="historicEvent">The historic event.</param>
        void Apply(DomainEvent historicEvent);

        /// <summary>
        /// Gets the pending events.
        /// </summary>
        /// <returns></returns>
        List<DomainEvent> GetPendingEvents();

        /// <summary>
        /// Gets the name of the stream.
        /// </summary>
        /// <returns></returns>
        String GetStreamName();
    }
}