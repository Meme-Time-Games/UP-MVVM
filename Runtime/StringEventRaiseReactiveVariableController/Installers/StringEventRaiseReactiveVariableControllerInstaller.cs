using MVVM.Core;
using MVVM.Core.Installers;
using MVVM.Core.InterfaceAdapters;

public class StringEventRaiseReactiveVariableControllerInstaller : ReactiveVariableControllerWithUseCaseInstaller<string, IEventViewModel>
{
    protected override IController GetReactiveController(IReactiveVariable<string> reactiveVariable, IEventViewModel useCase)
    {
        return new StringEventRaiseReactiveVariableController(reactiveVariable, useCase);
    }
}