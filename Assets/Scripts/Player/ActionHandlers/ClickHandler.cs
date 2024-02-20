using System;
using UnityEngine;


namespace Player.ActionHandlers
{
    public class ClickHandler : MonoBehaviour
    {
        [SerializeField] private Camera gameCamera;
        [SerializeField] private float clickToDragDuration;

        public event Action<Vector3> DragStartEvent;
        public event Action<Vector3> DragEndEvent;

        private Vector3 _startDragPosition;

        private bool _isClick;
        private bool _isDrag;
        private float _clickHoldDuration;


        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _isClick = true;
                _clickHoldDuration = .0f;

                _startDragPosition = gameCamera.ScreenToWorldPoint(Input.mousePosition);
                _startDragPosition = new Vector3(_startDragPosition.x, _startDragPosition.y, .0f);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (_isDrag)
                {
                    Vector2 dragEndPosition = gameCamera.ScreenToWorldPoint(Input.mousePosition);
                    DragEndEvent?.Invoke(dragEndPosition);

                    _isDrag = false;
                }

                _isClick = false;
            }
        }

        private void LateUpdate()
        {
            if (!_isClick)
                return;

            _clickHoldDuration += Time.deltaTime;
            if (_clickHoldDuration >= clickToDragDuration)
            {
                DragStartEvent?.Invoke(_startDragPosition);

                _isClick = false;
                _isDrag = true;
            }
        }

        public void SetDragEventHandlers(Action<Vector3> dragStartEvent, Action<Vector3> dragEndEvent)
        {
            ClearEvents();

            DragStartEvent = dragStartEvent;
            DragEndEvent = dragEndEvent;
        }

        public void ClearEvents()
        {
            DragStartEvent = null;
            DragEndEvent = null;
        }
    }
}