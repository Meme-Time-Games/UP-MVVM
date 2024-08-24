using UnityEngine.UI;

namespace View
{
    public class OnClickButtonView : ButtonView
    {
        protected override void Subscribe(Button button)
        {
            button.onClick.AddListener(Raise);
        }

        protected override void Dispose(Button button)
        {
            button.onClick.RemoveListener(Raise);
        }
    }
}