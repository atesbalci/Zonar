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
        public const float TimerLimit = 4f;

        private const float RingWidth = 1f;
        private const float Speed = 2.5f;
        private const float ScreenBoundTolerance = 100f;
        private const float MaxRange = 9.5f; //TODO: Perhaps actually calculate this?
        private const float MinRange = 2.25f;

        public Color MainColor;
        public Color IdleColor;
        public Color WarningBlinkColor;
        [Space(10)]
        public GameObject CubePrefab;

        public float Timer { get; set; }
        public List<ZCube> Cubes { get; private set; }

        private Tweener[] _tweeners;
        private float _curVisibility;
        private GameObject _cubeParent;
        private int _amountOfCubesWithinRange;
        private Color _curMainColor;

        private void Start()
        {
            _curMainColor = MainColor;
            Shader.SetGlobalFloat("_MIN", 1f);
            Shader.SetGlobalFloat("_MAX", ZCube.MaxHeight);
            Shader.SetGlobalFloat("_MULTIPLIER", 1f);
            Shader.SetGlobalColor("_MAIN", _curMainColor);
            Shader.SetGlobalColor("_IDLE", IdleColor);
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
            _amountOfCubesWithinRange = Cubes.Count(cube =>
                cube.transform.localPosition.magnitude > MinRange
                && cube.transform.localPosition.magnitude < MaxRange);
            SetUserCube(Vector3.zero);
        }

        private void Update()
        {
            var level = GameCore.Instance.Player.Level;
            var globalTime = Time.time;
            var curSpeed = (Speed + level * 0.25f) * GameCore.Instance.GetSonarSpeedMultiplier();
            var state = GameCore.Instance.State;
            var newVisibility = Mathf.Clamp01(_curVisibility + (state == GameState.Transmitting ? -1f : 1f) * Time.deltaTime * 10f);
            if (!Mathf.Approximately(newVisibility, _curVisibility))
            {
                _curVisibility = newVisibility;
                Shader.SetGlobalFloat("_MULTIPLIER", _curVisibility);
            }

            if (state == GameState.AwaitingTransmission)
            {
                if (Timer < TimerLimit * (Speed / curSpeed))
                {
                    Timer += Time.deltaTime;
                }
                else if (state != GameState.LevelCompleted)
                {
                    GameCore.Instance.State = GameState.GameOver;
                }
                GameCore.Instance.RemainingTime -= Time.deltaTime;
                if (GameCore.Instance.RemainingTime <= 0f)
                    GameCore.Instance.State = GameState.GameOver;
            }
            else if (state != GameState.LevelCompleted && state != GameState.Menu)
            {
                return;
            }
            
            // ReSharper disable once TooWideLocalVariableScope
            float height;
            foreach (var cube in Cubes)
            {
                height = 1f - Mathf.PerlinNoise(cube.LocalPos.x * 0.5f + globalTime, cube.LocalPos.z * 0.5f + globalTime);
                if (cube.Type == ZCubeType.Player && state != GameState.Menu)
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
                        height = CalculateHeight(cube.RadialDistance, Timer, curSpeed,
                            RingWidth, height, ZCube.MaxHeight);
                    }
                }
                cube.transform.localScale = new Vector3(1f, height, 1f);
            }

            var remainingTime = GameCore.Instance.RemainingTime;
            var prog = Mathf.Pow(15f - remainingTime, 2) * 0.25f;
            while (prog > 2f)
                prog -= 2f;
            _curMainColor = remainingTime < 15f ?
                (Color)Vector4.MoveTowards(MainColor, WarningBlinkColor, prog > 1f ? 2f - prog : prog) : MainColor;
            Shader.SetGlobalColor("_MAIN", _curMainColor);
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
                var amt = Random.Range(1, Mathf.Max(1, Mathf.RoundToInt(
                    ((float)_amountOfCubesWithinRange / transmissiveTypeAmt) * 0.04f)));
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
            const float maxRangeSq = MaxRange * MaxRange;
            const float minRangeSq = MinRange * MinRange;
            foreach (var zCube in Cubes) //set other cubes
            {
                if (zCube.Type == ZCubeType.Player)
                {
                    continue;
                }
                if ((GameCore.Instance.Player.GoalPosition - zCube.transform.position).magnitude < 0.5f)
                {
                    zCube.Type = ZCubeType.Goal;
                    continue;
                }

                var distSq = zCube.transform.localPosition.sqrMagnitude;
                if (distSq > minRangeSq && distSq < maxRangeSq)
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
