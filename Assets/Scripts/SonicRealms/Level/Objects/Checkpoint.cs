using System;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Internal;
using UnityEngine;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine.Events;

namespace SonicRealms.Level.Objects
{
    [Serializable]
    public class CheckpointEvent : UnityEvent<Checkpoint>
    {

    }

    public class Checkpoint : ReactiveEffect
    {
        [SerializeField]
        private string _id;

        [SerializeField, SrFoldout("Animation")]
        private Animator _animator;

        [SerializeField, SrFoldout("Animation")]
        [Tooltip("Name of an Animator trigger to set when the checkpoint is activated.")]
        private string _activatedTrigger;

        private int _activatedTriggerHash;

        [SerializeField, SrFoldout("Animation")]
        [Tooltip("Name of an Animator trigger to set when the checkpoint is pre-activated. " +
                 "This might happen when a player resumes a game at a checkpoint.")]
        private string _preActivatedTrigger;
        private int _preActivatedTriggerHash;

        [SerializeField, SrFoldout("Events")]
        private CheckpointEvent _onCheckpointActivate;

        [SerializeField, SrFoldout("Events")]
        private CheckpointEvent _onCheckpointPreActivate;

        private bool _isCheckpointActive;

        public string Id { get { return _id; } }

        public string ActivatedTrigger { get { return _activatedTrigger; } }

        public string PreActivatedTrigger { get { return _preActivatedTrigger; } }

        public CheckpointEvent OnCheckpointActivate { get { return _onCheckpointActivate; } }

        public CheckpointEvent OnCheckpointPreActivate { get { return _onCheckpointPreActivate; } }

        public bool IsCheckpointActive { get { return _isCheckpointActive; } }

        public override void Reset()
        {
            base.Reset();

            _animator = GetComponent<Animator>();
        }

        public override void Awake()
        {
            base.Awake();

            _onCheckpointActivate = _onCheckpointActivate ?? new CheckpointEvent();
            _onCheckpointPreActivate = _onCheckpointPreActivate ?? new CheckpointEvent();

            _animator = _animator ?? GetComponent<Animator>();
            _activatedTriggerHash = Animator.StringToHash(_activatedTrigger);
            _preActivatedTriggerHash = Animator.StringToHash(_preActivatedTrigger);
        }

        public void PreActivate()
        {
            if (_isCheckpointActive)
                return;

            if (_animator && _preActivatedTriggerHash != 0)
                _animator.SetTrigger(_preActivatedTriggerHash);

            OnCheckpointPreActivate.Invoke(this);
        }


        public override void OnActivate(HedgehogController controller)
        {
            if (_isCheckpointActive)
                return;

            if (_animator && _activatedTriggerHash != 0)
                _animator.SetTrigger(_activatedTriggerHash);

            OnCheckpointActivate.Invoke(this);
        }
    }
}
