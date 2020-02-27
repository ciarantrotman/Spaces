﻿using System;
using Delayed_Messaging.Scripts.Utilities;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using VR_Prototyping.Scripts;

namespace Delayed_Messaging.Scripts.Interaction
{
    public abstract class RaycastInterface : MonoBehaviour, IRaycastInterface, IHoverable
    {
        internal UserInterface userInterface;
        internal UnityEvent OnSelect;
        
        private Bounds bounds;
        private BoxCollider interfaceCollider;

        private MeshRenderer visualRenderer;
        private static readonly int State = Shader.PropertyToID("_State");

        [Header("Base Interface Settings")]
        [SerializeField, Range(0, 1)] private float fadeDuration = .2f;

        private float state;

        private void Start()
        {
            if (userInterface != null) return;
            foreach (GameObject rootGameObject in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (rootGameObject.name != "[VR Player]") continue;
                userInterface = rootGameObject.GetComponent<UserInterface>();
                Debug.Log("<b>[RAYCAST INTERFACE] </b>" + name + " player set to " + rootGameObject.name);
            }

            interfaceCollider = gameObject.AddComponent<BoxCollider>();

            visualRenderer = GetComponentInChildren<MeshRenderer>();
            interfaceCollider.size = transform.BoundsOfChildren(bounds).size;
            
        }

        public void Select()
        {
            OnSelect.Invoke();
        }

        public void HoverStart()
        {
            DOTween.To(() => state, x => state = x, 1, fadeDuration);
            visualRenderer.material.SetFloat(State, 1);
            interfaceCollider.size = transform.BoundsOfChildren(bounds).size;
        }

        public void HoverStay()
        {
            
        }

        public void HoverEnd()
        {
            DOTween.To(() => state, x => state = x, 0, fadeDuration);
            visualRenderer.material.SetFloat(State, 0);
            interfaceCollider.size = transform.BoundsOfChildren(bounds).size;
        }
    }
}