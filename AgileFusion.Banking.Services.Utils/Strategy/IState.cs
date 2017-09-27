using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileFusion.Banking.Services.Utils.Strategy
{
    public interface IState<TCondition>
    {
        int Id { get; set; }
        bool IsMatch(TCondition conditions, out int nextId);
    }

    public interface IState<TCondition, TValue> : IState<TCondition>
    {
        TValue Value { get; }
    }

    public interface IActionState<TCondition> : IState<TCondition>
    {
        Action<TCondition> Action { get; }
    }
}
