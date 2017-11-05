using System;
using System.Collections;
using System.Collections.Generic;
using SonicRealms.Core.Internal;
using SonicRealms.Core.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SonicRealms.Legacy.UI
{
    /// <summary>
    /// When selected, moves between a list of items from joystick or keyboard input.
    /// </summary>
    [ExecuteInEditMode, SelectionBase, RequireComponent(typeof(Selectable), typeof(EventTrigger))]
    public class SrLegacyItemCarousel : MonoBehaviour
    {
        #region Nested Types

        public class SelectionChangedEvent : UnityEvent<SelectionChangedEvent.Args>
        {
            [Serializable]
            public class Args
            {
                public readonly int OldIndex;
                public readonly GameObject OldSelection;
                public readonly int NewIndex;
                public readonly GameObject NewSelection;

                public Args(int oldIndex, GameObject oldSelection, int newIndex, GameObject newSelection)
                {
                    OldIndex = oldIndex;
                    OldSelection = oldSelection;
                    NewIndex = newIndex;
                    NewSelection = newSelection;
                }
            }
        }

        /// <summary>
        /// Decides when item carousel is focused and listens for input.
        /// </summary>
        public enum FocusBehavior
        {
            Always,
            MustBeSelected,
            MustBeSubmitted,
        }

        /// <summary>
        /// Decides what happens when the item carousel is selecting its first or last item and receives input to go 
        /// out of bounds.
        /// </summary>
        public enum ClampBehavior
        {
            Loop,
            Constrain,
            Exit,
        }

        /// <summary>
        /// Represents the directions in which the item carousel can select an item adjacent to the one currently selected.
        /// </summary>
        public enum Direction
        {
            None,
            Previous,
            Next,
        }

        public enum NavigationDirection
        {
            Vertical,
            Horizontal,
        }
        #endregion

        #region Public Fields & Properties
        /// <summary>
        /// Number of items in the carousel.
        /// </summary>
        public int ItemCount { get { return _items == null ? 0 : _items.Count; } }

        /// <summary>
        /// Whether the carousel can go forward and select the next item.
        /// </summary>
        public bool HasNext
        {
            get
            {
                return ItemCount > 1 &&
                       (ClampMode == ClampBehavior.Loop || SelectedIndex < ItemCount - 1);
            }
        }

        /// <summary>
        /// Whether the carousel can go backward and select the previous item.
        /// </summary>
        public bool HasPrevious
        {
            get
            {
                return ItemCount > 1 &&
                       (ClampMode == ClampBehavior.Loop || SelectedIndex > 0);
            }
        }

        /// <summary>
        /// Whether the item carousel is listening to player input.
        /// </summary>
        public bool IsFocused { get { return _isFocused; } }

        /// <summary>
        /// Index of the currently selected item in the carousel, from zero to <see cref="ItemCount"/>.
        /// </summary>
        public int SelectedIndex { get { return _selectedIndex; } }

        public GameObject SelectedItem { get { return Get(SelectedIndex); } }

        /// <summary>
        /// When this input axis is negative, the carousel will select the previous item. When positive, it will 
        /// select the next item.
        /// </summary>
        public string InputAxis { get { return _inputAxis; } set { _inputAxis = value; } }

        public bool InvertAxis { get { return _invertAxis; } set { _invertAxis = value; } }

        /// <summary>
        /// The direction in which the item carousel scrolls through its items. This doesn't affect appearance, only 
        /// input and UI navigation. Appearance is handled by <see cref="Arranger"/>.
        /// </summary>
        public NavigationDirection InputDirection { get { return _inputDirection; } set { _inputDirection = value; } }

        /// <summary>
        /// Decides when item carousel is focused and listens for input.
        /// </summary>
        public FocusBehavior FocusMode { get { return _focusMode; } set { _focusMode = value; } }

        /// <summary>
        /// Decides what happens when the item carousel is selecting its first or last item and receives input to go 
        /// out of bounds.
        /// </summary>
        public ClampBehavior ClampMode { get { return _clampMode; } set { _clampMode = value; } }

        /// <summary>
        /// Whether the carousel's item list should be set to the immediate children of its transform.
        /// </summary>
        public bool SetItemsToChildren { get { return _setItemsToChildren; } set { _setItemsToChildren = value; } }

        /// <summary>
        /// The component that handles the position and orientation, and selection change of its items.
        /// </summary>
        public SrLegacyItemCarouselArranger Arranger { get { return _arranger; } }
        
        public SelectionChangedEvent OnSelectionChange { get { return _onSelectionChange; } }

        public UnityEvent OnSelectNext { get { return _onSelectNext; } }

        public UnityEvent OnSelectPrevious { get { return _onSelectPrevious; } }

        public UnityEvent OnFocusEnter { get { return _onFocusEnter; } }

        public UnityEvent OnFocusExit { get { return _onFocusExit; } }

        public UnityEvent OnItemsChanged { get { return _onItemsChanged; } }

        #endregion

        #region Private & Inspector Fields
        [SerializeField, SrFoldout("Input")]
        private string _inputAxis;

        [SerializeField, SrFoldout("Input")]
        private bool _invertAxis;
        
        [SerializeField, SrFoldout("Input")]
        [Tooltip("The direction in which the item carousel scrolls through its items. This doesn't affect appearance, only " +
                 "input and UI navigation. Appearance is handled by the carousel's Arranger.")]
        private NavigationDirection _inputDirection;

        [SerializeField, SrFoldout("Input")]
        [Tooltip("Decides when item carousel is focused and listens for input.")]
        private FocusBehavior _focusMode;

        [SerializeField, SrFoldout("Input")]
        [Tooltip("Decides what happens when the item carousel is selecting its first or last item and receives input " +
                 "to go out of bounds.")]
        private ClampBehavior _clampMode;

        [SerializeField, SrFoldout("Items")]
        [Tooltip("The component that will handle the position and orientation of its items.")]
        private SrLegacyItemCarouselArranger _arranger;

        [SerializeField, Space, SrFoldout("Items")]
        private bool _setItemsToChildren;

        [SerializeField, SrFoldout("Items")]
        private List<GameObject> _items;

        [SerializeField, SrFoldout("Events")]
        private SelectionChangedEvent _onSelectionChange;

        [SerializeField, SrFoldout("Events")]
        private UnityEvent _onSelectNext;

        [SerializeField, SrFoldout("Events")]
        private UnityEvent _onSelectPrevious;

        [SerializeField, SrFoldout("Events")]
        private UnityEvent _onFocusEnter;
        
        [SerializeField, SrFoldout("Events")]
        private UnityEvent _onFocusExit;

        [SerializeField, SrFoldout("Events")]
        private UnityEvent _onItemsChanged;

        private bool _isFocused;

        private Selectable _selectable;
        
        private Navigation _trueNavigation;

        private EventSystem _eventSystem;
        private EventTrigger _eventTrigger;

        private int _selectedIndex;

        private Coroutine _stateCoroutine;
        private Coroutine _focusExitCoroutine;
        private Coroutine _selectItemCoroutine;

        private EventTrigger.TriggerEvent _selected;
        private EventTrigger.TriggerEvent _deselected;
        private EventTrigger.TriggerEvent _submitted;
        private EventTrigger.TriggerEvent _canceled;
        #endregion

        #region Public Functions
        /// <summary>
        /// Returns the item in the carousel at the given index. Use <see cref="ItemCount"/> when looping through
        /// the items.
        /// </summary>
        public GameObject Get(int index)
        {
            return _items[index];
        }

        /// <summary>
        /// Returns the item in the carousel at the given index. Same as <see cref="Get"/>. Use <see cref="ItemCount"/>
        /// when looping through the items.
        /// </summary>
        public GameObject this[int index] { get { return Get(index); } }

        /// <summary>
        /// Adds the given item to the end of the carousel.
        /// </summary>
        public void Add(GameObject item)
        {
            _items.Add(item);

            ArrangeItem(item, ItemCount - 1);

            if (ItemCount == 1)
                SelectFirst();

            OnItemsChanged.Invoke();
        }

        /// <summary>
        /// Removes the item at the given index.
        /// </summary>
        public void RemoveAt(int itemIndex)
        {
            RemoveAt(itemIndex, true);
        }

        /// <summary>
        /// Removes the item at the given index.
        /// </summary>
        /// <param name="itemIndex"></param>
        /// <param name="destroyItem">If true, the removed item will also be destroyed in the scene.</param>
        public void RemoveAt(int itemIndex, bool destroyItem)
        {
            if (itemIndex == ItemCount)
                throw new InvalidOperationException("Can't call RemoveAt because there are no items in the carousel.");

            if (itemIndex < 0 || itemIndex >= ItemCount)
                throw new IndexOutOfRangeException(string.Format("itemIndex must be between 0 and ItemCount ({0}).",
                    ItemCount));

            var item = Get(itemIndex);
            _items.RemoveAt(itemIndex);

            if (SelectedIndex >= ItemCount)
                Select(ItemCount - 1);

            RearrangeAll();

            if (destroyItem)
                Destroy(item.gameObject);

            OnItemsChanged.Invoke();
        }

        /// <summary>
        /// Clears the carousel of all items.
        /// </summary>
        public void Clear()
        {
            Clear(true);
        }

        /// <summary>
        /// Clears the carousel of all items.
        /// </summary>
        /// <param name="destroyItems">If true, all of the carousel's items will also be destoyed in the scene.</param>
        public void Clear(bool destroyItems)
        {
            for (var i = ItemCount - 1; i >= 0; --i)
            {
                var item = Get(i);
                _items.RemoveAt(i);

                if (destroyItems)
                    Destroy(item.gameObject);
            }

            _selectedIndex = 0;

            RearrangeAll();

            OnItemsChanged.Invoke();
        }

        /// <summary>
        /// If the given item is in the carousel, selects it. Otherwise does nothing.
        /// </summary>
        public void Select(GameObject item)
        {
            var index = _items.IndexOf(item);
            if (index >= 0)
            {
                Select(index);
            }
        }

        /// <summary>
        /// Selects the item in the carousel at the given index.
        /// </summary>
        public void Select(int itemIndex)
        {
            if (itemIndex == ItemCount)
                throw new InvalidOperationException("Can't call Select because there are no items in the carousel.");

            if (itemIndex < 0 || itemIndex >= ItemCount)
                throw new IndexOutOfRangeException(string.Format("itemIndex must be between 0 and ItemCount ({0}).",
                    ItemCount));

            if (_selectItemCoroutine != null)
                StopCoroutine(_selectItemCoroutine);

            var old = SelectedIndex;
            _selectedIndex = itemIndex;

            _selectItemCoroutine = StartCoroutine(Coroutine_SelectItem(old, itemIndex));

            OnSelectionChange.Invoke(new SelectionChangedEvent.Args(old, Get(old), itemIndex, Get(itemIndex)));
        }

        /// <summary>
        /// Selects the next item in the carousel. If the last item is currently selected, behavior is determined by 
        /// <see cref="ClampMode"/>.
        /// </summary>
        public void SelectNext()
        {
            if (ItemCount == 0)
                return;

            if (SelectedIndex == ItemCount - 1)
            {
                HandleClampBehavior(ClampMode, Direction.Next);
            }
            else
            {
                Select(SelectedIndex + 1);
                OnSelectNext.Invoke();
            }
        }

        /// <summary>
        /// Selects the previous item in the carousel. If the first item is currently selected, behavior is determined by 
        /// <see cref="ClampMode"/>
        /// </summary>
        public void SelectPrevious()
        {
            if (ItemCount == 0)
                return;

            if (SelectedIndex == 0)
            {
                HandleClampBehavior(ClampMode, Direction.Previous);
            }
            else
            {
                Select(SelectedIndex - 1);
                OnSelectPrevious.Invoke();
            }
        }

        /// <summary>
        /// Selects the first item in the carousel.
        /// </summary>
        public void SelectFirst()
        {
            if (ItemCount > 0)
                Select(0);
        }

        /// <summary>
        /// Selects the last item in the carousel.
        /// </summary>
        public void SelectLast()
        {
            if (ItemCount > 0)
                Select(ItemCount - 1);
        }

        /// <summary>
        /// Selects a random item in the carousel.
        /// </summary>
        public void SelectRandom()
        {
            if (ItemCount > 0)
                Select(UnityEngine.Random.Range(0, ItemCount));
        }

        /// <summary>
        /// Starts changing the carousel's selection in response to player input.
        /// </summary>
        public void EnterFocus()
        {
            if (IsFocused)
                return;

            _isFocused = true;

            // Copy the selectable's navigation in case clamp mode is Exit, we'll be making changes to it
            // to prevent navigation events from messing with the carousel's selection
            _trueNavigation = _selectable.navigation;

            _eventSystem.SetSelectedGameObject(gameObject);

            SetStateCoroutine(Coroutine_CheckFocusedInput);
            SetFocusExitCoroutine(Coroutine_CheckDeselection);

            OnFocusEnter.Invoke();
        }

        /// <summary>
        /// Stops changing the carousel's selection in response to player input.
        /// </summary>
        public void ExitFocus()
        {
            if (!IsFocused)
                return;

            _isFocused = false;

            _selectable.navigation = _trueNavigation;

            SetStateCoroutine(Coroutine_AwaitFocus);
            SetFocusExitCoroutine(null);

            OnFocusExit.Invoke();
        }
        #endregion

        #region Nonpublic Functions
        protected void ArrangeItem(GameObject item, int index)
        {
            if (_arranger)
                _arranger.PlaceItem(item, index);
        }

        protected void RearrangeAll()
        {
            if (!_arranger)
                return;

            for (var i = 0; i < ItemCount; ++i)
                _arranger.PlaceItem(this[i], i);
        }

        protected void CollectChildrenAsItems()
        {
            _items.Clear();

            for (var i = 0; i < transform.childCount; ++i)
            {
                _items.Add(transform.GetChild(i).gameObject);
            }
        }

        protected void HandleClampBehavior(ClampBehavior clampBehavior, Direction direction)
        {
            // Do nothing if clamp behavior is Constrain

            if (clampBehavior == ClampBehavior.Loop)
            {
                if (ItemCount < 2)
                    return;

                if (direction == Direction.Next)
                {
                    SelectFirst();
                    OnSelectNext.Invoke();
                }
                else if (direction == Direction.Previous)
                {
                    SelectLast();
                    OnSelectPrevious.Invoke();
                }
            }
            else if (clampBehavior == ClampBehavior.Exit && FocusMode != FocusBehavior.Always)
            {
                ExitFocus();

                var adjacent = GetAdjacentSelectable(InputDirection, direction);
                if (adjacent)
                    _eventSystem.SetSelectedGameObject(adjacent.gameObject);
            }
        }

        protected Selectable GetAdjacentSelectable(NavigationDirection navigationDirection, Direction selectionDirection)
        {
            if (navigationDirection == NavigationDirection.Vertical)
            {
                if(selectionDirection == Direction.Next)
                    return IsFocused ? _trueNavigation.selectOnUp : _selectable.FindSelectableOnUp();

                if (selectionDirection == Direction.Previous)
                    return IsFocused ? _trueNavigation.selectOnDown : _selectable.FindSelectableOnDown();
            }

            if (navigationDirection == NavigationDirection.Horizontal)
            {
                if (selectionDirection == Direction.Next)
                    return IsFocused ? _trueNavigation.selectOnRight : _selectable.FindSelectableOnRight();

                if (selectionDirection == Direction.Previous)
                    return IsFocused ? _trueNavigation.selectOnLeft : _selectable.FindSelectableOnLeft();
            }

            return null;
        }

        protected int GetAxisInput()
        {
            var input = Input.GetAxis(_inputAxis);

            if (InvertAxis)
                input *= -1;

            if (input == 0)
                return 0;

            if (input > 0)
                return 1;

            return -1;
        }
        
        private void SetStateCoroutine(Func<IEnumerator> newStateCoroutine)
        {
            if (!gameObject.activeInHierarchy)
                return;

            if (_stateCoroutine != null)
                StopCoroutine(_stateCoroutine);

            _stateCoroutine = null;

            if (newStateCoroutine != null)
                _stateCoroutine = StartCoroutine(newStateCoroutine());
        }

        private void SetFocusExitCoroutine(Func<IEnumerator> newCoroutine)
        {
            if (!gameObject.activeInHierarchy)
                return;

            if (_focusExitCoroutine != null)
                StopCoroutine(_focusExitCoroutine);

            _focusExitCoroutine = null;

            if (newCoroutine != null)
                _focusExitCoroutine = StartCoroutine(newCoroutine());
        }
        #endregion

        #region Lifecycle Functions
        protected void Reset()
        {
            _arranger = GetComponentInChildren<SrLegacyItemCarouselArranger>();
            _inputAxis = "Horizontal";

            _setItemsToChildren = true;
        }

        protected void Awake()
        {
            _selectable = _selectable ?? GetComponent<Selectable>();
            _eventSystem = _eventSystem ?? FindObjectOfType<EventSystem>();
            _eventTrigger = _eventTrigger ?? GetComponent<EventTrigger>();

            _onSelectionChange = _onSelectionChange ?? new SelectionChangedEvent();
            _onSelectNext = _onSelectNext ?? new UnityEvent();
            _onSelectPrevious = _onSelectPrevious ?? new UnityEvent();
            _onFocusEnter = _onFocusEnter ?? new UnityEvent();
            _onFocusExit = _onFocusExit ?? new UnityEvent();
            _onItemsChanged = _onItemsChanged ?? new UnityEvent();

            _selected = new EventTrigger.TriggerEvent();
            _deselected = new EventTrigger.TriggerEvent();
            _submitted = new EventTrigger.TriggerEvent();
            _canceled = new EventTrigger.TriggerEvent();
            
            _eventTrigger.triggers.Clear();
            _eventTrigger.triggers.Add(new EventTrigger.Entry {callback = _selected, eventID = EventTriggerType.Select});
            _eventTrigger.triggers.Add(new EventTrigger.Entry {callback = _deselected, eventID = EventTriggerType.Deselect});
            _eventTrigger.triggers.Add(new EventTrigger.Entry {callback = _submitted, eventID = EventTriggerType.Submit});
            _eventTrigger.triggers.Add(new EventTrigger.Entry {callback = _canceled, eventID = EventTriggerType.Cancel});
            
            _items = _items ?? new List<GameObject>();

            if (_arranger)
            {
                _arranger.Carousel = this;
            }
            else
            {
                if (Application.isPlaying)
                    Debug.LogError(string.Format("Item Carousel '{0}' needs an Arranger to handle placement of its " +
                                                 "items. Try using a 'Blink Item Carousel Arranger' for the least " +
                                                 "amount of setup.", name));
            }

            if (_setItemsToChildren)
                CollectChildrenAsItems();

            for (var i = 0; i < _items.Count; ++i)
                ArrangeItem(_items[i], i);

#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
        }

#if UNITY_EDITOR
        protected void Update()
        {
            if (Application.isPlaying)
                return;

            if (_setItemsToChildren)
                CollectChildrenAsItems();

            if (_arranger)
            {
                _arranger.Carousel = this;
                RearrangeAll();
            }
        }
#endif

        protected void OnTransformChildrenChanged()
        {
            if (_setItemsToChildren)
            {
                CollectChildrenAsItems();

                if (_arranger)
                    RearrangeAll();
            }
        }

        protected void OnEnable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif

            SetStateCoroutine(Coroutine_AwaitFocus);
        }

        protected void OnDisable()
        {
            if (IsFocused)
                ExitFocus();
        }

        #endregion

        #region Coroutines & Event Handlers

        private IEnumerator Coroutine_AwaitFocus()
        {
            if (FocusMode == FocusBehavior.Always)
            {
                EnterFocus();
            }
            else if (FocusMode == FocusBehavior.MustBeSelected)
            {
                if (!IsFocused)
                    yield return new SrWaitForUnityEvent<BaseEventData>(_selected);

                EnterFocus();
            }
            else if (FocusMode == FocusBehavior.MustBeSubmitted)
            {
                if (!IsFocused)
                    yield return new SrWaitForUnityEvent<BaseEventData>(_submitted);

                EnterFocus();
            }
        }

        private IEnumerator Coroutine_CheckDeselection()
        {
            // Do nothing if FocusMode is Always

            if (FocusMode == FocusBehavior.MustBeSelected || FocusMode == FocusBehavior.MustBeSubmitted)
            {
                if (FocusMode == FocusBehavior.MustBeSelected)
                {
                    Navigation navigation;

                    if (InputDirection == NavigationDirection.Horizontal)
                    {
                        navigation = new Navigation
                        {
                            mode = Navigation.Mode.Explicit,
                            selectOnUp = _selectable.FindSelectableOnUp(),
                            selectOnDown = _selectable.FindSelectableOnDown(),
                            selectOnLeft = null,
                            selectOnRight = null,
                        };
                    }
                    else
                    {
                        navigation = new Navigation
                        {
                            mode = Navigation.Mode.Explicit,
                            selectOnUp = null,
                            selectOnDown = null,
                            selectOnLeft = _selectable.FindSelectableOnLeft(),
                            selectOnRight = _selectable.FindSelectableOnRight(),
                        };
                    }

                    _selectable.navigation = navigation;

                    yield return new SrWaitForUnityEvent<BaseEventData>(_deselected);
                }
                else if (FocusMode == FocusBehavior.MustBeSubmitted)
                {
                    var navigation = new Navigation
                    {
                        mode = Navigation.Mode.Explicit,
                        selectOnDown = null,
                        selectOnUp = null,
                        selectOnLeft = null,
                        selectOnRight = null
                    };

                    _selectable.navigation = navigation;
                    
                    yield return new SrWaitForAny(true,
                        new SrWaitForUnityEvent<BaseEventData>(_canceled),
                        new SrWaitForUnityEvent<BaseEventData>(_deselected));
                }

                ExitFocus();
            }
        }

        private IEnumerator Coroutine_CheckFocusedInput()
        {
            var previousInput = GetAxisInput();

            // Wait one Update to reset input
            yield return null;

            while (true)
            {
                var input = GetAxisInput();
                if (input != previousInput)
                {
                    if (previousInput == 0)
                    {
                        if (input == 1)
                            SelectNext();
                        else if (input == -1)
                            SelectPrevious();
                    }

                    previousInput = input;
                }

                yield return null;
            }
        }

        private IEnumerator Coroutine_SelectItem(int oldIndex, int newIndex)
        {
            yield return _arranger.ChangeSelection(oldIndex, newIndex);
        }
        #endregion
    }
}
