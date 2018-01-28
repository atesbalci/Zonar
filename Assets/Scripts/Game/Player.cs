﻿using System.Linq;
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
        public bool IsBoostActive;
        public Vector3 GoalPosition;
        public ZCubeType CurrentCubeType = ZCubeType.Transmissive;
        public ZCubeType NextCubeType;

        public int Level = 1;
        public int Score;

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
            GoalPosition = new Vector3(Random.Range(100*Level, 150 * Level) * CubesController.Gap, 0f, Random.Range(-150 * Level, 150 * Level) * CubesController.Gap);
        }

        void Update()
        {
            if (IsBoostActive && GameCore.Instance.State == GameState.AwaitingTransmission)
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
                            IsBoostActive = true;
                            _boostSteps = 3 + Level * 2; //TODO: change later
                            GameCore.TransmissionDuration = Mathf.Clamp(0.45f - Level*0.05f, 0.2f, 1f);
                        }
                        NormalMove(selectedCube);
                        break;
                    }
                }
            }
        }

        private void NormalMove(ZCube cube)
        {
            if ((cube.Type == ZCubeType.Transmissive || cube.Type == ZCubeType.Boost || cube.Type == ZCubeType.Goal) && (cube.transform.localScale.y > 8f || IsBoostActive)) //NodeSelect Logic here
            {
                Score += Mathf.CeilToInt((transform.position - cube.transform.position).magnitude * Level);
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
                    IsBoostActive = false;
                    ConsecutiveBoostCount = 0;
                    GameCore.TransmissionDuration = 0.75f;
                }
            }
        }

    }
}
