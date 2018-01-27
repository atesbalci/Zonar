using System.Linq;
using DG.Tweening;
using UniRx;
using UnityEngine;

namespace Game
{
    public class Player : MonoBehaviour
    {
        private Vector3 _camOffset;
        private int _consecutiveBoostCount;
        private const int BoostLimit = 2;
        private bool _isBoostActive;
        private Vector3 _goalPosition;

        private void Start()
        {
            DOTween.Init();
            _camOffset = Camera.main.transform.position;
            GameCore.Instance.Player = this;
            _goalPosition = new Vector3(Random.Range(200,250), 10f, Random.Range(200, 250));
            var goalCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            goalCube.transform.position = _goalPosition;
            goalCube.GetComponent<Renderer>().material.color = Color.red;
        }

        void Update()
        {
            if (_isBoostActive && GameCore.Instance.State == GameState.AwaitingTransmission)
            {
                BoostMove();
            }
            else if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 100))
                {
                    var cube = hit.transform.GetComponent<ZCube>();
                    if (cube.Type == ZCubeType.Boost) //Set boost count
                    {
                        _consecutiveBoostCount++;
                        Debug.Log("BoostCount " + _consecutiveBoostCount);
                    }
                    else
                    {
                        _consecutiveBoostCount = 0;
                        Debug.Log("Boost Cleared");
                    }

                    if (_consecutiveBoostCount == BoostLimit)
                    {
                        _isBoostActive = true;
                        GameCore.TransmissionDuration = 0.5f;
                    }
                    NormalMove(cube);
                }
            }
        }

        private void NormalMove(ZCube cube)
        {
            if ((cube.Type == ZCubeType.Transmissive || cube.Type == ZCubeType.Boost) && (cube.transform.localScale.y > 8f || _isBoostActive)) //NodeSelect Logic here
            {
                transform.position = cube.transform.position;//Move player
                Camera.main.transform.DOMove(transform.position + _camOffset, GameCore.TransmissionDuration);
                GameCore.Instance.State = GameState.Transmitting;
            }
        }

        private void BoostMove()
        {
            var controller = FindObjectOfType<CubesController>();
            if (controller != null)
            {
                var transmissives = controller.Cubes.Where(x => x.Type == ZCubeType.Transmissive).ToList();
                var selectedCubeindex = 0;
                var distance = float.MaxValue;
                for (int i = 0; i < transmissives.Count; i++) //TODO: Distance for goal node
                {
                    var zCube = transmissives[i];
                    var dis = (zCube.transform.position - _goalPosition).magnitude;
                    if (dis < distance)
                    {
                        selectedCubeindex = i;
                        distance = dis;
                    }
                }
                NormalMove(transmissives[selectedCubeindex]);

                if (--_consecutiveBoostCount == 0)
                {
                    _isBoostActive = false;
                    GameCore.TransmissionDuration = 1f;
                }
            }
        }
    }
}
