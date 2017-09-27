using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileFusion.Banking.Services.Utils.Strategy
{
    public class StateCondition<T> : ICondition<T>
    {
        public OrConditions<T> OrConditions { get; private set; }
        public AndConditions<T> AndConditions { get; private set; }
        public StateCondition(Func<T, bool> condition)
        {
            OrConditions = new OrConditions<T>();
            AndConditions = new AndConditions<T>();
            AndConditions.Conditions.Add(new Condition<T>(condition));
            AndConditions.Conditions.Add(OrConditions);
        }

        public bool IsCarriedOut(T input)
        {
            return AndConditions.IsCarriedOut(input);
        }

        public static implicit operator StateCondition<T>(Func<T, bool> condition)
        {
            return new StateCondition<T>(condition);
        }
    }
}
