using System;
using System.Collections.Generic;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// A static event bus that uses <see cref="UGEDelayedEventInvoker"/> to queue and invoke events during <c>LateUpdate</c>
    /// to ensure predictable execution order.
    /// </summary>
    public static class UGEDelayedEventBus
    {
        private static Dictionary<Type, Delegate> _subscriptions = new Dictionary<Type, Delegate>();

        /// <summary>
        /// Subscribes a callback action to a specific event type, ensuring it is invoked via the <see cref="UGEDelayedEventInvoker"/>.
        /// </summary>
        /// <typeparam name="T">The type of the event, which must implement <see cref="IGameBusEvent"/>.</typeparam>
        /// <param name="action">The action to be invoked when the event of type T is published.</param>
        public static void Subscribe<T>(Action<T> action) where T : IGameBusEvent
        {
            Type eventType = typeof(T);
            if (!_subscriptions.ContainsKey(eventType))
            {
                _subscriptions[eventType] = null;
            }
            _subscriptions[eventType] = (Action<T>)_subscriptions[eventType] + action;
        }

        /// <summary>
        /// Unsubscribes a previously registered callback action from a specific event type.
        /// </summary>
        /// <typeparam name="T">The type of the event, which must implement <see cref="IGameBusEvent"/>.</typeparam>
        /// <param name="action">The action to be unsubscribed.</param>
        public static void Unsubscribe<T>(Action<T> action) where T : IGameBusEvent
        {
            Type eventType = typeof(T);
            if (_subscriptions.ContainsKey(eventType))
            {
                _subscriptions[eventType] = (Action<T>)_subscriptions[eventType] - action;
                if (_subscriptions[eventType] == null)
                {
                    _subscriptions.Remove(eventType);
                }
            }
        }

        /// <summary>
        /// Publishes an event to all subscribed listeners. The event will be enqueued and invoked during <c>LateUpdate</c>
        /// to maintain a predictable execution order.
        /// </summary>
        /// <typeparam name="T">The type of the event, which must implement <see cref="IGameBusEvent"/>.</typeparam>
        /// <param name="eventData">The event data object to be published.</param>
        public static void Publish<T>(T eventData) where T : IGameBusEvent
        {
            if (UGESystemController.Instance == null || UGESystemController.Instance.DelayedEventInvoker == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[EventBus] Publish<{typeof(T).Name}> failed: UGESystemController or DelayedEventInvoker instance does not exist in the scene. The event system requires a UGESystemController component in the scene to function correctly.");
#endif
                return;
            }

            UGESystemController.Instance.DelayedEventInvoker.Enqueue(() =>
            {
                if (_subscriptions.TryGetValue(typeof(T), out Delegate d))
                {
                    if (d is Action<T> action)
                    {
                        action?.Invoke(eventData);
                    }
                }
            });
        }
    }
}