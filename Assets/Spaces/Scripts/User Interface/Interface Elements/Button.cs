﻿using UnityEngine;
using UnityEngine.Events;

namespace Spaces.Scripts.User_Interface.Interface_Elements
{
    public class Button : BaseInterface
    {
        public UnityEvent select;
        [HideInInspector] public UnityEvent grabStart, grabStay, grabEnd;

        protected override void Initialise()
        {
            gameObject.tag = Button;
        }

        public void HoverStart()
        {
            outline.enabled = true;
        }
        public void HoverEnd()
        {
            outline.enabled = false;
        }

        private void OnTriggerEnter(Collider userCollider)
        {
            HoverStart();
        }

        private void OnTriggerExit(Collider userCollider)
        {
            HoverEnd();
        }
    }
}