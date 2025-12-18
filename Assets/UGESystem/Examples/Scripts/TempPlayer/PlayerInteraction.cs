using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// Temporary player script that handles user input to detect and trigger an <see cref="InteractionTriggeredEvent"/>
    /// when looking at an <see cref="InteractableObject"/>.
    /// <br/>
    /// InteractableObject를 볼 때 <see cref="InteractionTriggeredEvent"/>를 감지하고 발생시키기 위한 사용자 입력을 처리하는 임시 플레이어 스크립트입니다.
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [Tooltip("Maximum distance at which interaction will be detected")]
        [SerializeField] private float _interactionDistance = 3f;
        
        [Tooltip("Select the layer to which interactable objects belong.")]
        [SerializeField] private LayerMask _interactableLayer;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                TryInteract();
            }
        }

        private void TryInteract()
        {
            RaycastHit hit;
            
            // 플레이어의 위치에서 정면으로 레이 발사
            // Fire a ray forward from the player's position
            Vector3 rayOrigin = transform.position;
            Vector3 rayDirection = transform.forward;

            // Raycast를 사용하여 상호작용 가능한 오브젝트 감지
            // Detect interactable objects using Raycast
            if (Physics.Raycast(rayOrigin, rayDirection, out hit, _interactionDistance, _interactableLayer))
            {
                InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();
                if (interactable != null)
                {
                    if (string.IsNullOrEmpty(interactable.InteractionID))
                    {
#if UNITY_EDITOR
                        Debug.LogWarning($"InteractableObject on '{hit.collider.name}' has an empty InteractionID.", hit.collider.gameObject);
#endif
                        return;
                    }

                    UGEDelayedEventBus.Publish(new InteractionTriggeredEvent(interactable.InteractionID));
                }
            }
        }

        // 에디터의 씬 뷰에서 상호작용 범위를 시각적으로 표시합니다.
        // Visually displays the interaction range in the editor's scene view.
        // 이 기즈모는 플레이어 오브젝트를 선택해야만 보입니다.
        // This gizmo is only visible when the player object is selected.
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Vector3 rayOrigin = transform.position;
            Vector3 rayDirection = transform.forward;
            Gizmos.DrawRay(rayOrigin, rayDirection * _interactionDistance);
        }
    }
}