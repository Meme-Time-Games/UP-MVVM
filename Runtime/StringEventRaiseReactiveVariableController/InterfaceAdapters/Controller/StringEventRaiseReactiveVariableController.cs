using MVVM.Core;
using MVVM.Core.InterfaceAdapters;

public class StringEventRaiseReactiveVariableController  : ReactiveVariableControllerWithUseCase<string, IEventViewModel>
{
    public StringEventRaiseReactiveVariableController(IReactiveVariable<string> reactiveVariable, IEventViewModel useCase) : base(reactiveVariable, useCase)
    {
    }

    protected override void ExecuteUseCase(IEventViewModel eventViewModel, string _)
    {
        eventViewModel.RaiseEvent();
    }
}