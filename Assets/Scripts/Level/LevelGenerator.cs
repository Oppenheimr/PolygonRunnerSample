using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using Enums;
using UnityEngine;
using UnityUtils.BaseClasses;
using Random = UnityEngine.Random;

namespace Level
{
    public class LevelGenerator : SingletonBehavior<LevelGenerator>
    {
        [Header("Properties")]
        [SerializeField] private float rightSideX = 3.5f;
        [SerializeField] private float middleSideX = 0;
        [SerializeField] private float leftSideX = -3.5f;

        [SerializeField] private float levelScale = 300;
        [SerializeField] private float obstacleDistance = 10;
        [SerializeField] private float planeDistance = 50;

        [Header("Spawn")]
        [SerializeField] private GameObject groundTile;
        [SerializeField] private Obstacle[] obstacles;
        [SerializeField] private Plane plane;

        private int[] _obstacleIds;
        private int _planeId;
        private int _tilePoolId;
        private float _lastSpawnedGroundZ;

        private float _lastSpawnedRightObstacleZ;
        private float _lastSpawnedMiddleObstacleZ;
        private float _lastSpawnedLeftObstacleZ;


        private void Start()
        {
            _lastSpawnedGroundZ = -5;
            _obstacleIds = new int[obstacles.Length];

            for (int i = 0; i < obstacles.Length; i++)
                _obstacleIds[i] = ObjectPooler.CreatePool(obstacles[i], (int)(levelScale));

            _tilePoolId = ObjectPooler.CreatePool(groundTile.transform, (int)(levelScale / 6));
            _planeId = ObjectPooler.CreatePool(plane, (int)(levelScale / 12));

            //Setup ground tiles
            for (int i = 0; i < (int)(levelScale / 6); i++)
            {
                _lastSpawnedGroundZ += 5;
                var tile = ObjectPooler.GetObject(_tilePoolId);
                tile.transform.position = new Vector3(0, 0, _lastSpawnedGroundZ);

            }

            StartCoroutine(SlowUpdate());
            SetupLineObstacles(middleSideX);
            SetupLineObstacles(leftSideX);
            SetupLineObstacles(rightSideX);
            SetupPlanes();
        }

        private void SetupPlanes()
        {
            float linePositionX;
            float distance = levelScale;
            distance -= planeDistance * 2;

            for (int i = 0; i < distance; i++)
            {
                distance -= Random.Range(0, 10);

                linePositionX = Random.Range(0, 3) switch
                {
                    0 => leftSideX,
                    1 => middleSideX,
                    2 => rightSideX,
                    _ => middleSideX
                };

                float linePositionZ = levelScale - distance;

                var wall = (Plane)ObjectPooler.GetObject(_planeId);

                wall.SetStartPosition(new Vector3(linePositionX, 6.25f, linePositionZ));
                distance -= planeDistance;
            }
        }

        public void SetupPlane()
        {
            float linePositionX = Random.Range(0, 3) switch
            {
                0 => leftSideX,
                1 => middleSideX,
                2 => rightSideX,
                _ => middleSideX
            };

            float linePositionZ = GameManager.Instance.Player.transform.position.z +
                                  planeDistance + Random.Range(0, planeDistance / 3);

            var plane = (Plane)ObjectPooler.GetObject(_planeId);
            plane.SetStartPosition(new Vector3(linePositionX, 6.25f, linePositionZ));

        }

        public void SetupLineObstacles(float linePositionX)
        {
            var line = GetLineType(linePositionX);
            float distance = levelScale;
            distance -= obstacleDistance * 2;

            for (int i = 0; i < distance; i++)
            {
                distance -= Random.Range(0, 10);
                float linePositionZ = levelScale - distance;

                var obstacle = GetObstacleFromPool(GetRandomObstacleType());
                obstacle.Setup(linePositionZ, line);
                distance -= obstacleDistance + obstacle.scaleZAxis;

                SetLastSpawnedObstacleZ(line, linePositionZ + obstacle.scaleZAxis / 2);
            }
        }

        private void SetLastSpawnedObstacleZ(Line line, float positionZ)
        {
            switch (line)
            {
                case Line.Right:
                    _lastSpawnedRightObstacleZ = positionZ;
                    break;
                case Line.Left:
                    _lastSpawnedLeftObstacleZ = positionZ;
                    break;
                case Line.Middle:
                    _lastSpawnedMiddleObstacleZ = positionZ;
                    break;
                default:
                    Debug.LogError("Line position is not valid!");
                    break;
            }
        }

        public void SetupLineObstacle(Line line)
        {
            float lastPositionZ = 0;
            
            switch (line)
            {
                case Line.Right:
                    lastPositionZ = _lastSpawnedRightObstacleZ;
                    break;
                case Line.Left:
                    lastPositionZ = _lastSpawnedLeftObstacleZ;
                    break;
                case Line.Middle:
                    lastPositionZ = _lastSpawnedMiddleObstacleZ;
                    break;
                default:
                    Debug.LogError("Line position is not valid!");
                    break;
            }
            
            if (lastPositionZ < 50 + GameManager.Instance.Player.transform.position.z)
                lastPositionZ = 50 + GameManager.Instance.Player.transform.position.z;

            float spawnZ = lastPositionZ + obstacleDistance + Random.Range(0, 10);

            var obstacle = GetObstacleFromPool(GetRandomObstacleType());
            obstacle.Setup(spawnZ, line);
            SetLastSpawnedObstacleZ(line, spawnZ);
        }

        public static Line GetLineType(float linePositionX)
        {
            if (Instance== null)
            {
                Debug.LogError("Instance is null!");
                return Line.Middle;
            }
            
            if (linePositionX == Instance.leftSideX)
                return Line.Left;
            
            if (linePositionX == Instance.middleSideX)
                return Line.Middle;
            
            if (linePositionX == Instance.rightSideX)
                return Line.Right;
            
            Debug.LogError("Line position is not valid!");
            return Line.Middle;
        }

        public static float GetLineXAxis(Line line) => line switch
        {
            Line.Right => Instance.rightSideX,
            Line.Middle => Instance.middleSideX,
            Line.Left => Instance.leftSideX,
            _ => throw new ArgumentOutOfRangeException(nameof(line), line, "Line position is not valid!")
        };
        
        private ObstacleTypes GetRandomObstacleType() =>
            (ObstacleTypes)(Random.Range(0, Enum.GetNames(typeof(ObstacleTypes)).Length));
        
        private Obstacle GetObstacleFromPool(ObstacleTypes type)
        {
            //Optimize edilebilir...
            List<Obstacle> findedObstacles = new List<Obstacle>();
            for (int i = 0; i < obstacles.Length; i++)
            {
                if (obstacles[i].type == type)
                    findedObstacles.Add((Obstacle)ObjectPooler.GetObject(_obstacleIds[i]));
            }

            if (findedObstacles.Count != 0)
            {
                var result = findedObstacles[Random.Range(0, findedObstacles.Count)];
                result.gameObject.SetActive(true);
                return result;
            }

            Debug.LogError("Obstacle type is not valid!");
            return null;

        }

        private IEnumerator SlowUpdate()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(0.01f, 0.2f));

                if (!(_lastSpawnedGroundZ - GameManager.Instance.Player.transform.position.z < levelScale / 3))
                    continue;

                _lastSpawnedGroundZ += 5;
                var tile = ObjectPooler.GetObject(_tilePoolId);
                tile.transform.position = new Vector3(0, 0, _lastSpawnedGroundZ);
            }
        }
    }
}