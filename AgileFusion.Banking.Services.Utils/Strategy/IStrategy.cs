using System.Collections.Generic;

namespace AgileFusion.Banking.Services.Utils.Strategy
{
    public interface IStrategy<TCondition>
    {
        IState<TCondition> CurrentState { get; }
        IState<TCondition> Execute(TCondition condition);
        void ResetState();
    }

    public interface IStrategy<TCondition, TValue> : IStrategy<TCondition>
    {
        IState<TCondition, TValue> CurrentState { get; }
        IState<TCondition, TValue> Execute(TCondition condition);
    }
}
