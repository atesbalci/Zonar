using System.Collections.Generic;
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

        [Space(10)]
        public AnimationCurve BlinkCurve;
        public float BlinkSpeedMultiplier;
        
        private Camera _cam;
        private Dictionary<Image, float> _alphas;

        private void Start()
        {
            _cam = Camera.main;
            gameObject.SetActive(false); //close when gameLoads
            MessageBroker.Default.Receive<GameStateChangeEvent>().Subscribe(ev =>
            {
                if (ev.State == GameState.Menu || ev.State == GameState.GameOver || ev.State == GameState.LevelCompleted)
                {
                    gameObject.SetActive(false);
                }
                else
                {
                    gameObject.SetActive(true);
                }
            });
            _alphas = new Dictionary<Image, float>
            {
                { Top, 0f },
                { Bottom, 0f },
                { Left, 0f },
                { Right, 0f },
            };
        }

        private void Reset()
        {
            BlinkSpeedMultiplier = 1f;
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


            _alphas[Top] = ratios.y;
            _alphas[Bottom] = -ratios.y;
            _alphas[Left] = -ratios.x;
            _alphas[Right] = ratios.x;

            foreach (var kvp in _alphas)
            {
                var col = kvp.Key.color;
                col.a = kvp.Value * BlinkCurve.Evaluate(Time.time * BlinkSpeedMultiplier);
                kvp.Key.color = col;
            }
        }
    }
}
