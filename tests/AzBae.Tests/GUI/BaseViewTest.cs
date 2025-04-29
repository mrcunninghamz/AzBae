using System.Reflection;
using CommunityToolkit.Mvvm.Messaging;
using TestBae.BaseClasses;

namespace AzBae.Tests.GUI
{
    public abstract class BaseViewTest<T> : BaseTest<T>, IDisposable where T : class
    {
        /// <summary>
        /// Helper method to get private fields from an object using reflection
        /// </summary>
        protected static TField GetPrivateField<TField>(object obj, string fieldName, Type type = null)
        {
            type ??= obj.GetType();
            
            var field = type.GetField(fieldName, 
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            
            if (field == null)
            {
                throw new ArgumentException($"Field {fieldName} not found in {type.Name}");
            }
            
            return (TField)field.GetValue(obj);
        }
        
        public virtual void Dispose()
        {
            // Cleanup for next test
            WeakReferenceMessenger.Default.Reset();
        }
    }
}