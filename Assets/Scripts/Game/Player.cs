using System.Linq;
using DG.Tweening;
using JetBrains.Annotations;
using UniRx;
using UnityEngine;

namespace Game
{
    public class Player : MonoBehaviour
    {
        public Vector3 CamOffset;
        public int ConsecutiveBoostCount { get; set; }
        private int _boostSteps;
        private const int BoostLimit = 3;
        private bool _isBoostActive;
        public Vector3 GoalPosition;
        public ZCubeType CurrentCubeType = ZCubeType.Transmissive;
        public ZCubeType NextCubeType;

        public int Level;

        private void Start()
        {
            DOTween.Init();
            CamOffset = Camera.main.transform.position;
            GameCore.Instance.Player = this;
            CalculateGoalPosition();
            Debug.Log(GoalPosition);

            MessageBroker.Default.Receive<GameStateChangeEvent>().Subscribe(ev =>
            {
                if (ev.State == GameState.AwaitingTransmission)
                {
                    CurrentCubeType = NextCubeType;
                    if (CurrentCubeType == ZCubeType.Goal)
                    {
                        GameCore.Instance.State = GameState.LevelCompleted;
                    }
                }
            });
        }

        public void CalculateGoalPosition()
        {
            GoalPosition = new Vector3(Random.Range(100, 150) * CubesController.Gap, 0f, Random.Range(-150, 150) * CubesController.Gap);
        }

        void Update()
        {
            if (_isBoostActive && GameCore.Instance.State == GameState.AwaitingTransmission)
            {
                BoostMove();
            }
            else if (Input.GetMouseButtonDown(0) && GameCore.Instance.State != GameState.GameOver && GameCore.Instance.State == GameState.AwaitingTransmission)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                var hits = Physics.SphereCastAll(ray, 2f, 1000);
                var selectedHits =
                    hits.Where(y => y.transform.localScale.y > 5f)
                        .OrderBy(x => (ray.origin - x.transform.position).magnitude)
                        .ToList();
                var boost = selectedHits.Where(x =>
                {
                    var c = x.transform.GetComponent<ZCube>();
                    if (c != null && c.Type == ZCubeType.Boost)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                });

                if (boost.Any())
                {
                    selectedHits = boost.ToList();
                }

                foreach (var raycastHit in selectedHits)
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
                            ConsecutiveBoostCount++;
                            Debug.Log("BoostCount " + ConsecutiveBoostCount);
                        }
                        else if(selectedCube.Type == ZCubeType.Transmissive)
                        {
                            ConsecutiveBoostCount = 0;
                            Debug.Log("Boost Cleared");
                        }

                        if (ConsecutiveBoostCount == BoostLimit)
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
                Camera.main.transform.DOMove(transform.position + CamOffset, GameCore.TransmissionDuration);
                GameCore.Instance.State = GameState.Transmitting;
                NextCubeType = cube.Type;
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
                    ConsecutiveBoostCount = 0;
                    GameCore.TransmissionDuration = 0.75f;
                }
            }
        }

    }
}
