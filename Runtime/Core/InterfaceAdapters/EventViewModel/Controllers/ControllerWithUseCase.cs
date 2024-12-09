namespace MVVM.Core.InterfaceAdapters
{
    public abstract class ControllerWithUseCase<TUseCase> : Controller
    {
        private readonly TUseCase _useCase;

        protected ControllerWithUseCase(IEventViewModel eventViewModel, TUseCase useCase) : base(eventViewModel)
        {
            _useCase = useCase;
        }

        public override void Execute()
        {
            ExecuteUseCase(_useCase);
        }

        protected abstract void ExecuteUseCase(TUseCase useCase);
    }
}