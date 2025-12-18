using TMPro;
using UnityEngine;
using System.Collections;

namespace UGESystem
{
    /// <summary>
    /// Adjusts the size of a container to fit its TextMeshPro text content.
    /// If the text exceeds the max height, it enables pagination, allowing text to be displayed across multiple pages.
    /// This component should be placed on the root Dialogue Panel.
    /// </summary>
    public class TMP_ContentSizeAdjuster : MonoBehaviour
    {
        [Header("Targets")]
        [Tooltip("The root container panel whose size will be adjusted.")]
        [SerializeField] private RectTransform _containerToAdjust;
        [Tooltip("The TextMeshProUGUI component that contains the dialogue text.")]
        [SerializeField] private TextMeshProUGUI _textComponent;
        [Tooltip("(Optional) A panel on the left (like a character name panel) whose width will be included in the total calculation.")]
        [SerializeField] private RectTransform _leftPanelToConsider;
        [Tooltip("(Optional) A panel on the right (like a button panel) whose width will be included in the total calculation.")]
        [SerializeField] private RectTransform _rightPanelToConsider;

        [Header("Sizing Options")]
        [Tooltip("Padding added to the left and right of the calculated text width.")]
        [SerializeField] private float _horizontalPadding = 40f;
        [Tooltip("Padding added to the top and bottom of the calculated text height.")]
        [SerializeField] private float _verticalPadding = 40f;
        
        [Tooltip("The minimum width the container can shrink to.")]
        [SerializeField] private float _minWidth = 200f;
        [Tooltip("The maximum width the container can expand to before text wraps.")]
        [SerializeField] private float _maxWidth = 800f;
        [Tooltip("The maximum height the container can expand to. Text will be paginated if it exceeds this. Set to 0 or less to disable pagination and dynamically resize height.")]
        [SerializeField] private float _maxHeight = 0f;

        [Header("Animation")]
        [Tooltip("Duration of the resize animation. Set to 0 for instant resizing.")]
        [SerializeField] private float _resizeDuration = 0.1f;
        [Tooltip("Duration of the fade animation when changing pages. Set to 0 to disable.")]
        [SerializeField] private float _pageTransitionDuration = 0.1f;

        public bool IsOnLastPage => _currentPage >= _totalPages;

        private int _totalPages = 1;
        private int _currentPage = 1;

        private Coroutine _resizeCoroutine;
        private Coroutine _processCoroutine;
        private Coroutine _pageTransitionCoroutine;
        private bool _isProcessing = false;
        private bool _isAnimatingPage = false;

        private void Awake()
        {
            if (_containerToAdjust == null) _containerToAdjust = GetComponent<RectTransform>();
            if (_textComponent == null)
            {
                Debug.LogError("Text Component is not assigned.", this);
                return;
            }
            _textComponent.overflowMode = TextOverflowModes.Page;
        }

