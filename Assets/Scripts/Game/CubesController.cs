using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Game
{
    public class CubesController : MonoBehaviour
    {
        public GameObject CubePrefab;
        private GameObject _cubeParent;
        private const int WaveRadius = 41;
        private List<ZCube> _cubes;
        private float _timer;

        private void Start()
        {
            Tile();
            MessageBroker.Default.Receive<SetUserCubeEvent>().Subscribe(ev =>
            {
                SetUserCube(ev.Cube);
            });

        }

        public void Tile()
        {
            _cubes = new List<ZCube>();
            _cubeParent = new GameObject("Cubes");
            for (int x = 0; x < WaveRadius; x++)
            {
                for (int y = 0; y < WaveRadius; y++)
                {
                    var cube = Instantiate(CubePrefab, new Vector3(x - WaveRadius / 2f, 0f, y - WaveRadius / 2f) * 1.1f, Quaternion.identity).GetComponent<ZCube>();
                    cube.transform.SetParent(_cubeParent.transform);
                    _cubes.Add(cube);
                    cube.SetCubeType();
                }
            }
            _cubes[(WaveRadius/2+1) * WaveRadius + WaveRadius/2+1].Type = ZCubeType.Player;
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer > 7f)
            {
                _timer -= 7f;
            }
            foreach (var cube in _cubes)
            {
                var scale = cube.transform.localScale;
                if (cube.Type == ZCubeType.Player)
                {
                    scale = new Vector3(1f,ZCube.MaxHeight,1f);
                }
                else
                {
                    scale.y = CalculateHeight(Vector3.Distance(Vector3.zero, cube.transform.position), _timer, 3f, 1f, 1f, ZCube.MaxHeight);
                }
                cube.transform.localScale = scale;
            }
        }

        private static float CalculateHeight(float distance, float time, float speed, float width, float min, float max)
        {
            return Mathf.Clamp(max - Mathf.Abs(Mathf.Max(0f, Mathf.Abs(distance - time * speed) - width) * (max - min)), min, max);
        }

        public void SetUserCube(ZCube cube)
        {
            _cubeParent.transform.position = cube.transform.position;
            foreach (var zCube in _cubes) //set other cubes
            {
                zCube.SetCubeType();
            }
        }
    }

    public class SetUserCubeEvent
    {
        public ZCube Cube { get; set; }
    }
}
