using System;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// Centralized manager for event-related input, handling <see cref="ContinueDialogue"/> and <see cref="SkipCinematic"/> actions from Unity's Input System.
    /// <br/>
    /// 이벤트 관련 입력을 위한 중앙 관리자로, Unity의 입력 시스템에서 <see cref="OnContinueDialogue"/> 및 <see cref="OnSkipCinematic"/> 액션을 처리합니다.
    /// </summary>
    public class UGEInputManager : MonoBehaviour
    {
        /// <summary>
        /// Event triggered when the "Continue Dialogue" input action is performed.
        /// "대화 계속" 입력 액션이 수행될 때 발생하는 이벤트입니다.
        /// </summary>
        public event Action OnContinueDialogue;
        /// <summary>
        /// Event triggered when the "Skip Cinematic" input action is performed.
        /// "시네마틱 스킵" 입력 액션이 수행될 때 발생하는 이벤트입니다.
        /// </summary>
        public event Action OnSkipCinematic;

        private InputSystem_Actions _actions;

        private bool _isContinueListenerActive = false;
        private bool _isSkipListenerActive = false;

        private void Awake()
        {
            _actions = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            _actions.UI_Event.Enable();
            // _actions.UI_Event.ContinueDialogue.performed += HandleContinueDialogue;
            _actions.UI_Event.SkipCinematic.performed += HandleSkipCinematic;
        }

        private void OnDisable()
        {
            _actions.UI_Event.Disable();
            // _actions.UI_Event.ContinueDialogue.performed -= HandleContinueDialogue;
            _actions.UI_Event.SkipCinematic.performed -= HandleSkipCinematic;
        }

        /// <summary>
        /// Enables or disables the listener for the "Continue Dialogue" input action.
        /// "대화 계속" 입력 액션에 대한 리스너를 활성화하거나 비활성화합니다.
        /// </summary>
        /// <param name="enable">True to enable the listener, false to disable.</param>
        public void EnableDialogueContinueListener(bool enable)
        {
            _isContinueListenerActive = enable;
        }

        /// <summary>
        /// Enables or disables the listener for the "Skip Cinematic" input action.
        /// "시네마틱 스킵" 입력 액션에 대한 리스너를 활성화하거나 비활성화합니다.
        /// </summary>
        /// <param name="enable">True to enable the listener, false to disable.</param>
        public void EnableCinematicSkipListener(bool enable)
        {
            _isSkipListenerActive = enable;
        }

        /// <summary>
        /// Manually triggers the OnContinueDialogue event. Can be used by UI buttons.
        /// 수동으로 OnContinueDialogue 이벤트를 트리거합니다. UI 버튼 등에서 사용할 수 있습니다.
        /// </summary>
        public void TriggerContinueDialogue()
        {
            // We check the active flag here as well to ensure UI buttons don't bypass the control.
            // UI 버튼이 제어를 우회하지 않도록 여기서도 활성화 플래그를 확인합니다.
            if (_isContinueListenerActive)
            {
                OnContinueDialogue?.Invoke();
            }
        }

        private void HandleContinueDialogue(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (_isContinueListenerActive)
            {
                OnContinueDialogue?.Invoke();
            }
        }

        private void HandleSkipCinematic(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (_isSkipListenerActive)
            {
                OnSkipCinematic?.Invoke();
            }
        }
    }
}