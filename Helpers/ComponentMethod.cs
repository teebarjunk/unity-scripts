using UnityEngine;
using System.Reflection;

// Simplifys calling component methods.
public struct ComponentMethod
{
    public Component component;
    public MethodInfo methodInfo;

    public ComponentMethod(Component component, MethodInfo methodInfo)
    {
        this.component = component;
        this.methodInfo = methodInfo;
    }

    // Calls method, Converts string data to parameters, as safely as possible.
    public void Invoke(string[] data)
    {
        MethodHelper.Invoke(component, methodInfo, data);
    }
}
