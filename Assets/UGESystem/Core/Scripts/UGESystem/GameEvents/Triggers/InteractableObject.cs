using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// Component placed on objects that can be interacted with by the player.
    /// It has a unique <see cref="InteractionID"/> for use in the event system.
    /// <br/>
    /// 플레이어가 상호작용할 수 있는 오브젝트에 배치되는 컴포넌트로, 이벤트 시스템에서 사용할 고유한 <see cref="InteractionID"/>를 가집니다.
    /// </summary>
    public class InteractableObject : MonoBehaviour
    {
        [Tooltip("Unique ID to be published to EventBus upon player interaction.")]
        [SerializeField] private string _interactionID;
        /// <summary>
        /// Gets the unique ID of this interactable object.
        /// 이 상호작용 가능한 오브젝트의 고유 ID를 가져옵니다.
        /// </summary>
        public string InteractionID => _interactionID;

        [Tooltip("Used to visually indicate the interactable distance.")]
        [SerializeField] private float _interactionRange = 2f;
        /// <summary>
        /// Gets the range within which interaction is possible.
        /// 상호작용이 가능한 범위를 가져옵니다.
        /// </summary>
        public float InteractionRange => _interactionRange;

        // 상호작용 시 플레이어에게 보여줄 UI 힌트 (예: "F키로 상호작용")
        // UI hint to show to the player upon interaction (e.g., "Press F to interact")
        // TODO: UIManager와 연동하여 구현
        // TODO: Implement in conjunction with UIManager
        // [SerializeField] private GameObject _interactionHintUI;

        private void OnDrawGizmosSelected()
        {
            // 에디터에서 상호작용 가능 거리를 시각적으로 표시
            // Visually display interactable distance in the editor
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _interactionRange);
        }
    }
}
