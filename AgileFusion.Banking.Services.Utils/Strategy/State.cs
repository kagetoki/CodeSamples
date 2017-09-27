using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileFusion.Banking.Services.Utils.Strategy
{
    public class State<TCondition, TResult> : IState<TCondition, TResult>
    {
        public Func<TCondition, int> NextIdSelector { get; set; }
        public int Id { get; set; }
        public TResult Value { get; set; }

        public StateCondition<TCondition> Conditions { get; set; }

        public State() {}

        public State(StateCondition<TCondition> conditions, TResult result = default(TResult))
        {
            Conditions = conditions;
            Value = result;
        }

        public bool IsMatch(TCondition conditions, out int nextId)
        {
            if (NextIdSelector != null)
            {
                nextId = NextIdSelector(conditions);
            }
            else
            {
                nextId = Id + 1;
            }
            return Conditions.IsCarriedOut(conditions);
        }
        
        public static StateCollection Chain(Func<TCondition, bool> predicate, TResult result)
        {
            var collection = new StateCollection();
            collection.Append(predicate, result);
            return collection;
        }
        public class StateCollection : Queue<State<TCondition, TResult>>
        {
            private int _currentId;
            public StateCollection Append(Func<TCondition, bool> predicate, TResult result)
            {
                State<TCondition, TResult> state = new State<TCondition, TResult>(predicate, result);
                state.Id = _currentId++;
                base.Enqueue(state);
                return this;
            }
        }
    }
}
