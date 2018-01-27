using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class CompassView : MonoBehaviour
    {
        public Image Top;
        public Image Bottom;
        public Image Left;
        public Image Right;
        
        private Camera _cam;

        private void Start()
        {
            _cam = Camera.main;
            MessageBroker.Default.Receive<GameStateChangeEvent>().Subscribe(ev =>
            {
                gameObject.SetActive(ev.State != GameState.Menu);
            });
        }

        private void Update()
        {
            var scrPos = _cam.WorldToScreenPoint(GameCore.Instance.Player.GoalPosition);
            var offSetFromScr = Vector2.zero;
            if (scrPos.x < 0f)
                offSetFromScr.x = scrPos.x;
            else if (scrPos.x > Screen.width)
                offSetFromScr.x = scrPos.x - Screen.width;
            if (scrPos.y < 0f)
                offSetFromScr.y = scrPos.y;
            else if (scrPos.y > Screen.height)
                offSetFromScr.y = scrPos.y - Screen.height;
            var ratios = Vector2.zero;
            ratios.x = offSetFromScr.x / (Mathf.Abs(offSetFromScr.x) + Mathf.Abs(offSetFromScr.y));
            ratios.y = offSetFromScr.y / (Mathf.Abs(offSetFromScr.x) + Mathf.Abs(offSetFromScr.y));

            SetImageAlpha(Top, ratios.y);
            SetImageAlpha(Bottom, -ratios.y);
            SetImageAlpha(Left, -ratios.x);
            SetImageAlpha(Right, ratios.x);
        }

        private static void SetImageAlpha(Image image, float alpha)
        {
            var col = image.color;
            col.a = alpha;
            image.color = col;
        }
    }
}
