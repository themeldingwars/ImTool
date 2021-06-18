namespace ImTool
{
    public abstract class Tab
    {
        public abstract string Name { get; } 
        public abstract void SubmitContent();
        public virtual void SubmitSettings(bool active) { }
    }
}