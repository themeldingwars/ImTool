namespace ImTool
{
    public class WindowButton
    {
        public delegate void ClickedDelegate();
        
        public string Text;
        public ClickedDelegate OnClicked;

        public WindowButton()
        {
            Text = "?";
        }
        public WindowButton(string text)
        {
            Text = text ?? "?";
        }
        public WindowButton(string text, ClickedDelegate onClicked)
        {
            Text = text ?? "?";
            OnClicked = onClicked;
        }
    }
}