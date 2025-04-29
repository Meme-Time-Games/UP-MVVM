using System;

namespace MVVM.Core.InterfaceAdapters
{
    public interface IAfterFrameExecutor
    { 
        public Action OnExecute { get; set; }
        public void Execute();
    }
}