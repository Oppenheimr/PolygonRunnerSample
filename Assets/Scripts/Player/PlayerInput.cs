using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityUtils.BaseClasses;

namespace Player
{
    public class PlayerInput : SingletonBehavior<PlayerInput>, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public UnityEvent<int> onSwipeHorizontal;
        public UnityEvent<int> onSwipeVertical;
        
        private float _horizontalSwipeStartPos;
        private float _horizontalSwipeEndPos;
        
        private float _verticalSwipeStartPos;
        private float _verticalSwipeEndPos;
        
        private bool IsHorizontalSwiping => _horizontalSwipeStartPos != 0 && _horizontalSwipeEndPos != 0;
        private bool IsVerticalSwiping => _horizontalSwipeStartPos != 0 && _horizontalSwipeEndPos != 0;

        private void Update()
        {
            CheckSwipeInput();
        }

        private void CheckSwipeInput()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            
            if (Mathf.Abs(horizontal) > 0.1f)
                onSwipeHorizontal?.Invoke(horizontal > 0 ? 1 : -1);
            
            if (Mathf.Abs(vertical) > 0.1f)
                onSwipeVertical?.Invoke(vertical > 0 ? 1 : -1);
        }

        // Dokunmatik hareketin başlangıç pozisyonunu kaydet
        public void OnPointerDown(PointerEventData eventData)
        {
            _horizontalSwipeStartPos = eventData.position.x;
            _verticalSwipeStartPos = eventData.position.y;
        }

        // Dokunmatik hareketin bitiş pozisyonunu kaydet
        public void OnDrag(PointerEventData eventData)
        {
            _horizontalSwipeEndPos = eventData.position.x;
            _verticalSwipeEndPos = eventData.position.y;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // Hareket yönünü belirle
            float horizontalDirection = _horizontalSwipeEndPos - _horizontalSwipeStartPos;
            float verticalDirection = _verticalSwipeEndPos - _verticalSwipeStartPos;
            
            // Eğer yeterince kaydırma yapıldıysa
            if (Mathf.Abs(horizontalDirection) > Screen.width * 0.2f)
                onSwipeHorizontal?.Invoke((int)Mathf.Sign(horizontalDirection));
            if (Mathf.Abs(verticalDirection) > Screen.width * 0.2f)
                onSwipeVertical?.Invoke((int)Mathf.Sign(verticalDirection));
            
            _horizontalSwipeEndPos = 0;
            _horizontalSwipeStartPos = 0;
            _verticalSwipeEndPos = 0;
            _verticalSwipeStartPos = 0;
        }
    }
}