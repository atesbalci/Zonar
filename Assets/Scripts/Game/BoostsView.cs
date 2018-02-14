using DG.Tweening;
using UniRx;
using UnityEngine;

namespace Game
{
    public class BoostsView : MonoBehaviour
    {
        public Renderer[] Renderers;

        private void Awake()
        {
            foreach (var rend in Renderers)
            {
                rend.transform.DOLocalRotate(new Vector3(0, 360, 0), 5f).SetRelative().SetEase(Ease.Linear).SetLoops(-1);
            }

            MessageBroker.Default.Receive<GameStateChangeEvent>().Subscribe(ev =>
            {
                if (ev.State == GameState.Menu || ev.State == GameState.LevelCompleted)
                {
                    foreach (var renderer1 in Renderers)
                    {
                        renderer1.enabled = false;
                    }
                }
                else
                {
                    foreach (var renderer1 in Renderers)
                    {
                        renderer1.enabled = true;
                    }
                }
            });
        }

        private void Update()
        {
            var boosts = GameCore.Instance.Player.Boosts;
            for (var i = 0; i < Renderers.Length; i++)
            {
                var col = i < boosts.Count ?
                    ZCube.GetCubeColor((ZCubeType)boosts[i]) : new Color(0.33f, 0.33f, 0.33f);
                var rend = Renderers[i];
                rend.material.color = Color.Lerp(rend.material.color, col, Time.deltaTime * 10f);
            }
        }
    }
}
