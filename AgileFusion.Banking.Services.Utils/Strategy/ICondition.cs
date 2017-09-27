using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileFusion.Banking.Services.Utils.Strategy
{
    public interface ICondition<T>
    {
        bool IsCarriedOut(T input);
    }
}
