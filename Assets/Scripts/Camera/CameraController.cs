using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Connection;
using Events;
using Player.ActionHandlers;
using UnityEngine;
using Utils.MonoBehaviourUtils;

namespace Camera
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private float viewExtension = 2;
        [SerializeField] private float friction = 5;
        
        private Bounds? _bounds;
        private HashSet<ColorNode> _nodes = new ();

        private void Awake() 
        {
            // регистируем события появления/исчезания нодов для подсчета играбельной области
            EventsController.Subscribe<ColorNodeEnable>(this, OnNodeEnable);    
            EventsController.Subscribe<ColorNodeDisable>(this, OnNodeDisable);
            
            // в случае если ноды уже созданы, ищем их и регистируем
            foreach (var node in FindObjectsOfType<ColorNode>()) 
                OnNodeEnable(new ColorNodeEnable(node));
            
            // выравнимаем позицию камеру с играбельной области
            CropCameraPosition();
            
            // регистируем события перетаскивания, чтобы
            // своевременно начать/завершить движение камеры
            ClickHandler.Instance.AddDragEventHandlers(OnDragStart, OnDragEnd);
        }

        private void OnDestroy() 
        {
            // отписываемся ото всех событий
            EventsController.Unsubscribe<ColorNodeEnable>(OnNodeEnable);    
            EventsController.Unsubscribe<ColorNodeDisable>(OnNodeDisable);
            
            ClickHandler.Instance.RemoveDragEventHandlers(OnDragStart, OnDragEnd);
        }

        #region Bounds
        
        private void OnNodeEnable(ColorNodeEnable data) 
        {
            // если нода была уже добавлена, то ничего делать не нужно
            if (!_nodes.Add(data.node)) 
                return;
         
            // расширяем играбельную область
            Encapsulate(data.node.Bounds);
            
            // выравнимаем позицию камеру с учетом новой области
            CropCameraPosition();
        }
        
        private void OnNodeDisable(ColorNodeDisable data) 
        {
            // если ноды и так не было, ничего делать не нужно
            if (!_nodes.Remove(data.node)) 
                return;
            
            // пересчитываем область полностью
            _bounds = null;
            foreach (var node in _nodes) 
                Encapsulate(node.Bounds);
            
            // выравнимаем позицию камеру с учетом новой области
            CropCameraPosition();
        }

        private void Encapsulate(Bounds bounds)
        {
            // если область уже есть, то расширяем ее,
            // а если нет, то просто берем ее из параметра
            if (_bounds.HasValue) {
                var _b = _bounds.Value;
                _b.Encapsulate(bounds);
                _bounds = _b;
            } else 
                _bounds = bounds;
        }

        #endregion

        #region User Input

        private void OnDragStart(Vector3 point)
        {
            // если наткнулись на ноду, то ничего не делаем
            if (_nodes.Any(n => n.IsInBounds(point)))
                return;

            // если уже запущен процесс перетаскивания, то завершаем его
            if (_dragging != null) 
            {
                Coroutines.Instance.StopCoroutine(_dragging);
                _dragging = null;
            }
            
            // запускаем процесс перетаскивания
            _dragging = Dragging();
            Coroutines.Instance.StartCoroutine(_dragging);
        }
        
        private void OnDragEnd(Vector3 point) 
        {
            // обнуляем ссылку на перетаскивание, чтобы процесс перещел в стадию инерции
            _dragging = null;
        }
        
        #endregion

        #region Camera Position
        
        private void CropCameraPosition() 
        {
            var camera = CameraHolder.Instance.MainCamera;
            
            if (!camera) return;
            
            var cameraTransform = camera.transform;
            
            Vector3 position = (Vector2) cameraTransform.position;
            
            if (!_bounds.HasValue) 
            {
                // если нет области, то центрируем камеру
                position = default;
                
            } else {
                
                // высчитываем доступную область для камеры
                // с учетом параметра viewExtension
                var view = _bounds.Value;
                view.center = (Vector2) view.center;
                view.extents = new Vector2(
                    Mathf.Max(view.extents.x - camera.orthographicSize * camera.aspect + viewExtension, 0),
                    Mathf.Max(view.extents.y - camera.orthographicSize + viewExtension, 0));
                    
                
                // если камера и так в этой области, то цель достигнута
                if (view.Contains(position))
                    return;
                
                // берем ближайшую к камере точку в области 
                position = view.ClosestPoint(position);
            }

            position.z = cameraTransform.position.z;
            
            cameraTransform.position = position;
        }
        
        private IEnumerator _dragging = null;
        
        private IEnumerator Dragging() 
        {
            Vector2 lastPointerPosition = Input.mousePosition;
            Vector2 velocity = default;
            
            var camera = CameraHolder.Instance.MainCamera;
            
            // до тех пор пока есть ссылка на _dragging камера следует положению курсора
            while (_dragging != null && camera) 
            {
                var delta = (Vector2) Input.mousePosition - lastPointerPosition;
                
                // приводим дельту в простаноство World
                // (не делаем это через камеру, так как камера двигается и подсчет будет некорректным)
                delta *= camera.orthographicSize * 2 / Screen.height;
                
                // высчитываем скорость для будущего движения по инерции
                if (velocity == default) 
                    velocity = delta / Time.deltaTime;
                else
                    velocity += (delta / Time.deltaTime - velocity) * Time.deltaTime * 10;
                
                camera.transform.position -= (Vector3) delta;
                
                lastPointerPosition = Input.mousePosition;
                
                CropCameraPosition();
                
                yield return null;
            }

            // двигаем камеру по инерции,
            // пока не запущен новый процесс перетаскивания
            // или инерция не станет слишком низкой
            while (_dragging == null && camera && velocity.magnitude > 0.1f) 
            {
                var delta = velocity * Time.deltaTime;
                
                camera.transform.position -= (Vector3) delta;
                
                // уменьшаем скорость камеры из-за трения
                velocity -= velocity * Time.deltaTime * friction;
                
                CropCameraPosition();
                
                yield return null;
            }
        }

        #endregion

        private void OnDrawGizmos() 
        {
            if (!_bounds.HasValue)
                return;
            
            // считаем область с игровыми элементами
            // (обнуляем Z)
            var view = _bounds.Value;
            view.center = (Vector2) view.center;
            view.extents = (Vector2) view.extents;

            // отрисовываем ее в редакторе красным цветом
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(view.center, view.extents * 2);
            
            // расширяем область на параметр viewExtension
            view.Expand(viewExtension * 2);
            view.extents = (Vector2) view.extents;
            
            // отрисовываем ее в редакторе розовым цветом
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(view.center, view.extents * 2);
        }
    }
}