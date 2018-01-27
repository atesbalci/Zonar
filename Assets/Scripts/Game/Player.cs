using DG.Tweening;
using UniRx;
using UnityEngine;

namespace Game
{
    class Player : MonoBehaviour
    {
        private Vector3 _camOffset;

        private void Start()
        {
            DOTween.Init();
            _camOffset = Camera.main.transform.position;
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 100))
                {
                    var cube = hit.transform.GetComponent<ZCube>();
                    if (cube != null && cube.Type == ZCubeType.Node && cube.transform.localScale.y>8f) //NodeSelect Logic here
                    {
                        transform.position = cube.transform.position;//Move player
                        Camera.main.transform.DOMove(transform.position + _camOffset, GameCore.TransmissionDuration).SetEase(Ease.InOutSine);
                        GameCore.Instance.State = GameState.Transmitting;
                        MessageBroker.Default.Publish(new TransmissionStartedEvent()
                        {
                            Position = transform.position
                        });
                    }
                }
            }
        }


    }
}
