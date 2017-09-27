using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileFusion.Banking.Services.Utils.Strategy
{
    public class AndConditions<T> : ICondition<T>
    {
        public AndConditions()
        {
            Conditions = new List<ICondition<T>>();
        }
        public List<ICondition<T>> Conditions { get; protected set; }

        public bool IsCarriedOut(T input)
        {
            foreach(var condition in Conditions)
            {
                if (!condition.IsCarriedOut(input))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
