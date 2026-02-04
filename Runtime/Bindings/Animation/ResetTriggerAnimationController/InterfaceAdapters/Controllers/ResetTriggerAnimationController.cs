using MVVM.Core;
using MVVM.Core.InterfaceAdapters;
using UnityEngine;

namespace MVVM.Bindings
{
    public class ResetTriggerAnimationController : Controller
    {
        private readonly Animator _animator;
        private readonly string _triggerName;

        public ResetTriggerAnimationController(IEventViewModel eventViewModel, Animator animator, string triggerName) : base(eventViewModel)
        {
            _animator = animator;
            _triggerName = triggerName;
        }

        public override void Execute()
        {
            _animator.ResetTrigger(_triggerName);
        }
    }
}