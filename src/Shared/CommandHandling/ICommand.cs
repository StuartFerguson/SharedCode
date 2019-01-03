using System;

namespace Shared.CommandHandling
{
    public interface ICommand
    {
        /// <summary>
        /// Gets the command identifier.
        /// </summary>
        /// <value>
        /// The command identifier.
        /// </value>
        Guid CommandId { get; }
    }
}