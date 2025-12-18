using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace UGESystem
{
    /// <summary>
    /// Manages all UI elements related to the event system.
    /// This includes dialogue boxes, choice panels, cinematic text, and background displays.
    /// </summary>
    public class UGEUIManager : MonoBehaviour
    {
        [Header("Dialogue Elements")]
        [SerializeField] private GameObject _dialogueBox;
        [SerializeField] private TextMeshProUGUI _characterNameText;
        [SerializeField] private TextMeshProUGUI _dialogueText;
        [SerializeField] private Button _continueButton;
        [Tooltip("Optional: Assign the adjuster component if you want to support text pagination.")]
        [SerializeField] private TMP_ContentSizeAdjuster _dialogueSizeAdjuster;

        [Header("Choice Elements")]
        [SerializeField] private GameObject _choiceBox;
        [SerializeField] private List<Button> _choiceButtons;

        [Header("Cinematic Text Elements")]
        [SerializeField] private GameObject _cinematicTextBox;
        [SerializeField] private TextMeshProUGUI _cinematicTextMesh;

        [Header("Background Elements")]
        [SerializeField] private RawImage _backgroundRawImage;
        [SerializeField] private VideoPlayer _backgroundVideoPlayer;
        [SerializeField] private RenderTexture _videoRenderTexture;

        private void Start()
        {
            if (_continueButton != null)
            {
                // The button click now triggers the central "Continue Dialogue" action via its public method.
                // This unifies input from this button and general screen clicks.
                // 이제 버튼 클릭은 public 메서드를 통해 중앙 "대화 계속" 액션을 트리거합니다.
                // 이를 통해 이 버튼의 입력과 일반 화면 클릭의 입력이 통합됩니다.
                _continueButton.onClick.AddListener(() =>
                {
                    var inputManager = UGESystemController.Instance.InputManager;
                    if (inputManager != null)
                    {
                        inputManager.TriggerContinueDialogue();
                    }
                });
            }

            // 초기 상태 설정
            // Initial state setup
            if (_backgroundRawImage != null) _backgroundRawImage.gameObject.SetActive(false);
            if (_backgroundVideoPlayer != null) _backgroundVideoPlayer.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            // Subscribe to the central dialogue continue event.
            // 중앙 대화 계속 이벤트를 구독합니다.
            var inputManager = UGESystemController.Instance.InputManager;
            if (inputManager != null)
            {
                inputManager.OnContinueDialogue += OnContinueClicked;
            }
        }

        private void OnDisable()
        {
            // Unsubscribe to prevent memory leaks.
            // 메모리 누수를 방지하기 위해 구독을 해제합니다.
            var inputManager = UGESystemController.Instance?.InputManager;
            if (inputManager != null)
            {
                inputManager.OnContinueDialogue -= OnContinueClicked;
            }
        }
        
        /// <summary>
        /// Handles the central "continue" input, deciding whether to paginate text or advance the game event.
        /// </summary>
        private void OnContinueClicked()
        {
            // If the dialogue box is not active, do nothing. This prevents clicks from advancing events when the dialogue UI isn't visible.
            // 대화 상자가 활성화되어 있지 않으면 아무것도 하지 않습니다. 이는 대화 UI가 보이지 않을 때 클릭으로 이벤트가 진행되는 것을 방지합니다.
            if (_dialogueBox == null || !_dialogueBox.activeInHierarchy)
            {
                return;
            }

            // If a size adjuster is present, try to let it handle the pagination.
            // 만약 _dialogueSizeAdjuster가 할당되어 있다면, 페이지네이션을 처리하도록 시도합니다.
            if (_dialogueSizeAdjuster != null)
            {
                // TryShowNextPage returns true if it handled the input (i.e., it's busy or successfully paged).
                // TryShowNextPage는 입력을 처리했을 경우 (페이지 전환 중이거나, 성공적으로 페이지를 넘겼을 경우) true를 반환합니다.
                if (_dialogueSizeAdjuster.TryShowNextPage())
                {
                    // The adjuster handled it, so we do nothing more.
                    // Adjuster가 입력을 처리했으므로, 여기서 추가 작업을 하지 않습니다.
                    return;
                }
            }

            // If the adjuster is not present, or if it returned false (meaning it's on the last page), proceed to the next game event.
            // Adjuster가 없거나, false를 반환했다면 (마지막 페이지라는 의미), 다음 게임 이벤트를 진행합니다.
            UGESystemController.Instance.GameEventController.ContinueEvent();
        }

        /// <summary>
        /// Displays the dialogue box with the specified character name and text.
        /// </summary>
        /// <param name="characterName">The name of the speaking character.</param>
        /// <param name="dialogue">The dialogue text to display.</param>
        public void ShowDialogue(string characterName, string dialogue)
        {
            if(_dialogueBox == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("UIManager's 'Dialogue Box' field is empty! Please check the object in the inspector.");
#endif
                return;
            }

            if(_choiceBox != null) _choiceBox.SetActive(false);
            if(_dialogueBox != null) _dialogueBox.SetActive(true);
            if (_continueButton != null) _continueButton.gameObject.SetActive(true);

            if(_characterNameText != null) _characterNameText.text = characterName;
            if(_dialogueText != null) _dialogueText.text = dialogue;
        }

        /// <summary>
        /// Displays the choice box with a list of options.
        /// </summary>
        /// <param name="choices">The list of choice options to display.</param>
        /// <param name="onChoiceSelected">A callback action that returns the index of the selected choice.</param>
        public void ShowChoices(List<ChoiceOption> choices, Action<int> onChoiceSelected)
        {
            if (_continueButton != null) _continueButton.gameObject.SetActive(false);
            if (_dialogueBox != null) _dialogueBox.SetActive(false);
            if (_choiceBox != null) _choiceBox.SetActive(true);

            for (int i = 0; i < _choiceButtons.Count; i++)
            {
                if (i < choices.Count)
                {
                    _choiceButtons[i].gameObject.SetActive(true);
                    
                    var buttonTexts = _choiceButtons[i].GetComponentsInChildren<TextMeshProUGUI>();

                    foreach(var btntext in buttonTexts)
                    {
                        if (btntext != null)
                        {
                            btntext.text = choices[i].Text;
                        }
                    }

                    int choiceIndex = i;
                    _choiceButtons[i].onClick.RemoveAllListeners();
                    _choiceButtons[i].onClick.AddListener(() =>
                    {
                        onChoiceSelected?.Invoke(choiceIndex);
                    });
                }
                else
                {
                    _choiceButtons[i].gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Displays and animates a cinematic-style text message.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="animationDuration">The duration of the reveal animation.</param>
        /// <returns>An IEnumerator for the coroutine.</returns>
        public System.Collections.IEnumerator ShowCinematicText(string text, float animationDuration = 0.5f)
        {
            if (_cinematicTextBox == null || _cinematicTextMesh == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("UIManager's 'Cinematic Text' field is empty! Please check the object in the inspector.");
#endif
                yield break;
            }
            
            if(_dialogueBox != null) _dialogueBox.SetActive(false);
            if(_choiceBox != null) _choiceBox.SetActive(false);
            if (_continueButton != null) _continueButton.gameObject.SetActive(false);

            _cinematicTextBox.SetActive(true);
            _cinematicTextMesh.text = text;

            // 텍스트가 설정된 후, 정확한 높이(preferredHeight)를 계산하기 위해 캔버스를 강제로 업데이트합니다.
            // After the text is set, force update the canvas to calculate the correct height (preferredHeight).
            Canvas.ForceUpdateCanvases();
            yield return null;

            RectTransform textRect = _cinematicTextMesh.rectTransform;
            Vector2 finalPosition = textRect.anchoredPosition;
            
            // 시작 위치를 텍스트의 실제 높이만큼 최종 위치의 아래로 설정합니다.
            // Set the start position below the final position by the actual height of the text.
            // 이렇게 하면 항상 텍스트 영역 바로 바깥에서 애니메이션이 시작됩니다.
            // This way the animation always starts just outside the text area.
            Vector2 startPosition = finalPosition - new Vector2(0, _cinematicTextMesh.preferredHeight + 10); 

            // 애니메이션 시작 전 초기 위치 설정
            // Set initial position before animation starts
            textRect.anchoredPosition = startPosition;
            
            float elapsedTime = 0f;
            while (elapsedTime < animationDuration)
            {
                // 스킵 기능 추가: 스킵 플래그가 활성화되면 애니메이션 루프를 즉시 탈출
                // Add skip feature: Immediately exit animation loop if skip flag is active
                if (UGESystemController.Instance.GameEventController.IsSkipActive)
                {
                    break;
                }

                float t = elapsedTime / animationDuration;
                float easedT = 1 - Mathf.Pow(1 - t, 3); // Cubic ease-out
                
                textRect.anchoredPosition = Vector2.Lerp(startPosition, finalPosition, easedT);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            textRect.anchoredPosition = finalPosition;
        }

        /// <summary>
        /// Hides the cinematic text box.
        /// </summary>
        public void HideCinematicText()
        {
            if (_cinematicTextBox != null)
            {
                _cinematicTextBox.SetActive(false);
            }
        }

        /// <summary>
        /// Hides all major UI elements managed by this controller.
        /// </summary>
        public void HideAllUI()
        {
            if (_dialogueBox != null) _dialogueBox.SetActive(false);
            if (_choiceBox != null) _choiceBox.SetActive(false);
            if (_continueButton != null) _continueButton.gameObject.SetActive(false);
            if (_cinematicTextBox != null) _cinematicTextBox.SetActive(false); // Cinematic Text 숨기기 추가 // Add hiding Cinematic Text
            HideBackground();
        }

        /// <summary>
        /// Displays a static image as the background.
        /// </summary>
        /// <param name="image">The Texture2D to display.</param>
        public void ShowImageBackground(Texture2D image)
        {
            if (_backgroundRawImage == null) return;

            // 비디오 플레이어는 확실히 끈다.
            // Make sure to turn off the video player.
            if (_backgroundVideoPlayer != null)
            {
                _backgroundVideoPlayer.Stop();
                _backgroundVideoPlayer.gameObject.SetActive(false);
            }

            _backgroundRawImage.texture = image;
            _backgroundRawImage.gameObject.SetActive(true);
        }

        /// <summary>
        /// Plays a video clip as the background.
        /// </summary>
        /// <param name="video">The VideoClip to play.</param>
        public void PlayVideoBackground(VideoClip video)
        {
            if (_backgroundRawImage == null || _backgroundVideoPlayer == null || _videoRenderTexture == null) return;

            // RawImage와 VideoPlayer를 모두 활성화한다.
            // Activate both RawImage and VideoPlayer.
            _backgroundRawImage.gameObject.SetActive(true);
            _backgroundVideoPlayer.gameObject.SetActive(true);
            
            _backgroundRawImage.texture = _videoRenderTexture;
            _backgroundVideoPlayer.clip = video;
            _backgroundVideoPlayer.targetTexture = _videoRenderTexture;
            _backgroundVideoPlayer.Play();
        }

        /// <summary>
        /// Hides the currently displayed background.
        /// </summary>
        public void HideBackground()
        {
            // RawImage와 VideoPlayer를 모두 비활성화한다.
            // Deactivate both RawImage and VideoPlayer.
            if (_backgroundRawImage != null)
            {
                _backgroundRawImage.gameObject.SetActive(false);
            }
            if (_backgroundVideoPlayer != null)
            {
                _backgroundVideoPlayer.Stop();
                _backgroundVideoPlayer.gameObject.SetActive(false);
            }
        }
    }
}