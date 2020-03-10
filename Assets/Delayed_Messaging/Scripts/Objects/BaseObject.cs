﻿using System;
using System.Collections.Generic;
using Delayed_Messaging.Scripts.Environment;
using Delayed_Messaging.Scripts.Player.Selection;
using Delayed_Messaging.Scripts.Utilities;
using Pathfinding;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;
using VR_Prototyping.Plugins.QuickOutline.Scripts;
using VR_Prototyping.Scripts;
using VR_Prototyping.Scripts.Utilities;
using Draw = Pathfinding.Util.Draw;
using Selection = Delayed_Messaging.Scripts.Player.Selection.Selection;

namespace Delayed_Messaging.Scripts.Objects
{
	[DisallowMultipleComponent, RequireComponent(typeof(ModelController))]
	public abstract class BaseObject : MonoBehaviour, IHoverable, ISelectable<SelectionObjects>
	{
		private GameObject playerObject;
		private Selection selection;
		[HideInInspector] public Player.Player player;
		[HideInInspector] public ModelController modelController;

		private Outline outline;
		private Vector3 defaultPosition;
		private Vector3 defaultLocalPosition;

		private Vector3 lClosestPoint;
		private Vector3 rClosestPoint;

		private bool active;
		private bool hover;
		private bool selected;
		private bool baseInitialised;

		public GraphNode currentNode;
		public GraphNode previousNode;

		private VisualEffect selectionVisualEffect;
		[HideInInspector] public GameObject model;
		internal float health;
		
		public enum SpawnableObjectType
		{
			UNIT,
			STRUCTURE
		}
		[Serializable] public struct SpawnableObject 
		{
			public string objectName;
			public GameObject objectPrefab;
			public SpawnableObjectType objectType;
		}
			
		public float AngleL { get; private set; }
		public float AngleR { get; private set; }
		public float AngleG { get; private set; }
		public float CastDistanceR { get; private set; }
		public float CastDistanceL { get; private set; }
		public Bounds ObjectBounds { get; set; }

		private const string SelectEvent = "Select";
		private const string DeselectEvent = "Deselect";

		public BaseClass ObjectClass { get; set; }

		[Header("Base Object Settings")] 
		[SerializeField] private ControllerTransforms.DebugType debugType;
		[SerializeField] protected bool nonSpawnedObject;

		[Header("Selection Aesthetics")] 
		[SerializeField] private GameObject selectionVisual;
		[SerializeField] internal float selectionRadius;
		
		[Header("Spawnable Units")] 
		public List<SpawnableObject> spawnableObjects;

		private void Start ()
		{
			InitialiseBaseObject();
		}
		public void InitialiseBaseObject()
		{
			ObjectInitialisation();
			Initialise();
		}
		protected abstract void Initialise();
		public void OnEnable()
		{
			ObjectInitialisation();
		}
		private void OnDestroy()
		{
			DestroyBaseObject();
		}
		public void OnDisable()
		{
			DestroyBaseObject();
		}
		private void DestroyBaseObject()
		{
			if (selection == null) return;
			
			baseInitialised = false;
			
			GameObject g = gameObject;
			g.ToggleList(selection.globalList, false);
			selection.baseObjectsList.Remove(this);
			
			g.ToggleList(selection.lCastList, false);
			g.ToggleList(selection.rCastList, false);
		}
		private void ObjectInitialisation()
		{
			if (baseInitialised) return;
			baseInitialised = true;
			
			AssignComponents();
			SetupOutline();
			SetupSelectedVisual();
			SetObjectBounds();
			SetupModelController();

			gameObject.ToggleList(selection.globalList, true);
			selection.baseObjectsList.Add(this);
			
			Debug.Log($"Base Object: <b>{name}</b> was initialised.");
		}
		private void AssignComponents()
		{
			if (playerObject == null)
			{
				foreach (GameObject rootGameObject in SceneManager.GetActiveScene().GetRootGameObjects())
				{
					if (rootGameObject.name != "[VR Player]") continue;
					playerObject = rootGameObject;
					//Debug.Log($"<b>[{name}] </b> player set to " + rootGameObject.name);
				}
			}
			selection = playerObject.GetComponent<Selection>();
			player = playerObject.GetComponent<Player.Player>();
			modelController = GetComponent<ModelController>();
		}
		private void SetupOutline()
		{
			outline = transform.AddOrGetOutline();
			outline.enabled = false;
			outline.precomputeOutline = true;
		}
		private void SetupSelectedVisual()
		{
			selectionVisual = Instantiate(selectionVisual, transform);
			selectionVisualEffect = selectionVisual.GetComponent<VisualEffect>();
			selectionVisualEffect.SetFloat("SelectionRadius", selectionRadius);
			//selectionVisualEffect.SetFloat("Health", Mathf.InverseLerp(0, objectClass.healthMax, health));
		}

