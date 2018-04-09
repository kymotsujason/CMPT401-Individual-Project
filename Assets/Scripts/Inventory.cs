namespace GoogleARCore.ARDDPS
{

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class Inventory : MonoBehaviour {
        [SerializeField] Transform slots;
        [SerializeField] Text log;
        public AudioSource audioClip;

        private void Update()
        {
            HasChanged();
        }

        public void HasChanged()
        {
            
            if (log.text.Equals("if result ") || log.text.Equals("while result "))
            {
                GetComponent<Drone>().BeginMove();
                audioClip.Play();
            } else
            {
                System.Text.StringBuilder builder = new System.Text.StringBuilder();
                builder.Append("");
                foreach (Transform slotTransform in slots)
                {
                    GameObject item = slotTransform.GetComponent<Slot>().Item;
                    if (item)
                    {
                        builder.Append(item.name);
                        builder.Append(" ");
                    }
                }
                log.text = builder.ToString();
            }
        }

    }
}