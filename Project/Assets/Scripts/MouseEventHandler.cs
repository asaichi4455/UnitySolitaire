using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Solitaire
{
    public class MouseEventHandler : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public event Action OnClick;
        public event Action<Vector2> OnBeginDrag;
        public event Action<Vector2> OnDrag;
        public event Action<Vector2> OnEndDrag;

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && !eventData.dragging)
            {
                OnClick?.Invoke();
            }
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                var pos = new Vector2(transform.position.x + eventData.delta.x, transform.position.y + eventData.delta.y);
                OnBeginDrag?.Invoke(pos);
            }
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                var pos = new Vector2(transform.position.x + eventData.delta.x, transform.position.y + eventData.delta.y);
                OnDrag?.Invoke(pos);
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                var pos = new Vector2(transform.position.x + eventData.delta.x, transform.position.y + eventData.delta.y);
                OnEndDrag?.Invoke(pos);
            }
        }
    }
}
