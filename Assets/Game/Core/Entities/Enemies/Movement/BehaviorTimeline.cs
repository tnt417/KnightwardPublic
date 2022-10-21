using System;
using UnityEngine;

namespace TonyDev.Game.Core.Entities.Enemies.Movement
{
    public class BehaviorTimeline : MonoBehaviour
    {
        private BehaviorTimelineData _data;
        private BehaviorTimelineData _originalDataState;
        
        private int _entryIndex = 0;

        public void Set(BehaviorTimelineData newData)
        {
            _originalDataState = newData; //Store initial state of the SO
            _data = Instantiate(newData); //Instantiate a copy of the SO
        }

        private void Start()
        {
            _data.timelineEntries[_entryIndex].Execute(gameObject, NextEntry); //Execute the starting entry
        }

        private void NextEntry()
        {
            _entryIndex++; //Step the index of the active entry

            if (_entryIndex >= _data.timelineEntries.Count) Repeat(); //Repeat when reaching the end of the list

            _data.timelineEntries[_entryIndex].Execute(gameObject, NextEntry); //Execute the next entry
        }

        private void Repeat()
        {
            _entryIndex = 0; //Back to the start
            _data = Instantiate(_originalDataState); //Re-instantiate the original data to return values to entry values.
        }

        private void Update()
        {
            _data.timelineEntries[_entryIndex].Update(gameObject); //Call update functions
        }
    }
}
