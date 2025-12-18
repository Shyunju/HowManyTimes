using System;
using System.Collections.Generic;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// A simple manager component that queues actions from the <see cref="UGEDelayedEventBus"/> and invokes them in <c>LateUpdate</c> to prevent race conditions.
    /// <br/>
    /// <see cref="UGEDelayedEventBus"/>의 액션을 큐에 넣고 <c>LateUpdate</c>에서 호출하여 경합 상태를 방지하는 간단한 관리자 컴포넌트입니다.
    /// </summary>
    public class UGEDelayedEventInvoker : MonoBehaviour
    {
        private readonly Queue<Action> _actionQueue = new Queue<Action>();

        private void LateUpdate()
        {
            // 매 프레임의 끝에 큐에 쌓인 모든 액션을 실행
            // Execute all actions queued at the end of each frame
            while (_actionQueue.Count > 0)
            {
                _actionQueue.Dequeue()?.Invoke();
            }
        }

        /// <summary>
        /// Adds an action to the queue to be invoked at the end of the current frame.
        /// 현재 프레임 끝에 호출될 액션을 큐에 추가합니다.
        /// </summary>
        /// <param name="action">The action to enqueue.</param>
        public void Enqueue(Action action)
        {
            _actionQueue.Enqueue(action);
        }
    }
}
