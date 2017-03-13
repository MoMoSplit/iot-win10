using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Temperature.ViewModel
{
    public class NotifyBase : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        private Dictionary<Object, Object> propertyValues = new Dictionary<Object, Object>();
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual Boolean Set(object value, [CallerMemberName] string name = "")
        {
            if (propertyValues.ContainsKey(name))
            {
                var oldValue = propertyValues[name];
                if (oldValue == null || !oldValue.Equals(value))
                {
                    propertyValues[name] = value;

                    OnPropertyChanged(name);
                    return true;
                }
            }
            else
            {
                propertyValues.Add(name, value);

                OnPropertyChanged(name);
                return true;
            }

            return false;
        }

        protected virtual T Get<T>([CallerMemberName] string name = "")
        {
            if (propertyValues.ContainsKey(name))
                return (T)propertyValues[name];
            else
                return default(T);
        }

        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #endregion
    }
}
