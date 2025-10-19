using System;
using System.Reflection;

namespace DebugMod
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class BindableMethod : Attribute
    {
        public string name;
        public string category;
        public bool allowLock = true;
    }

    public class BindAction
    {
        public string Name { get; }
        public string Category { get; }
        public bool AllowLock { get; }
        public Action Action { get; }

        public BindAction(string name, string category, bool allowLock, Action action)
        {
            Name = name;
            Category = category;
            AllowLock = allowLock;
            Action = action;
        }

        public BindAction(BindableMethod attribute, MethodInfo method)
        {
            Name = attribute.name;
            Category = attribute.category;
            AllowLock = attribute.allowLock;
            Action = (Action)Delegate.CreateDelegate(typeof(Action), method);
        }
    }
}
