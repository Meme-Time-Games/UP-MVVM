namespace MVVM.Core.InterfaceAdapters
{
    public abstract class ControllerExecuteAfterFrameWithUseCase<TUseCase> : Controller
    {
        private readonly TUseCase _useCase;
        private readonly IAfterFrameExecutor _afterFrameExecutor;

        protected ControllerExecuteAfterFrameWithUseCase(IEventViewModel eventViewModel, TUseCase useCase, IAfterFrameExecutor afterFrameExecutor) : base(eventViewModel)
        {
            _useCase = useCase;
            _afterFrameExecutor = afterFrameExecutor;
        }

        public override void Execute()
        {
            _afterFrameExecutor.OnExecute += ExecuteAfterTime;
            _afterFrameExecutor.Execute();
        }

        public void ExecuteAfterTime()
        {
            _afterFrameExecutor.OnExecute -= ExecuteAfterTime;
            ExecuteUseCase(_useCase);
        }

        protected abstract void ExecuteUseCase(TUseCase useCase);
    }
}