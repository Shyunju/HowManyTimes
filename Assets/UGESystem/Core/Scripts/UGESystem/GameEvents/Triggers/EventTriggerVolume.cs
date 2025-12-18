using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// Component that, when a player enters a specific area (Trigger Collider),
    /// publishes an <see cref="AreaEnteredEvent"/> containing the configured <see cref="_triggerID"/>.
    /// <br/>
    /// 플레이어가 특정 영역(Trigger Collider)에 진입했을 때, 설정된 <see cref="_triggerID"/>를 담아 <see cref="AreaEnteredEvent"/>를 발행하는 컴포넌트입니다.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class EventTriggerVolume : MonoBehaviour
    {
        [Tooltip("Must match the ID set in the Storyboard's AreaEnteredCondition.")]
        [SerializeField] private string _triggerID;

        [Tooltip("Set the event to occur only once.")]
        [SerializeField] private bool _triggerOnce = true;

        private bool _hasBeenTriggered = false;

        private void Awake()
        {
            // 트리거가 아닌 콜라이더가 실수로 할당되는 것을 방지
            // Prevents a non-trigger collider from being accidentally assigned
            var col = GetComponent<Collider>();
            if (!col.isTrigger)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"EventTriggerVolume on '{gameObject.name}' has a non-trigger Collider. Forcing isTrigger to true.", this);
#endif
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_triggerOnce && _hasBeenTriggered)
            {
                return;
            }

            // "Player" 태그를 가진 오브젝트만 감지
            // Detect only objects with the "Player" tag
            if (other.CompareTag("Player"))
            {
                if (string.IsNullOrEmpty(_triggerID))
                {
#if UNITY_EDITOR
                    Debug.LogError($"EventTriggerVolume on '{gameObject.name}' has an empty TriggerID.", this);
#endif
                    return;
                }

                UGEDelayedEventBus.Publish(new AreaEnteredEvent(_triggerID));
                _hasBeenTriggered = true;
            }
        }
    }
}
