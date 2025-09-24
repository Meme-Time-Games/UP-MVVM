using MVVM.Core;
using MVVM.Core.InterfaceAdapters;
using UnityEngine;

namespace MVVM.Bindings
{
    public class PlayAnimationWithTriggerController : Controller
    {
        private readonly Animator _animator;
        private readonly string _triggerName;
        
        public PlayAnimationWithTriggerController(IEventViewModel eventViewModel, Animator animator, string triggerName) : base(eventViewModel)
        {
            _animator = animator;
            _triggerName = triggerName;
        }

        public override void Execute()
        {
            _animator.SetTrigger(_triggerName);
        }
    }
}