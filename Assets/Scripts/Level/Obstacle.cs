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
        public ObstacleTypes type;
        public float scaleZAxis = 1;
        
        [SerializeField] private Coin[] coins;
        [SerializeField] private Line line; 
        
        private bool _isSetup;
        
        private void OnEnable()
        {
            foreach (var coin in coins)
                coin.gameObject.SetActive(Random.Range(0, 2) == 1);
        }

        private IEnumerator SlowUpdate()
        {
            yield return new WaitForSeconds(2);
            
            if (_isSetup)
            {
                while (true)
                {
                    yield return new WaitForSeconds(Random.Range(0.1f, 0.4f));
                
                    if (GameManager.Instance.Player.transform.position.z < transform.position.z + 10)
                        continue;
                
                    Close();
                    break;
                }
            }
            else
            {
                gameObject.SetActive(false);
                Debug.LogError("Obstacle is not setup!");
            }
        }
        
        public void Close()
        {
            if (LevelGenerator.GetLineXAxis(line) != transform.position.x)
            {
                Debug.LogError("WTF");
            }
            
            gameObject.SetActive(false);
            LevelGenerator.Instance.SetupLineObstacle(line);
        }
        
        public void Setup(float zAxis, Line line)
        {
            _isSetup = true;
            var newPosition = new Vector3(LevelGenerator.GetLineXAxis(line), 1, zAxis + scaleZAxis/2);
            
            transform.position = newPosition;
            this.line = line;
            
            StartCoroutine(SlowUpdate());
        }
    }
}