using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PJH.Web.AzureFunctionHosting.HostProxy.Extensions
{
    public static class ObjectExtensions
    {
        public static T GetPrivateInstanceField<T>(this object obj, string name)
        {
            var field = obj.GetType().GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (T)field.GetValue(obj);
        }
        public static T GetPrivateInstanceProperty<T>(this object obj, string name)
        {
            var field = obj.GetType().GetProperty(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (T)field.GetValue(obj, null);
        }
    }
}
