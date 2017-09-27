using System;

namespace AgileFusion.Banking.Services.Utils.Strategy
{
    public class Condition<T> : ICondition<T>
    {
        private readonly Func<T, bool> _predicate;
        public Condition(Func<T, bool> condition)
        {
            if(condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }
            _predicate = condition;
        }
        public bool IsCarriedOut(T input)
        {
            return _predicate(input);
        }
    }
}
