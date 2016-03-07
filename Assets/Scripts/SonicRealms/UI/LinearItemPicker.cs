using System;
using System.Collections.Generic;
using SonicRealms.Core.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace SonicRealms.UI
{
    /// <summary>
    /// Displays all of its children in a line and scrolls forward and backward in response to input.
    /// </summary>
    [ExecuteInEditMode]
    public class LinearItemPicker : MonoBehaviour
    {
        protected Canvas Canvas;

        /// <summary>
        /// The currently selected object.
        /// </summary>
        [Tooltip("The currently selected object.")]
        public GameObject SelectedObject;

        /// <summary>
        /// Amount of space between each item.
        /// </summary>
        [Tooltip("Amount of space between each item.")]
        public float Spacing;

        /// <summary>
        /// The direction in which the children are displayed, in degrees.
        /// </summary>
        [Tooltip("The direction in which children are displayed, in degrees.")]
        [Range(-180f, 180f)]
        public float Direction;

        /// <summary>
        /// The current position of the picker.
        /// </summary>
        [Foldout("Scrolling")]
        [Tooltip("The current position of the picker.")]
        public float ScrollPosition;

        /// <summary>
        /// The time it takes to scroll once, in seconds.
        /// </summary>
        [Foldout("Scrolling")]
        [Tooltip("The time it takes to scroll once, in seconds.")]
        public float ScrollTime;


        /// <summary>
        /// Easing curve used for the scrolling animation.
        /// </summary>
        [Foldout("Scrolling")]
        [Tooltip("Easing curve used for the scrolling animation.")]
        public AnimationCurve ScrollCurve;

        /// <summary>
        /// If checked, the picker will not scroll in response to input.
        /// </summary>
        [Space, Foldout("Scrolling")]
        [Tooltip("If checked, the picker will not scroll in response to input.")]
        public bool DisableInput;

        /// <summary>
        /// The input axis used for scrolling.
        /// </summary>
        [Foldout("Scrolling")]
        [Tooltip("The input axis used for scrolling.")]
        public string ScrollAxis;

        /// <summary>
        /// Whether to invert the input axis.
        /// </summary>
        [Foldout("Scrolling")]
        [Tooltip("Whether to invert the input axis.")]
        public bool InvertAxis;

        /// <summary>
        /// Invoked when a new item is selected.
        /// </summary>
        [Foldout("Events")]
        public UnityEvent OnSelect;

        [Foldout("Debug")] public List<Transform> Items;
        [Foldout("Debug")] public bool ScrollAxisActive;
        [Foldout("Debug")] public bool Scrolling;
        [Foldout("Debug")] public float ScrollTimer;
        [Foldout("Debug")] public GameObject ScrollTarget;
        [Foldout("Debug")] public float ScrollStartPosition;
        [Foldout("Debug")] public float ScrollTargetPosition;


        public void Reset()
        {
            Direction = 0;
            Spacing = 100f;
            ScrollTime = 0.5f;

            ScrollAxis = "Vertical";
            InvertAxis = true;
            ScrollCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

            OnSelect = new UnityEvent();
        }

        public void Awake()
        {
            ScrollTimer = 0f;
            Items = Items ?? new List<Transform>();
            UpdateItems();

            Scrolling = false;
            ScrollTimer = 0f;
            ScrollTarget = null;
            ScrollTargetPosition = 0f;
            ScrollPosition = 0f;

            OnSelect = OnSelect ?? new UnityEvent();
            ScrollAxisActive = false;
        }

        public void OnEnable()
        {
            Items = Items ?? new List<Transform>();
        }

        public void Start()
        {
            Canvas = FindObjectOfType<Canvas>();
        }

        public void OnTransformChildrenChanged()
        {
            UpdateItems();
        }

        /// <summary>
        /// Refreshes the picker's list of items.
        /// </summary>
        public void UpdateItems()
        {
            Items.Clear();
            foreach (var t in transform)
            {
                Items.Add(t as Transform);
            }
        }

        /// <summary>
        /// Positions children according to the given scroll position.
        /// </summary>
        /// <param name="scrollPosition"></param>
        public void PositionChildren(float scrollPosition)
        {
            var directionRadians = Direction*Mathf.Deg2Rad;
            for (var i = 0; i < Items.Count; ++i)
            {
                var child = Items[i];
                child.transform.position = transform.position -
                                           (Vector3)(DMath.AngleToVector(directionRadians)*(i - scrollPosition)*Spacing*
                                           Canvas.transform.localScale.x);
            }
        }

        /// <summary>
        /// Scrolls to the target position.
        /// </summary>
        /// <param name="targetPosition"></param>
        public void ScrollTo(float targetPosition)
        {
            Scrolling = true;
            ScrollTimer = 0f;
            ScrollStartPosition = ScrollPosition;
            ScrollTargetPosition = targetPosition;
            ScrollTarget = GetClosest(targetPosition);
        }

        /// <summary>
        /// Scrolls to the next item. No effect if the last item is selected.
        /// </summary>
        public void ScrollNext()
        {
            if (Items.Count == 0 || SelectedObject.transform == Items[Items.Count - 1]) return;
            ScrollTo(Mathf.Round(ScrollTargetPosition + 1f));
        }

        /// <summary>
        /// Scrolls to the previous item. No effect if the first item is selected.
        /// </summary>
        public void ScrollPrevious()
        {
            if (Items.Count == 0 || SelectedObject.transform == Items[0]) return;
            ScrollTo(Mathf.Round(ScrollTargetPosition - 1f));
        }

        public GameObject GetClosest(float scrollPosition)
        {
            var result = Items[Mathf.RoundToInt(Mathf.Clamp(scrollPosition, 0f, Items.Count - 1))];
            if (result == null) return null;
            return result.gameObject;
        }

        /// <summary>
        /// Updates the currently selected object based on the scroll position.
        /// </summary>
        public void UpdateSelectedObject()
        {
            GameObject newSelectedObject;

            if (Items.Count == 0)
                newSelectedObject = null;
            else if (ScrollTarget != null)
                newSelectedObject = ScrollTarget.gameObject;
            else
                newSelectedObject = GetClosest(ScrollPosition);

            if (newSelectedObject != SelectedObject || (newSelectedObject == null && SelectedObject != null))
            {
                SelectedObject = newSelectedObject;
                OnSelect.Invoke();
            }
        }

        /// <summary>
        /// Clamps the scroll position between the possible items.
        /// </summary>
        public void ClampScrollPosition()
        {
            ScrollPosition = Mathf.Clamp(ScrollPosition, 0f, Mathf.Max(0f, Items.Count - 1));
        }

        /// <summary>
        /// Checks for axis input and scrolls in response to it.
        /// </summary>
        public void CheckAxisInput()
        {
            var axis = Input.GetAxisRaw(ScrollAxis)*(InvertAxis ? 1f : -1f);
            if (ScrollAxisActive && axis == 0f)
            {
                ScrollAxisActive = false;
            }
            else if (!ScrollAxisActive && axis != 0f)
            {
                ScrollAxisActive = true;
                if (axis < 0f) ScrollPrevious();
                else ScrollNext();
            }
        }

        /// <summary>
        /// Handles the picker's position when scrolling.
        /// </summary>
        public void UpdateScrolling()
        {
            if ((ScrollTimer += Time.deltaTime) > ScrollTime)
            {
                Scrolling = false;
                ScrollTimer = ScrollTime;
            }

            ScrollPosition = Mathf.Lerp(ScrollStartPosition, ScrollTargetPosition,
                ScrollCurve.Evaluate(ScrollTimer / ScrollTime));
        }

        public void Update()
        {
            if (Scrolling) UpdateScrolling();
            if (!DisableInput) CheckAxisInput();

            UpdateSelectedObject();
            ClampScrollPosition();
            PositionChildren(ScrollPosition);
        }
    }
}
