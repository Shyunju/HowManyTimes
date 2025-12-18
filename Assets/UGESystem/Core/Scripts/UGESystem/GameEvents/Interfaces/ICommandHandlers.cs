using System;
using System.Collections;
using System.Collections.Generic;

namespace UGESystem
{
    /// <summary>
    /// Defines the <see cref="ICommandHandler"/> interface,
    /// standardizing the <see cref="Execute"/> coroutine method for all classes that handle the logic of specific commands.
    /// </summary>
    public interface ICommandHandler
    {
        /// <summary>
        /// Executes the logic for the given command as a coroutine.
        /// </summary>
        /// <param name="command">The <see cref="IGameEventCommand"/> to execute.</param>
        /// <param name="controller">The <see cref="UGEGameEventController"/> managing the command execution.</param>
        /// <returns>An <see cref="IEnumerator"/> to support coroutine execution.</returns>
        IEnumerator Execute(IGameEventCommand command, UGEGameEventController controller);
    }
}
