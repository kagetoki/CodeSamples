using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileFusion.Banking.Services.Utils.Strategy
{
    public class OrConditions<T> : ICondition<T>
    {
        public List<ICondition<T>> Conditions { get; protected set; }
        public OrConditions()
        {
            Conditions = new List<ICondition<T>>();
        }
        public bool IsCarriedOut(T input)
        {
            if(Conditions.Count == 0)
            {
                return true;
            }
            foreach(var condition in Conditions)
            {
                if (condition.IsCarriedOut(input))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
