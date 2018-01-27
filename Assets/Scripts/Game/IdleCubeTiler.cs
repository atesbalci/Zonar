using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Game
{
    public class IdleCubeTiler : MonoBehaviour
    {
        private const int Radius = 61;
        private static readonly Color IdleColor = new Color(0.17f, 0.17f, 0.17f);

        public GameObject IdleCubePrefab;

        private Transform _idleCubesParent;
        private List<Tuple<Transform, Renderer, MaterialPropertyBlock>> _cubes;
        private Transform _camLook;

        private void Start()
        {
            _idleCubesParent = new GameObject("IdleCubesParent").transform;
            _idleCubesParent.SetParent(transform);
            Tile();
            _camLook = new GameObject("CamLook").transform;
            _camLook.SetParent(Camera.main.transform);
        }

        public void Tile()
        {
            _cubes = new List<Tuple<Transform, Renderer, MaterialPropertyBlock>>();
            for (int x = 0; x < Radius; x++)
            {
                for (int y = 0; y < Radius; y++)
                {
                    //It's working don't fix it
                    var cube = Instantiate(IdleCubePrefab, new Vector3(x - Radius / 2 - 1, 0f, y - Radius / 2 - 1) * CubesController.Gap, Quaternion.identity);
                    cube.transform.SetParent(_idleCubesParent);
                    var tuple = new Tuple<Transform, Renderer, MaterialPropertyBlock>(cube.transform,
                        cube.GetComponentInChildren<Renderer>(), new MaterialPropertyBlock());
                    _cubes.Add(tuple);
                    tuple.Item3.SetColor("_Color", ZCube.IdleColor);
                    tuple.Item2.SetPropertyBlock(tuple.Item3);
                }
            }
        }

        private void Update()
        {
            var camPos = _camLook.position;
            camPos = Physics.BoxCastAll(camPos + Vector3.up * 20, Vector3.one / 4f, Vector3.down * 20)
                .OrderBy(x => x.distance).First().transform.position;
            foreach (var cube in _cubes)
            {
                var pos = cube.Item1.position;
                if (Vector3.Distance(pos, camPos) > Radius / 2f)
                {
                    pos = camPos + (camPos - pos);
                    cube.Item1.position = pos;
                }
                cube.Item3.SetColor("_Color", IdleColor + (Mathf.PerlinNoise(pos.x + Time.time, pos.z + Time.time) - 0.5f) * new Color(0.03f, 0.03f, 0.03f));
                cube.Item2.SetPropertyBlock(cube.Item3);
            }
        }
    }
}
