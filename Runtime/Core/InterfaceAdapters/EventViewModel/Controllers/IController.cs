using System;

namespace MVVM.Core.InterfaceAdapters
{
    public interface IController : IDisposable
    {
        void Execute();
    }
}