using TimerRaiser;

namespace MVVM.Core.InterfaceAdapters
{
    public abstract class ControllerExecuteAfterFrameWithUseCase<TUseCase> : Controller
    {
        private readonly TUseCase _useCase;
        private readonly ITimer _afterFrameExecutor;
        private readonly float _time;

        protected ControllerExecuteAfterFrameWithUseCase(IEventViewModel eventViewModel, TUseCase useCase, ITimer afterFrameExecutor, float time)
            : base(eventViewModel)
        {
            _useCase = useCase;
            _afterFrameExecutor = afterFrameExecutor;
            _time = time;
        }

        public override void Execute()
        {
            _afterFrameExecutor.OnTimerDone += ExecuteAfterTime;
            _afterFrameExecutor.StartTimer(_time);
        }

        public void ExecuteAfterTime()
        {
            _afterFrameExecutor.OnTimerDone -= ExecuteAfterTime;
            ExecuteUseCase(_useCase);
        }

        protected abstract void ExecuteUseCase(TUseCase useCase);
    }
}