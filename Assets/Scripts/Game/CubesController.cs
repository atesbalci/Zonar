using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class CubesController : MonoBehaviour
    {
        private List<ZCube> _cubes;

        private void Start()
        {
            _cubes = new List<ZCube>();
            var core = FindObjectOfType<GameCore>();
            for (int x = 0; x < 25; x++)
            {
                for (int y = 0; y < 25; y++)
                {
                    var cube = core.FetchCube();
                    _cubes.Add(cube);
                    cube.transform.position = new Vector3(x - 25 / 2f, 0f, y - 25 / 2f);
                }
            }
        }

        private void Update()
        {
            foreach (var cube in _cubes)
            {
                var scale = cube.transform.localScale;
                scale.y = CalculateHeight(Vector3.Distance(Vector3.zero, cube.transform.position), Time.time, 1f, 2f, 1f, 10f);
                cube.transform.localScale = scale;
            }
        }

        private static float CalculateHeight(float distance, float time, float speed, float width, float min, float max)
        {
            return Mathf.Clamp(max - Mathf.Abs(Mathf.Max(0f, distance - time * speed - width) * (max - min)), min, max);
        }
    }
}
