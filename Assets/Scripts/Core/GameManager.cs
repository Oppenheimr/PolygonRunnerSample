using System.Collections;
using Player;
using UnityEngine;
using UnityUtils.BaseClasses;
namespace Core
{
    public class GameManager : SingletonBehavior<GameManager>
    {
        private PlayerController _player;
        public PlayerController Player => _player ? _player : (_player = FindFirstObjectByType<PlayerController>());
        public int Score { get; set; }

        public bool IsPlay;
    }
}
