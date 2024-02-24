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
                _obstacleIds[i] = ObjectPooler.CreatePool(obstacles[i], (int)(levelScale /3));
            
            _tilePoolId = ObjectPooler.CreatePool(groundTile.transform, (int)(levelScale /6));
            _planeId = ObjectPooler.CreatePool(plane, (int)(levelScale /12));
            
            //Setup ground tiles
            for (int i = 0; i < (int)(levelScale /6); i++)
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
                
                wall.SetStartPosition(new Vector3(linePositionX, 6.25f,linePositionZ));
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
                                  planeDistance + Random.Range(0, planeDistance/ 3);
            
            var plane = (Plane)ObjectPooler.GetObject(_planeId);
            plane.SetStartPosition(new Vector3(linePositionX, 6.25f,linePositionZ));
            
        }

        public void SetupLineObstacles(float linePositionX)
        {
            float distance = levelScale;
            distance -= obstacleDistance * 2;
            
            for (int i = 0; i < distance; i++)
            {
                distance -= Random.Range(0, 10);
                float linePositionZ = levelScale - distance;
                
                var obstacle = GetObstacleFromPool(GetRandomObstacleType());
                obstacle.SetStartPosition(new Vector3(linePositionX, 1,linePositionZ));
                distance -= obstacleDistance + obstacle.scaleZAxis;
                
                SetLastSpawnedObstacleZ(linePositionX, linePositionZ + obstacle.scaleZAxis / 2);
            }
        }

        private void SetLastSpawnedObstacleZ(float linePositionX, float positionZ)
        {
            if (linePositionX == rightSideX)
                _lastSpawnedRightObstacleZ = positionZ;
            else if (linePositionX == leftSideX)
                _lastSpawnedLeftObstacleZ = positionZ;
            else if (linePositionX == middleSideX)
                _lastSpawnedMiddleObstacleZ = positionZ;
            else
                Debug.LogError("Line position is not valid!");
        }
        
        public void SetupLineObstacle(float linePositionX)
        {
            float lastPositionZ = 0;
            if (linePositionX == rightSideX)
                lastPositionZ = _lastSpawnedRightObstacleZ;
            else if (linePositionX == leftSideX)
                lastPositionZ = _lastSpawnedLeftObstacleZ;
            else if (linePositionX == middleSideX)
                lastPositionZ = _lastSpawnedMiddleObstacleZ;
            else
                Debug.LogError("Line position is not valid!");

            float spawnZ = lastPositionZ + obstacleDistance + Random.Range(0, 10);

            var obstacle = GetObstacleFromPool(GetRandomObstacleType());
            obstacle.SetStartPosition(new Vector3(linePositionX, 1,spawnZ));
            SetLastSpawnedObstacleZ(linePositionX, spawnZ);
        }
        
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