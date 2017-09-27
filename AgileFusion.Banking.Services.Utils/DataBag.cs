using AgileFusion.Banking.Services.Utils.Strategy;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace AgileFusion.Banking.Services.Utils
{
    public class DataBag : IEnumerable<KeyValuePair<string,object>>
    {
        private IDictionary<string, object> _storage { get; set; }
        private SimpleStrategy<KeyValuePair<string, object>, Action<string, object>> _insertOrUpdateStrategy;
        public bool IsThreadSafe { get; private set; }
        public DataBag(bool isThreadSafe = false)
        {
            this.IsThreadSafe = isThreadSafe;
            if (this.IsThreadSafe)
            {
                _storage = new ConcurrentDictionary<string, object>();
            }
            else
            {
                _storage = new Dictionary<string, object>();
            }
            _insertOrUpdateStrategy = new SimpleStrategy<KeyValuePair<string, object>, Action<string, object>>(GetStates());
        }

        public object this[string key]
        {
            get
            {
                object result;
                if (_storage.TryGetValue(key, out result))
                {
                    return result;
                }
                return null;
            }
            set
            {
                _insertOrUpdateStrategy.Execute(new KeyValuePair<string, object>(key, value)).Value.Invoke(key, value);
            }
        }

        public T Get<T>(string key)
        {
            var result = this[key];
            if (result is T) { return (T)result; }
            return default(T);
        }

        private IEnumerable<IState<KeyValuePair<string, object>, Action<string, object>>> GetStates()
        {
            return State<KeyValuePair<string, object>, Action<string, object>>.Chain(tp => IsThreadSafe, (key, value) =>
            {
                if (!((ConcurrentDictionary<string, object>)_storage).TryAdd(key, value))
                {
                    _storage[key] = value;
                }
            }).Append(tp => !_storage.ContainsKey(tp.Key), (key, value) => _storage.Add(key, value))
              .Append(tp => true, (key, value) => _storage[key] = value);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _storage.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
