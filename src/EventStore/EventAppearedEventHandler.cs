using System;
using System.Threading.Tasks;

namespace Shared.EventStore
{
    /// <summary>
    /// The Event Appeared Event Handler delegate
    /// </summary>
    /// <param name="subscription">The subscription.</param>
    /// <returns></returns>
    public delegate Task<Boolean> EventAppearedEventHandler(SubscriptionDataTransferObject subscription);
}