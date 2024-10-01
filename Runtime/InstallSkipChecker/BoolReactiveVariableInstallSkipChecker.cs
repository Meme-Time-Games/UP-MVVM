namespace InstallSkipChecker
{
    public class BoolReactiveVariableInstallSkipChecker : ReactiveVariableInstallSkipChecker<bool>
    {
        protected override bool Compare(bool conditionValue, bool reactiveVariableValue)
        {
            bool areEquals = conditionValue == reactiveVariableValue;

            return areEquals;
        }
    }
}