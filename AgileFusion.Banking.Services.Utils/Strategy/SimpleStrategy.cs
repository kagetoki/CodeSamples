using System;
using System.Collections.Generic;
using System.Linq;

namespace AgileFusion.Banking.Services.Utils.Strategy
{
    public class SimpleStrategy<TCondition> : IStrategy<TCondition>
    {
        private IDictionary<int, IState<TCondition>> _states;
        public IState<TCondition> CurrentState { get; private set; }
        private IState<TCondition> _originalState { get; set; }

        public SimpleStrategy(IEnumerable<IState<TCondition>> states):this(states, states.First())
        { }

        public SimpleStrategy(IEnumerable<IState<TCondition>> states, IState<TCondition> originalState)
        {
            if(states == null)
            {
                throw new ArgumentNullException(nameof(states));
            }
            _states = states.ToDictionary(s => s.Id);
            if (!_states.ContainsKey(originalState.Id))
            {
                _states.Add(originalState.Id, originalState);
            }
            _originalState = originalState;
            CurrentState = _originalState;
        }

        public IState<TCondition> Execute(TCondition condition)
        {
            int nextId;
            while(!CurrentState.IsMatch(condition, out nextId))
            {
                if (!_states.ContainsKey(nextId))
                {
                    ResetState();
                    return null;
                }
                CurrentState = _states[nextId];
            }
            return CurrentState;
        }

        public void ResetState()
        {
            CurrentState = _originalState;
        }
    }

    public class SimpleStrategy<TCondition, TValue> : IStrategy<TCondition, TValue>
    {

        private readonly IState<TCondition, TValue> _originalState;

        private IDictionary<int, IState<TCondition, TValue>> _states;
        public IState<TCondition, TValue> CurrentState { get; private set; }

        IState<TCondition> IStrategy<TCondition>.CurrentState{get{ return this.CurrentState; }}

        public SimpleStrategy(IEnumerable<IState<TCondition,TValue>> states) : this(states, states.First())
        { }

        public SimpleStrategy(IEnumerable<IState<TCondition, TValue>> states, IState<TCondition, TValue> originalState)
        {
            if (states == null)
            {
                throw new ArgumentNullException(nameof(states));
            }
            _states = states.ToDictionary(s => s.Id);
            if (!_states.ContainsKey(originalState.Id))
            {
                _states.Add(originalState.Id, originalState);
            }
            _originalState = originalState;
            CurrentState = _originalState;
        }

        public IState<TCondition, TValue> Execute(TCondition condition)
        {
            int nextId;
            while (!CurrentState.IsMatch(condition, out nextId))
            {
                if (!_states.ContainsKey(nextId))
                {
                    ResetState();
                    return null;
                }
                CurrentState = _states[nextId];
            }
            return CurrentState;
        }

        public void ResetState()
        {
            CurrentState = _originalState;
        }

        IState<TCondition> IStrategy<TCondition>.Execute(TCondition condition)
        {
            return this.Execute(condition);
        }
    }
}