        private void OnEnable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextMeshProTextChanged);
            ProcessTextUpdate();
        }

        private void OnDisable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextMeshProTextChanged);
            // Stop all coroutines and reset flags to prevent them from getting stuck in a bad state.
            StopAllCoroutines();
            _isProcessing = false;
            _isAnimatingPage = false;
            _pageTransitionCoroutine = null;
            _processCoroutine = null;
            _resizeCoroutine = null;
        }

        private void OnTextMeshProTextChanged(Object obj)
        {
            if (obj != _textComponent) return;

            if (_isAnimatingPage)
            {
                return;
            }
            
            ProcessTextUpdate();
        }

        private void ProcessTextUpdate()
        {
            if (_isProcessing) return;
            
            if (gameObject.activeInHierarchy)
            {
                if(_processCoroutine != null) StopCoroutine(_processCoroutine);
                _processCoroutine = StartCoroutine(ProcessTextUpdateCoroutine());
            }
        }
        
        private IEnumerator ProcessTextUpdateCoroutine()
        {
            _isProcessing = true;
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextMeshProTextChanged);

            _currentPage = 1;
            UpdatePaginationInfo();

            yield return new WaitForEndOfFrame();
            AdjustSize();

            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextMeshProTextChanged);
            _isProcessing = false;
        }

        private void UpdatePaginationInfo()
        {
            if (_textComponent == null) return;
            _textComponent.ForceMeshUpdate();
            _totalPages = _textComponent.textInfo.pageCount;
        }

        public bool TryShowNextPage()
        {
            if (_isAnimatingPage || _isProcessing)
            {
                return true;
            }

            if (IsOnLastPage)
            {
                return false;
            }

            _currentPage++;

            if (_pageTransitionDuration > 0 && gameObject.activeInHierarchy)
            {
                _pageTransitionCoroutine = StartCoroutine(AnimatePageTransition());
            }
            else
            {
                _textComponent.pageToDisplay = _currentPage;
            }
            return true;
        }

        private void AdjustSize()
        {
            if (_containerToAdjust == null || _textComponent == null) return;

            string text = _textComponent.text;
            if (string.IsNullOrEmpty(text)) text = " ";

            if (_maxWidth <= 0)
            {
                Debug.LogError("[SizeAdjuster] MaxWidth must be greater than 0.", this);
                return;
            }

            float leftPanelWidth = _leftPanelToConsider != null && _leftPanelToConsider.gameObject.activeSelf ? _leftPanelToConsider.rect.width : 0f;
            float rightPanelWidth = _rightPanelToConsider != null && _rightPanelToConsider.gameObject.activeSelf ? _rightPanelToConsider.rect.width : 0f;
            float additionalWidth = leftPanelWidth + rightPanelWidth;
            float widthConstraint = _maxWidth - _horizontalPadding - additionalWidth;
            
            _textComponent.ForceMeshUpdate();
            
            Vector2 singleLinePreferredSize = _textComponent.GetPreferredValues(text, float.PositiveInfinity, float.PositiveInfinity);
            float targetWidth = (singleLinePreferredSize.x > widthConstraint)
                ? _maxWidth
                : singleLinePreferredSize.x + _horizontalPadding + additionalWidth;
            targetWidth = Mathf.Max(targetWidth, _minWidth);

            _textComponent.pageToDisplay = _currentPage;

            float finalContainerHeight;
            if (_maxHeight > 0)
            {
                if (_totalPages > 1 || _textComponent.preferredHeight > _maxHeight)
                {
                    finalContainerHeight = _maxHeight;
                }
                else
                {
                    finalContainerHeight = _textComponent.preferredHeight + _verticalPadding;
                }
            }
            else
            {
                finalContainerHeight = _textComponent.GetPreferredValues(text, widthConstraint, float.PositiveInfinity).y + _verticalPadding;
            }

            Vector2 targetContainerSize = new Vector2(targetWidth, finalContainerHeight);
            
            if (_resizeDuration > 0 && gameObject.activeInHierarchy)
            {
                if (_containerToAdjust.sizeDelta != targetContainerSize)
                {
                    if (_resizeCoroutine != null) StopCoroutine(_resizeCoroutine);
                    _resizeCoroutine = StartCoroutine(AnimateContainerSize(targetContainerSize));
                }
            }
            else
            {
                _containerToAdjust.sizeDelta = targetContainerSize;
            }
        }
        
        private IEnumerator AnimateContainerSize(Vector2 targetSize)
        {
            Vector2 startSize = _containerToAdjust.sizeDelta;
            float elapsedTime = 0f;

            while (elapsedTime < _resizeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / _resizeDuration);
                _containerToAdjust.sizeDelta = Vector2.Lerp(startSize, targetSize, t);
                yield return null;
            }
            _containerToAdjust.sizeDelta = targetSize;
        }

        private IEnumerator AnimatePageTransition()
        {
            _isAnimatingPage = true;

            Color originalColor = _textComponent.color;
            
            // Fade out
            float elapsedTime = 0f;
            while (elapsedTime < _pageTransitionDuration)
            {
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / _pageTransitionDuration);
                _textComponent.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            _textComponent.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
            
            _textComponent.pageToDisplay = _currentPage;
            
            // Wait a frame for the new page geometry to be calculated
            yield return null;
            
            // Fade in
            elapsedTime = 0f;
            while (elapsedTime < _pageTransitionDuration)
            {
                float alpha = Mathf.Lerp(0f, 1f, elapsedTime / _pageTransitionDuration);
                _textComponent.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            _textComponent.color = originalColor;

            // Wait one more frame before resetting the flag to ensure all UI events from the animation have settled.
            yield return null;

            _pageTransitionCoroutine = null;
            _isAnimatingPage = false;
        }
    }
}