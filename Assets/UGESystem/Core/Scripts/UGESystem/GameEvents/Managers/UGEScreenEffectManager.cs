using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UGESystem
{
    /// <summary>
    /// Manager responsible for creating a persistent and high-priority UI canvas to render full-screen visual effects
    /// such as fades, flashes, and tints.
    /// <br/>
    /// 페이드, 플래시, 틴트와 같은 전체 화면 효과를 렌더링하기 위해 영구적이고 우선순위가 높은 UI 캔버스를 생성하는 관리자입니다.
    /// </summary>
    public class UGEScreenEffectManager : MonoBehaviour
    {
        private Image _overlayImage;

        /// <summary>
        /// Gets the current color of the overlay image.
        /// 오버레이 이미지의 현재 색상을 가져옵니다.
        /// </summary>
        public Color CurrentImageColor => _overlayImage != null ? _overlayImage.color : Color.clear;

        private void Awake()
        {
            SetupOverlayImage();
        }

        private void SetupOverlayImage()
        {
            // Canvas 생성
            GameObject canvasGO = new GameObject("UGEScreenEffectCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999; // 다른 모든 UI 위에 있도록 높은 값 설정

            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            // Image 생성
            GameObject imageGO = new GameObject("OverlayImage");
            imageGO.transform.SetParent(canvasGO.transform);
            
            _overlayImage = imageGO.AddComponent<Image>();
            _overlayImage.color = new Color(0, 0, 0, 0); // 기본적으로 투명
            _overlayImage.raycastTarget = false;

            // 화면을 꽉 채우도록 RectTransform 설정
            RectTransform rectTransform = imageGO.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            // 씬 전환 시 파괴되지 않도록 설정
            DontDestroyOnLoad(canvasGO);
        }

        /// <summary>
        /// Fades the screen in from a specified color to transparent over a duration.
        /// 특정 색상에서 투명으로 화면을 페이드 인 시킵니다.
        /// </summary>
        /// <param name="fromColor">The starting color of the fade (alpha determines initial opacity).</param>
        /// <param name="duration">The duration of the fade in seconds.</param>
        public IEnumerator FadeIn(Color fromColor, float duration)
        {
            if (_overlayImage == null) yield break;

            // Instantly set the start color
            _overlayImage.color = fromColor;

            // Define the target color (transparent version of fromColor)
            Color targetColor = new Color(fromColor.r, fromColor.g, fromColor.b, 0);
            
            // Use the existing Fade logic to transition
            yield return Fade(targetColor, duration);
        }

        /// <summary>
        /// Fades the screen to a target color over a duration.
        /// 화면을 지정된 목표 색상으로 지정된 시간 동안 페이드합니다.
        /// </summary>
        /// <param name="targetColor">The target color to fade to.</param>
        /// <param name="duration">The duration of the fade in seconds.</param>
        public IEnumerator Fade(Color targetColor, float duration)
        {
            if (_overlayImage == null) yield break;

            Color startColor = _overlayImage.color;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                _overlayImage.color = Color.Lerp(startColor, targetColor, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            _overlayImage.color = targetColor;
        }

        /// <summary>
        /// Flashes the screen with a specified color, holding it for a duration, then fading out.
        /// 지정된 색상으로 화면을 플래시하고, 일정 시간 유지한 다음 페이드 아웃합니다.
        /// </summary>
        /// <param name="flashColor">The color to flash with.</param>
        /// <param name="fadeInDuration">The duration of the fade-in to the flash color.</param>
        /// <param name="holdDuration">The duration to hold the flash color.</param>
        /// <param name="fadeOutDuration">The duration of the fade-out from the flash color.</param>
        public IEnumerator Flash(Color flashColor, float fadeInDuration, float holdDuration, float fadeOutDuration)
        {
            if (_overlayImage == null) yield break;

            // Fade In
            yield return Fade(flashColor, fadeInDuration);

            // Hold
            if (holdDuration > 0)
            {
                yield return new WaitForSeconds(holdDuration);
            }

            // Fade Out
            yield return Fade(new Color(flashColor.r, flashColor.g, flashColor.b, 0), fadeOutDuration);
        }

        /// <summary>
        /// Tints the screen with a specified color, holds it, and then fades back to the original color.
        /// 지정된 색상으로 화면을 틴트하고, 유지한 다음 원래 색상으로 다시 페이드합니다.
        /// </summary>
        /// <param name="tintColor">The color to tint the screen with.</param>
        /// <param name="holdDuration">The duration to hold the tint color.</param>
        public IEnumerator Tint(Color tintColor, float holdDuration)
        {
            if (_overlayImage == null) yield break;

            Color originalColor = _overlayImage.color;
            float fadeDuration = 0.5f; // Hardcoded fade in/out time

            // 1. Fade TO the tint color
            yield return Fade(tintColor, fadeDuration);

            // 2. HOLD for the specified duration
            if (holdDuration > 0)
            {
                yield return new WaitForSeconds(holdDuration);
            }

            // 3. Fade BACK to the original color
            yield return Fade(originalColor, fadeDuration);
        }

        /// <summary>
        /// Clears any active screen effect by setting the overlay image to transparent.
        /// 오버레이 이미지를 투명하게 설정하여 활성 화면 효과를 지웁니다.
        /// </summary>
        public void ClearEffect()
        {
            if (_overlayImage == null) return;
            _overlayImage.color = new Color(0, 0, 0, 0);
        }
    }
}
