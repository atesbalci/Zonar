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
        private int _boostSteps;
        private const int BoostLimit = 2;
        private bool _isBoostActive;
        public Vector3 GoalPosition;

        private void Start()
        {
            DOTween.Init();
            _camOffset = Camera.main.transform.position;
            GameCore.Instance.Player = this;
            GoalPosition = new Vector3(Random.Range(200,250), 0f, Random.Range(200, 250));
            Debug.Log(GoalPosition);
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

                var hits = Physics.SphereCastAll(ray, 3f, 100);
                foreach (var raycastHit in hits.OrderBy(x => (ray.origin - x.transform.position).magnitude).ToList())
                {
                    var cube = raycastHit.transform.GetComponent<ZCube>();
                    if (cube != null)
                    {
                        var selectedCube = cube;
                        if (selectedCube.Type == ZCubeType.Basic)
                        {
                            continue;
                        }
                        if (selectedCube.Type == ZCubeType.Boost) //Set boost count
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
                            _boostSteps = 5; //TODO: change later
                            GameCore.TransmissionDuration = 0.4f;
                        }
                        NormalMove(selectedCube);
                        break;
                    }
                }
            }
        }

        private void NormalMove(ZCube cube)
        {
            if ((cube.Type == ZCubeType.Transmissive || cube.Type == ZCubeType.Boost || cube.Type == ZCubeType.Goal) && (cube.transform.localScale.y > 8f || _isBoostActive)) //NodeSelect Logic here
            {
                transform.position = cube.transform.position;//Move player
                Camera.main.transform.DOMove(transform.position + _camOffset, GameCore.TransmissionDuration);
                GameCore.Instance.State = GameState.Transmitting;
                if (cube.Type == ZCubeType.Goal)
                {
                    Debug.Log("Yeeeey");
                }
            }
        }

        private void BoostMove()
        {
            var controller = FindObjectOfType<CubesController>();
            if (controller != null)
            {
                var transmissives = controller.Cubes.Where(x => x.Type == ZCubeType.Transmissive || x.Type == ZCubeType.Goal).ToList();
                var selectedCubeindex = 0;
                var distance = float.MaxValue;
                for (int i = 0; i < transmissives.Count; i++) //TODO: Distance for goal node
                {
                    var zCube = transmissives[i];
                    var dis = (zCube.transform.position - GoalPosition).magnitude;
                    if (dis < distance)
                    {
                        selectedCubeindex = i;
                        distance = dis;
                    }
                }
                NormalMove(transmissives[selectedCubeindex]);

                if (--_boostSteps == 0)
                {
                    _isBoostActive = false;
                    _consecutiveBoostCount = 0;
                    GameCore.TransmissionDuration = 0.75f;
                }
            }
        }
    }
}
