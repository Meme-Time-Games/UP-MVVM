namespace MVVM.Core.InterfaceAdapters
{
    public abstract class ReactiveVariableControllerWithUseCase<TType, TUseCase> : ReactiveVariableController<TType>
    {
        private readonly TUseCase _useCase;

        protected ReactiveVariableControllerWithUseCase(IReactiveVariable<TType> reactiveVariable) : base(reactiveVariable)
        {
        }

        protected override void ExecuteWithValue(TType value)
        {
            ExecuteUseCase(_useCase, value);
        }

        protected abstract void ExecuteUseCase(TUseCase useCase, TType value);
    }
}