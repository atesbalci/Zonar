﻿using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class CubesController : MonoBehaviour
    {
        public GameObject CubePrefab;
        private const int WaveRadius = 40;
        private List<ZCube> _cubes;
        private float _timer;

        private void Start()
        {
            Tile();
        }

        public void Tile()
        {
            _cubes = new List<ZCube>();
            for (int x = 0; x < WaveRadius; x++)
            {
                for (int y = 0; y < WaveRadius; y++)
                {
                    var cube = Instantiate(CubePrefab, new Vector3(x - 40 / 2f, 0f, y - 40 / 2f) * 1.1f, Quaternion.identity).GetComponent<ZCube>();
                    _cubes.Add(cube);
                    cube.SetCubeType();
                }
            }
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
                scale.y = CalculateHeight(Vector3.Distance(Vector3.zero, cube.transform.position), _timer, 3f, 1f, 1f, ZCube.MaxHeight);
                cube.transform.localScale = scale;
            }
        }

        private static float CalculateHeight(float distance, float time, float speed, float width, float min, float max)
        {
            return Mathf.Clamp(max - Mathf.Abs(Mathf.Max(0f, Mathf.Abs(distance - time * speed) - width) * (max - min)), min, max);
        }
    }
}
