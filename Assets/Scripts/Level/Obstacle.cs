using System;
using System.Collections;
using Core;
using Enums;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Level
{
    public class Obstacle : MonoBehaviour
    {
        [SerializeField] private Coin[] coins;
        public ObstacleTypes type;
        public float scaleZAxis = 1;

        private void OnEnable()
        {
            foreach (var coin in coins)
                coin.gameObject.SetActive(Random.Range(0, 2) == 1);
            
            StartCoroutine(SlowUpdate());
        }

        private IEnumerator SlowUpdate()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.4f));
                
                if (GameManager.Instance.Player.transform.position.z < transform.position.z + 10)
                    continue;
                
                gameObject.SetActive(false);
                LevelGenerator.Instance.SetupLineObstacle(transform.position.x);
                break;
            }
        }
        public void SetStartPosition(Vector3 position)
        {
            var newPosition = position;
            newPosition.z = position.z + scaleZAxis / 2;
            transform.position = newPosition;
        }
    }
}