using System.Reflection;

namespace ImTool
{
    public abstract class Tab
    {

        public abstract string Name { get; } 
        public abstract void SubmitContent();
        public virtual void SubmitSettings(bool active) { }
        public virtual void SubmitMainMenu() { }
        
        public bool IsMainMenuOverridden 
        {
            get
            {
                MethodInfo m = GetType().GetMethod("SubmitMainMenu");
                return m.GetBaseDefinition().DeclaringType != m.DeclaringType;
            }
        }
    }
}