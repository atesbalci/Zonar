using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UniRx;
using UnityEngine;

namespace Game
{
    public class CubesController : MonoBehaviour
    {
        public const float Gap = 1.1f;
        public const int WaveRadius = 1011;
        public const float TimerLimit = 3.5f;
        private const float RingWidth = 1f;
        private const float Speed = 2.5f;
        private const float ScreenBoundTolerance = 100f;
        private const float MaxRange = 9.5f; //TODO: Perhaps actually calculate this?

        public float Timer;
        public List<ZCube> Cubes;
        public GameObject CubePrefab;

        private Tweener[] _tweeners;
        private float _curVisibility;
        private GameObject _cubeParent;
        private int _amountOfCubesWithinRange;

        private void Start()
        {
            Shader.SetGlobalFloat("_MIN", 1f);
            Shader.SetGlobalFloat("_MAX", ZCube.MaxHeight);
            Shader.SetGlobalFloat("_MULTIPLIER", 1f);
            Application.targetFrameRate = 60;
            Tile();
            Vector3 pos = Vector3.zero;
            MessageBroker.Default.Receive<GameStateChangeEvent>().Subscribe(ev =>
            {
                if (ev.State == GameState.AwaitingTransmission)
                {
                    SetUserCube(GameCore.Instance.Player.transform.position);
                    if (_tweeners != null)
                    {
                        foreach (var tweener in _tweeners)
                        {
                            tweener.Kill(true);
                        }
                        Update();
                    }
                }
                else if (ev.State == GameState.Transmitting)
                {
                    pos = GameCore.Instance.Player.transform.position;
                    var playerDistance = _cubeParent.transform.InverseTransformPoint(pos).magnitude * 2f;
                    _tweeners = new Tweener[Cubes.Count];
                    for (var i = 0; i < Cubes.Count; i++)
                    {
                        var cube = Cubes[i];
                        if (cube.Type == ZCubeType.Goal || cube.Type == ZCubeType.Player)
                        {
                            continue;
                        }
                        //var y = cube.transform.localScale.y;
                        var dist = Vector3.Distance(cube.transform.position, pos);
                        _tweeners[i] = cube.transform.DOScaleY(dist < 1.5f ? ZCube.MaxHeight : 1f,
                            Mathf.Min(GameCore.TransmissionDuration - 0.1f,
                                GameCore.TransmissionDuration * (1f - dist / playerDistance))).SetEase(Ease.InOutSine);
                    }
                }
                else if(ev.State == GameState.GameOver)
                {
                    _tweeners = new Tweener[Cubes.Count];
                    for (var i = 0; i < Cubes.Count; i++)
                    {
                        var cube = Cubes[i];
                        //if (cube.Type == ZCubeType.Player)
                        //{
                        //    continue;
                        //}
                        var dist = Vector3.Distance(cube.transform.position, pos);
                        _tweeners[i] = cube.transform.DOScaleY(1f, GameCore.TransmissionDuration * dist/5f).SetEase(Ease.InOutSine);
                    }
                }
                else if (ev.State == GameState.LevelCompleted)
                {
                    Update();
                    _tweeners = new Tweener[Cubes.Count];
                    for (var i = 0; i < Cubes.Count; i++)
                    {
                        var cube = Cubes[i];
                        var dist = Vector3.Distance(cube.transform.position, pos);
                        //if (cube.Type == ZCubeType.Player || cube.Type == ZCubeType.Player)
                        //{
                        //    continue;
                        //}
                        _tweeners[i] = cube.transform.DOScaleY(1f, GameCore.TransmissionDuration * dist).SetEase(Ease.InOutSine);
                    }
                }
            });
        }

        public void Tile()
        {
            Cubes = new List<ZCube>();
            _cubeParent = new GameObject("Cubes");
            for (int x = 0; x < WaveRadius; x++)
            {
                for (int y = 0; y < WaveRadius; y++)
                {
                    var pos = new Vector3(x - WaveRadius / 2 - 1, 0f, y - WaveRadius / 2 - 1) * Gap;
                    var scrPos = Camera.main.WorldToScreenPoint(pos);
                    if ((pos.magnitude<GameCore.MaxBoostLeapDistance + 1f) || //max boost distance required for small screen devices.
                        scrPos.x >= -ScreenBoundTolerance && 
                        scrPos.y >= -ScreenBoundTolerance &&
                        scrPos.x < Screen.width + ScreenBoundTolerance &&
                        scrPos.y < Screen.height + ScreenBoundTolerance)
                    {
                        var cube = Instantiate(CubePrefab, pos, Quaternion.identity).GetComponent<ZCube>();
                        cube.transform.SetParent(_cubeParent.transform);
                        Cubes.Add(cube);
                        cube.Init();
                    }
                }
            }
            Cubes.First(x => Mathf.Approximately(Vector3.Distance(x.transform.localPosition, Vector3.zero), 0f)).Type = ZCubeType.Player;
            _amountOfCubesWithinRange = Cubes.Count(cube => cube.transform.localPosition.magnitude < MaxRange);
            SetUserCube(Vector3.zero);
        }

