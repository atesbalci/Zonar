using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Game
{
    public class GameCore : MonoBehaviour
    {
        private TileHelper _tileHelper = new TileHelper();

        void Awake()
        {
            //_tileHelper.GenerateTiles(25 * 25); //250 for some reason
        }

        void Start()
        {
            //Observable.Timer(TimeSpan.FromSeconds(5)).Subscribe(l =>
            //{
            //    for (int i = 0; i < 500; i++)
            //    {
            //        var x = _tileHelper.FetchCube();
            //        x.transform.position = new Vector3(Random.Range(-10f,10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));
            //    }

            //});
        }

        public ZCube FetchCube()
        {
            return _tileHelper.FetchCube();
        }
    }

    public class TileHelper
    {
        private List<ZCube> _cubes = new List<ZCube>();
        private int _current;

        public void GenerateTiles(int count)
        {
            var parent = new GameObject("ZCubes");
            for (int i = 0; i < count; i++)
            {
                var prefab = Object.Instantiate(Resources.Load("ZCube")) as GameObject;
                if (prefab != null)
                {
                    prefab.transform.SetParent(parent.transform);
                    var zcube = prefab.AddComponent<ZCube>();
                    _cubes.Add(zcube);
                }
            }
        }

        public ZCube FetchCube()
        {
            var cube = _cubes[_current];
            _current++;
            if (_current == _cubes.Count)
            {
                _current = 0;
            }
            return cube;
        }
    }
}
