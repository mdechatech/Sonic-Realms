using System.Collections.Generic;
using System.Linq;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Hook up to these events to react when a controller enters the area.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Hedgehog/Triggers/Area Trigger")]
    public class AreaTrigger : BaseTrigger
    {
        /// <summary>
        /// Invoked when a controller enters the area.
        /// </summary>
        [Foldout("Events")]
        public AreaTriggerEvent OnAreaEnter;

        /// <summary>
        /// Invoked when a controller stays in the area.
        /// </summary>
        [Foldout("Events")]
        public AreaTriggerEvent OnAreaStay;

        /// <summary>
        /// Invoked when a controller exits the area.
        /// </summary>
        [Foldout("Events")]
        public AreaTriggerEvent OnAreaExit;

        /// <summary>
        /// A list of controllers currently in the area trigger.
        /// </summary>
        [Foldout("Debug")]
        [Tooltip("A list of controllers currently in the area trigger.")]
        public List<HedgehogController> ControllersInArea;

        /// <summary>
        /// Defines whether the trigger should consider the given contact as inside the area.
        /// </summary>
        public delegate bool TouchPredicate(AreaCollision.Contact contact);

        /// <summary>
        /// A list of predicates which, if empty or all return true, allow the area trigger to touch the given
        /// hitbox and invoke events for it.
        /// </summary>
        public List<TouchPredicate> TouchRules;

        protected List<AreaTrigger> Parents;

        public Dictionary<HedgehogController, List<AreaCollision.Contact>> CurrentContacts;
        private List<AreaCollision.Contact> _possibleContacts;

        public override void Awake()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
            base.Awake();

            OnAreaEnter = OnAreaEnter ?? new AreaTriggerEvent();
            OnAreaStay = OnAreaStay ?? new AreaTriggerEvent();
            OnAreaExit = OnAreaExit ?? new AreaTriggerEvent();
            
            ControllersInArea = new List<HedgehogController>();
            TouchRules = new List<TouchPredicate>();

            Parents = new List<AreaTrigger>();
            GetComponentsInParent(true, Parents);

            for (var i = 0; i < Parents.Count; ++i)
            {
                if (Parents[i] == this)
                {
                    Parents.RemoveAt(i);
                    break;
                }
            }

            CurrentContacts = new Dictionary<HedgehogController, List<AreaCollision.Contact>>();
            _possibleContacts = new List<AreaCollision.Contact>();
        }

        public virtual void Start()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
            if (!TriggerFromChildren)
                return;

            foreach (var child in GetComponentsInChildren<Collider2D>())
            {
                if (child.transform == transform ||
                    child.GetComponent<PlatformTrigger>() ||
                    child.GetComponent<AreaTrigger>())
                    continue;

                child.gameObject.AddComponent<AreaTrigger>().TriggerFromChildren = true;
            }
        }

        public void FixedUpdate()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
            HandleCurrentContacts();
            HandlePossibleContacts();
        }

        public override bool IsAlone
        {
            get
            {
                return !GetComponent<ReactiveArea>() &&
                       GetComponentsInParent<AreaTrigger>().All(t => t == this || !t.TriggerFromChildren) &&
                       OnAreaEnter.GetPersistentEventCount() == 0 &&
                       OnAreaStay.GetPersistentEventCount() == 0 &&
                       OnAreaExit.GetPersistentEventCount() == 0;
            }
        }

        public override bool HasController(HedgehogController controller)
        {
            return ControllersInArea.Contains(controller);
        }

        /// <summary>
        /// Returns whether the specified controller collides with the collider.
        /// </summary>
        /// <param name="controller">The specified controller.</param>
        /// <returns></returns>
        public virtual bool CanTouch(AreaCollision.Contact contact)
        {
            for (var i = 0; i < TouchRules.Count; ++i)
            {
                if (!TouchRules[i](contact))
                    return false;
            }

            for (var i = 0; i < Parents.Count; ++i)
            {
                var parent = Parents[i];
                if (!parent.TriggerFromChildren)
                    continue;

                for (var j = 0; j < parent.TouchRules.Count; ++j)
                {
                    if (!parent.TouchRules[j](contact))
                        return false;
                }
            }

            return true;
        }

        public bool DefaultInsideRule(AreaCollision.Contact contact)
        {
            return contact.Hitbox.CompareTag(Hitbox.MainHitboxTag);
        }

        protected void NotifyContactEnter(AreaCollision.Contact contact, bool bubble = true)
        {
            bool isFirst;
            AddCurrentContact(contact, out isFirst);

            if (isFirst)
            {
                ControllersInArea.Add(contact.Controller);
                OnAreaEnter.Invoke(new AreaCollision(new[] {contact}));
            }

            if(bubble)
                BubbleContactEnter(contact);
        }

        protected void BubbleContactEnter(AreaCollision.Contact contact)
        {
            for (var i = 0; i < Parents.Count; ++i)
            {
                var parent = Parents[i];
                if (parent.TriggerFromChildren)
                {
                    parent.NotifyContactEnter(contact, false);
                }
            }
        }

        protected void NotifyContactExit(AreaCollision.Contact contact, bool bubble = true)
        {
            bool wasLast;
            if (RemoveCurrentContact(contact, out wasLast) && wasLast)
            {
                ControllersInArea.Remove(contact.Controller);
                OnAreaExit.Invoke(new AreaCollision(new[] {contact}));
            }

            if (bubble)
                BubbleContactExit(contact);
        }

        protected void BubbleContactExit(AreaCollision.Contact contact)
        {
            for (var i = 0; i < Parents.Count; ++i)
            {
                var parent = Parents[i];
                if (parent.TriggerFromChildren)
                {
                    parent.NotifyContactExit(contact, false);
                }
            }
        }

        protected void AddCurrentContact(AreaCollision.Contact contact, out bool isFirst)
        {
            List<AreaCollision.Contact> contactList;
            if (CurrentContacts.TryGetValue(contact.Controller, out contactList))
            {   
                for (var i = contactList.Count - 1; i >= 0; --i)
                {
                    if (contactList[i].Hitbox == contact.Hitbox)
                    {
                        contactList.RemoveAt(i);
                    }
                }

                contactList.Add(contact);
            }
            else
            {
                CurrentContacts.Add(contact.Controller, contactList = new List<AreaCollision.Contact> {contact});
            }

            isFirst = contactList.Count == 1;
        }

        protected bool RemoveCurrentContact(AreaCollision.Contact contact, out bool wasLast)
        {
            List<AreaCollision.Contact> contactList;

            if (!CurrentContacts.TryGetValue(contact.Controller, out contactList))
            {
                wasLast = false;
                return false;
            }

            for (var i = 0; i < contactList.Count; ++i)
            {
                var item = contactList[i];
                if (item.Hitbox == contact.Hitbox)
                {
                    contactList.RemoveAt(i);
                    wasLast = contactList.Count == 0;
                    return true;
                }
            }

            wasLast = false;
            return false;
        }

        protected void AddPossibleContact(AreaCollision.Contact contact)
        {
            _possibleContacts.Add(contact);
        }

        protected bool RemovePossibleContact(AreaCollision.Contact contact)
        {
            for (var i = 0; i < _possibleContacts.Count; ++i)
            {
                var possibleContact = _possibleContacts[i];
                if (possibleContact.Hitbox == contact.Hitbox)
                {
                    _possibleContacts.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        protected void OnTriggerEnter2D(Collider2D collider2D)
        {
            var hitbox = collider2D.GetComponent<Hitbox>();
            if (!hitbox)
                return;

            var contact = new AreaCollision.Contact(this, hitbox);

            if (hitbox.CanTouch(contact) && CanTouch(contact))
            {
                NotifyContactEnter(contact);
            }
            else
            {
                AddPossibleContact(contact);
            }
        }

        protected void OnTriggerExit2D(Collider2D collider2D)
        {
            var hitbox = collider2D.GetComponent<Hitbox>();
            if (!hitbox)
                return;
            
            var contact = new AreaCollision.Contact(this, hitbox);

            // Remove from current and possible
            if (!RemovePossibleContact(contact))
            {
                NotifyContactExit(contact);
            }
        }

        private void HandlePossibleContacts()
        {
            for (var i = _possibleContacts.Count - 1; i >= 0; --i)
            {
                var possibleContact = _possibleContacts[i];

                var updatedContact = new AreaCollision.Contact(possibleContact.AreaTrigger, possibleContact.Hitbox);
                _possibleContacts[i] = updatedContact;

                if (CanTouch(updatedContact) && possibleContact.Hitbox.CanTouch(updatedContact))
                {
                    _possibleContacts.RemoveAt(i);
                    NotifyContactEnter(updatedContact);
                }
            }
        }

        private void HandleCurrentContacts()
        {
            for (var i = ControllersInArea.Count - 1; i >= 0; --i)
            {
                var contacts = CurrentContacts[ControllersInArea[i]];

                for (var j = contacts.Count - 1; j >= 0; --j)
                {
                    var contact = contacts[j];
                    var updatedContact = new AreaCollision.Contact(contact.AreaTrigger, contact.Hitbox);

                    if (CanTouch(updatedContact) && contact.Hitbox.CanTouch(updatedContact))
                    {
                        contacts[j] = updatedContact;
                    }
                    else if(contact.AreaTrigger == this)
                    {
                        AddPossibleContact(updatedContact);
                        NotifyContactExit(contact);
                    }
                }

                if (contacts.Count > 0)
                {
                    OnAreaStay.Invoke(new AreaCollision(contacts));
                }
            }
        }
    }
}
