﻿using System;
using Spaces.Scripts.Utilities;
using UnityEngine;
using Outline = VR_Prototyping.Plugins.QuickOutline.Scripts.Outline;

namespace Spaces.Scripts.User_Interface.Interface_Elements
{
    [RequireComponent(typeof(Collider))]
    public abstract class BaseInterface : MonoBehaviour
    {
        public enum TriggerType { GRAB, SELECT, BOTH }
        public enum InteractionType { DIRECT, INDIRECT, BOTH }

        [Header("Base Interface Options")]
        public TriggerType triggerType = TriggerType.SELECT;
        public InteractionType interactionType = InteractionType.INDIRECT;

        [Header("References")]
        [SerializeField] protected GameObject model;
        
        [Header("Visual Options")] 
        public OutlineConfiguration outlineConfiguration;
        
        // Protected Variables
        protected Collider TriggerCollider => GetComponent<Collider>();
        public const string Interface = "Interface", Direct = "Direct";
        protected Outline outline;
        [Serializable] public class OutlineConfiguration
        {
            [Range(1, 10)] public float width = 10f;
            public Outline.Mode mode = Outline.Mode.OutlineAll;
            public Color color = new Color(1,1,1,1);
        }
        
        
        private void Awake()
        {
            // Create visual effect for hovering
            outline = model.Outline(outlineConfiguration);
            
            // Call abstract initialisation method
            Initialise();
        }

        /// <summary>
        /// I have built this in such a way that it shouldn't need to reference anything to work
        /// </summary>
        protected abstract void Initialise();
    }
}
