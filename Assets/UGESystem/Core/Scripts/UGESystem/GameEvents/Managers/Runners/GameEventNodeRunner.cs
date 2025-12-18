using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// Implementation of <see cref="IEventNodeRunner"/> that starts a <see cref="GameEvent"/> asset
    /// within the <see cref="UGESystemController.Instance.GameEventController"/> and waits for its completion.
    /// </summary>
    public class GameEventNodeRunner : IEventNodeRunner
    {
        /// <summary>
        /// Runs the game event associated with the given node.
        /// It starts the <see cref="GameEvent"/> using the <see cref="UGEGameEventController"/>
        /// and waits until the event is finished, reporting the result via the callback.
        /// </summary>
        /// <param name="node">The <see cref="EventNodeData"/> containing the <see cref="GameEvent"/> to run.</param>
        /// <param name="runner">The <see cref="UGEEventTaskRunner"/> that is managing this node's execution.</param>
        /// <param name="onComplete">Callback action to be invoked upon completion, providing the <see cref="NodeRunResult"/>.</param>
        /// <returns>An IEnumerator for coroutine execution.</returns>
        public IEnumerator Run(EventNodeData node, UGEEventTaskRunner runner, Action<NodeRunResult> onComplete)
        {
            if (node.GameEventAsset == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"GameEventNodeRunner: Node '{node.Name}' has no GameEventAsset assigned.");
#endif
                onComplete(new NodeRunResult { Success = false });
                yield break;
            }

            UGESystemController.Instance.GameEventController.StartEvent(node.GameEventAsset, node.Type, runner.Storyboard);

            bool isEventDone = false;
            NodeRunResult result = new NodeRunResult();

            Action<GameEvent, List<AbstractEventReward>> onFinish = null;
            onFinish = (finishedEvent, rewards) =>
            {
                if (finishedEvent == node.GameEventAsset)
                {
                    result.Rewards = rewards;
                    isEventDone = true;
                }
            };
            
            UGEGameEventController.OnEventFinished += onFinish;
            yield return new WaitUntil(() => isEventDone);
            UGEGameEventController.OnEventFinished -= onFinish;

            onComplete(result);
        }
    }
}