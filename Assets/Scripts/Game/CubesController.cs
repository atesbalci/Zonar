using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UniRx;
using UnityEngine;

namespace Game
{
    public class CubesController : MonoBehaviour
    {
        public List<ZCube> Cubes;
        public const float Gap = 1.1f;
        public GameObject CubePrefab;
        public const int WaveRadius = 1011;
        private const float RingWidth = 1f;
        private const float Speed = 2.5f;
        private const float ScreenBoundTolerance = 100f;

        private GameObject _cubeParent;

        public float Timer;
        private Tweener[] _tweeners;
        private float _curVisibility;

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
                    if (scrPos.x >= -ScreenBoundTolerance && scrPos.y >= -ScreenBoundTolerance &&
                        scrPos.x < Screen.width + ScreenBoundTolerance &&
                        scrPos.y < Screen.height + ScreenBoundTolerance)
                    {
                        var cube = Instantiate(CubePrefab, pos, Quaternion.identity).GetComponent<ZCube>();
                        cube.transform.SetParent(_cubeParent.transform);
                        Cubes.Add(cube);
                        cube.Init();
                        cube.SetCubeType();
                    }
                }
            }
            Cubes.First(x => Mathf.Approximately(Vector3.Distance(x.transform.position, Vector3.zero), 0f)).Type = ZCubeType.Player;
            Debug.Log(Cubes.Count);
        }

        private void Update()
        {
            var state = GameCore.Instance.State;
            if (state == GameState.AwaitingTransmission || state == GameState.LevelCompleted)
            {
                if (Timer < 3.5f)
                {
                    Timer += Time.deltaTime;
                    //Timer -= 3.5f;
                }
                else if(state != GameState.LevelCompleted)
                {
                    GameCore.Instance.State = GameState.GameOver;
                }
            }

            foreach (var cube in Cubes)
            {
                if (GameCore.Instance.State != GameState.Menu)
                {
                    if (state == GameState.AwaitingTransmission)
                    {
                        var scale = cube.transform.localScale;
                        if (cube.Type == ZCubeType.Player || cube.Type == ZCubeType.Goal)
                        {
                            scale = new Vector3(1f, ZCube.MaxHeight, 1f);
                        }
                        else
                        {
                            scale.y = CalculateHeight(cube.transform.localPosition.magnitude, Timer, Speed + GameCore.Instance.Player.Level * 0.25f,
                                RingWidth, 1f, ZCube.MaxHeight);
                        }

                        cube.transform.localScale = scale;
                    }
                    if (cube.Type == ZCubeType.Player)
                    {
                        cube.transform.localScale = new Vector3(1f, ZCube.MaxHeight, 1f);
                    }
                }

                //cube.RefreshColor(Mathf.PerlinNoise(cube.transform.localPosition.x + Time.time, cube.transform.localPosition.z + Time.time));
            }

            //ZCube.IdleColor = Color.Lerp(ZCube.IdleColor,
            //    state == GameState.Transmitting ? Color.black : ZCube.DefaultIdleColor , Time.deltaTime * 10f);
            var newVisibility = Mathf.Clamp01(_curVisibility + (state == GameState.Transmitting ? -1f : 1f) * Time.deltaTime * 4f);
            if (!Mathf.Approximately(newVisibility, _curVisibility))
            {
                _curVisibility = newVisibility;
                Shader.SetGlobalFloat("_MULTIPLIER", _curVisibility);
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
            foreach (var zCube in Cubes) //set other cubes
            {
                zCube.SetCubeType();
            }
        }
    }
}
