namespace GoogleARCore.ARDDPS
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class Slot : MonoBehaviour, IDropHandler
    {
        public Text log;
        public GameObject Item
        {
            get
            {
                if (transform.childCount > 0)
                {
                    return transform.GetChild(0).gameObject;
                }
                return null;
            }
        }

        #region IDropHandler implementation
        public void OnDrop(PointerEventData eventData)
        {
            if (!Item)
            {
                Draghandler.itemBeingDragged.transform.SetParent(transform);
            }
        }
        #endregion
        // Use this for initialization
        //void Start () {

        //}

        // Update is called once per frame
        //void Update () {

        //}
    }
}


