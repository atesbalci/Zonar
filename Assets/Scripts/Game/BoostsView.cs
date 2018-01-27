using DG.Tweening;
using UnityEngine;

namespace Game
{
    public class BoostsView : MonoBehaviour
    {
        public Renderer[] Renderers;

        private void Start()
        {
            foreach (var rend in Renderers)
            {
                rend.transform.DOLocalRotate(new Vector3(0, 360, 0), 5f).SetRelative().SetEase(Ease.Linear).SetLoops(-1);
            }
        }

        private void Update()
        {
            for (var i = 0; i < Renderers.Length; i++)
            {
                var rend = Renderers[i];
                rend.material.color = Color.Lerp(rend.material.color, GameCore.Instance.Player.ConsecutiveBoostCount > i ? Color.magenta : Color.Lerp(Color.black, Color.magenta, 0.2f), Time.deltaTime * 10f);
            }
        }
    }
}
