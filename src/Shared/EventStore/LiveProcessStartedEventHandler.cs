using System;

namespace Shared.EventStore
{
    /// <summary>
    /// The Live Process Started Event Handler deletgate
    /// </summary>
    /// <param name="catchUpSubscriptionId"></param>
    public delegate void LiveProcessStartedEventHandler(Guid catchUpSubscriptionId);
}