		private void SetupModelController()
		{
			if (modelController == null)
			{
				modelController = gameObject.AddComponent<ModelController>();
			}
			modelController.InitialiseModelController(this);
			model = new GameObject("[Model Parent]");
			model.transform.SetParent(transform);
			
			model.transform.DefaultTransform();
		}
		private void Update()
		{				
			if (!baseInitialised) return; // Only call once this has been initialised
			
			GetSortingValues();
			
			GameObject o = gameObject;
			Transform t = transform;
			Vector3 position = t.position;
			
			currentNode = AstarPath.active.GetNearest(position).node;
			EnvironmentController.GeneratePath(currentNode, previousNode, ObjectClass.weight);
			previousNode = currentNode;
			
			//selectionVisualEffect.SetFloat("Health", Mathf.InverseLerp(0, objectClass.healthMax, health));

			o.ManageList(selection.lCastList, o.WithinCastDistance(selection.globalList, selection.castSelectionRadius, CastDistanceL));
			o.ManageList(selection.rCastList, o.WithinCastDistance(selection.globalList, selection.castSelectionRadius, CastDistanceR));
			
			
			
			ObjectUpdate();
		}
		public void SetObjectBounds()
		{
			ObjectBounds = transform.BoundsOfChildren(ObjectBounds);
		}
		protected abstract void ObjectUpdate();
		private void GetSortingValues()
		{
			if (selection == null
			    || selection.selectionObjectsL.castLocation == null
			    || selection.selectionObjectsR.castLocation == null)
			{
				return;
			}
			
			Vector3 castLocationL = selection.selectionObjectsL.castLocation.position;
			Vector3 castLocationR = selection.selectionObjectsR.castLocation.position;
			
			CastDistanceL = Vector3.Distance(lClosestPoint = ObjectBounds.ClosestPoint(castLocationL), castLocationL);
			CastDistanceR = Vector3.Distance(rClosestPoint = ObjectBounds.ClosestPoint(castLocationR), castLocationR);
		}
		public void SetModel(BaseClass.Model spawnModel)
		{
			modelController.SetModel(spawnModel);
		}
		private void SelectionVisual(string eventName)
		{
			selectionVisualEffect.SendEvent(eventName);
		}
		public virtual void HoverStart()
		{
			//outline.SetOutline(hoverOutlineMode, hoverOutlineWidth, hoverOutlineColour, true);
		}
		public virtual void HoverStay()
		{

		}
		public virtual void HoverEnd()
		{
			//outline.SetOutline(hoverOutlineMode, hoverOutlineWidth, hoverOutlineColour, false);
		}
		public virtual void SelectStart(SelectionObjects selectionObjects)
		{
			SelectionVisual(SelectEvent);
			selected = true;
			if (!selectionObjects.list.Contains(this))
			{
				selectionObjects.list.Add(this);
			}
		}
		public virtual void SelectHold(SelectionObjects selectionObjects)
		{

		}
		public void SelectHoldEnd(SelectionObjects selectionObjects)
		{
			
		}
		public virtual void QuickSelect(SelectionObjects selectionObjects)
		{
			SelectionVisual(SelectEvent);
			selected = true;
			Selection.ClearSelectedObjects(selectionObjects, this);
			if (!selectionObjects.list.Contains(this))
			{
				selectionObjects.list.Add(this);
			}
		}
		public virtual void Deselect(SelectionObjects selectionObjects)
		{
			SelectionVisual(DeselectEvent);
			selected = false;
			if (selectionObjects.list.Contains(this))
			{
				selectionObjects.list.Remove(this);
			}
		}
		private void OnDrawGizmos () 
		{
			if (debugType == ControllerTransforms.DebugType.ALWAYS)
			{
				DrawGizmos ();
			}
		}
		private void OnDrawGizmosSelected ()
		{
			if (debugType == ControllerTransforms.DebugType.SELECTED_ONLY)
			{
				DrawGizmos ();
			}
		}
		protected virtual void DrawGizmos ()
		{
			Draw.Gizmos.CircleXZ(transform.position, selectionRadius, Color.white);

			if (selection == null) return;
			Gizmos.color = Color.green;
			Gizmos.DrawWireCube(ObjectBounds.center, ObjectBounds.size);
				
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(lClosestPoint, .02f);
			Gizmos.DrawWireSphere(rClosestPoint, .02f);
			Gizmos.DrawRay(rClosestPoint, Vector3.Normalize(selection.selectionObjectsL.castLocation.position - rClosestPoint) * selection.castSelectionRadius);
			Gizmos.DrawRay(lClosestPoint, Vector3.Normalize(selection.selectionObjectsR.castLocation.position - lClosestPoint) * selection.castSelectionRadius);
		}
	}
}
