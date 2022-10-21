using System;
using System.Collections.Generic;
using System.Net.Mail;
using UnityEngine;
using UnityEngine.Events;

namespace TonyDev.Game.Core.Entities.Enemies.Movement
{
    public enum TimelineType //Types of things that timeline can control
    {
        Enemy, Projectile, All, None
    }
    
    public enum TimelineEntryType //Enums representing executable timeline elements
    {
        Teleport, Linear, Sin, Arc, Destroy, Pause, Animation, SpawnPrefab, Attack, Empty
    }

    #region TimelineEntries
    
    [Serializable]
    public class TimelineEntry
    {
        public static readonly Dictionary<TimelineEntryType, Type> EntryTypeDictionary = new()
        {
            {TimelineEntryType.Empty, typeof(TimelineEntry)},
            {TimelineEntryType.Teleport, typeof(TeleportEntry)},
            {TimelineEntryType.Pause, typeof(PauseEntry)}
        };
        
        public virtual TimelineEntryType TimelineEntryType => TimelineEntryType.Empty;
        public virtual TimelineType[] ValidTypes => new [] {TimelineType.None};

        protected UnityEvent FinishEvent = new();
        
        /* Called when the timeline entry is reached in the sequence.
         * return function lets the timeline manager know when the action is finished */
        public virtual void Execute(GameObject receiver, Action finishCallback)
        {
            FinishEvent.AddListener(finishCallback.Invoke);
        }

        public virtual void Update(GameObject receiver)
        {
            
        }
    }
    
    public class TeleportEntry : TimelineEntry
    {
        public Vector2 TeleportVector;

        public override void Execute(GameObject receiver, Action finishCallback)
        {
            base.Execute(receiver, finishCallback);
            receiver.transform.Translate(TeleportVector);
            FinishEvent?.Invoke();
        }
    }
    
    public class PauseEntry : TimelineEntry
    {
        public override TimelineEntryType TimelineEntryType => TimelineEntryType.Pause;
        public override TimelineType[] ValidTypes => new[] {TimelineType.All};

        public float PauseTimeSeconds;

        private float _finishTime;
        
        public override void Execute(GameObject receiver, Action finishCallback)
        {
            base.Execute(receiver, finishCallback);
            _finishTime = Time.time + PauseTimeSeconds;
        }

        public override void Update(GameObject receiver)
        {
            if(Time.time > _finishTime) FinishEvent?.Invoke();
        }
    }
    
    #endregion
    
    [CreateAssetMenu(menuName = "Behavior Timeline")] //Add entry to Unity's Create Asset menu
    public class BehaviorTimelineData : ScriptableObject
    {
        /*  Holds an array of timeline "instructions" that will execute sequentially.
         *  Data will be stored here, execution will be done in a MonoBehaviour */
        
        [SerializeReference] public List<TimelineEntry> timelineEntries = new();
    }
}
