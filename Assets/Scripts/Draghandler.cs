namespace GoogleARCore.ARDDPS
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class Draghandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        // User only drags 1 item at a time
        public static GameObject itemBeingDragged;
        Vector3 startPosition;
        Transform startParent;

        public void OnBeginDrag(PointerEventData eventData)
        {
            itemBeingDragged = gameObject;
            startPosition = transform.position;
            startParent = transform.parent;
            GetComponent<CanvasGroup>().blocksRaycasts = false;
            
            transform.SetParent(transform.root);
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = Input.GetTouch(0).position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            itemBeingDragged = null;

            if (transform.parent == startParent || transform.parent == transform.root)
            {
                transform.position = startPosition;
                transform.SetParent(startParent);
            }

            GetComponent<CanvasGroup>().blocksRaycasts = true;
        }

        // Use this for initialization
        //void Start () {

        //}

        // Update is called once per frame
        //void Update () {

        //}
    }
}