        private void Update()
        {
            var state = GameCore.Instance.State;
            var newVisibility = Mathf.Clamp01(_curVisibility + (state == GameState.Transmitting ? -1f : 1f) * Time.deltaTime * 10f);
            if (!Mathf.Approximately(newVisibility, _curVisibility))
            {
                _curVisibility = newVisibility;
                Shader.SetGlobalFloat("_MULTIPLIER", _curVisibility);
            }

            if (state == GameState.AwaitingTransmission)
            {
                if (Timer < TimerLimit)
                {
                    Timer += Time.deltaTime;
                }
                else if (state != GameState.LevelCompleted)
                {
                    GameCore.Instance.State = GameState.GameOver;
                }
            }
            else if (state != GameState.LevelCompleted)
            {
                return;
            }
            
            // ReSharper disable once TooWideLocalVariableScope
            float height;
            var level = GameCore.Instance.Player.Level;
            var globalTime = Time.time;
            foreach (var cube in Cubes)
            {
                height = 1f - Mathf.PerlinNoise(cube.LocalPos.x * 0.5f + globalTime, cube.LocalPos.z * 0.5f + globalTime);
                if (cube.Type == ZCubeType.Player)
                {
                    height = ZCube.MaxHeight * (1f - Timer / TimerLimit);
                }
                else if (cube.Type == ZCubeType.Goal)
                {
                    height = ZCube.MaxHeight;
                }
                else
                {
                    if (state == GameState.AwaitingTransmission)
                    {
                        height = CalculateHeight(cube.RadialDistance, Timer, Speed + level * 0.25f,
                            RingWidth, height, ZCube.MaxHeight);
                    }
                }
                cube.transform.localScale = new Vector3(1f, height, 1f);
            }
        }

        private static float CalculateHeight(float distance, float time, float speed, float width, float min, float max)
        {
            return Mathf.Clamp(max - Mathf.Abs(Mathf.Max(0f, Mathf.Abs(distance - time * speed) - width) * (max - min)), min, max);
        }

        public void SetUserCube(Vector3 pos)
        {
            _cubeParent.transform.position = pos;
            Timer = 0f;

            //Begin type placement process
            var typeMap = new ZCubeType[_amountOfCubesWithinRange];
            var transmissiveTypeAmt = (ZCubeType.Transmissive3 - ZCubeType.Transmissive1) + 1;
            for (var i = 0; i < transmissiveTypeAmt; i++)
            {
                var amt = Random.Range(1, Mathf.Max(1,
                    Mathf.RoundToInt(((float)_amountOfCubesWithinRange / transmissiveTypeAmt) * 0.05f)));
                for (var j = 0; j < amt; j++)
                {
                    int rand;
                    do
                    {
                        rand = Random.Range(0, _amountOfCubesWithinRange);
                    } while (typeMap[rand] != ZCubeType.Basic);
                    typeMap[rand] = ZCubeType.Transmissive1 + i;
                }
            }

            //Set the types
            var inRangeIndex = 0;
            const float rangeSq = MaxRange * MaxRange; //Haram
            foreach (var zCube in Cubes) //set other cubes
            {
                if (zCube.Type == ZCubeType.Player)
                {
                    continue;
                }
                if ((GameCore.Instance.Player.GoalPosition - transform.position).magnitude < 0.5f)
                {
                    zCube.Type = ZCubeType.Goal;
                    continue;
                }
                if (zCube.transform.localPosition.sqrMagnitude < rangeSq)
                {
                    zCube.Type = typeMap[inRangeIndex];
                    inRangeIndex++;
                }
                else
                {
                    zCube.Type = ZCubeType.Basic;
                }
            }
        }
    }
}
