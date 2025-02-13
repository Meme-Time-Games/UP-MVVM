namespace MVVM.Core.InterfaceAdapters
{
    public abstract class ReactiveVariableControllerWithUseCase<TType, TUseCase> : ReactiveVariableController<TType>
    {
        private readonly TUseCase _useCase;

        protected ReactiveVariableControllerWithUseCase(IReactiveVariable<TType> reactiveVariable, TUseCase useCase) : base(reactiveVariable)
        {
            _useCase = useCase;
        }

        protected override void ExecuteWithValue(TType value)
        {
            ExecuteUseCase(_useCase, value);
        }

        protected abstract void ExecuteUseCase(TUseCase useCase, TType value);
    }
}