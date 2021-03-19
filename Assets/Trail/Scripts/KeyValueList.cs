using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Trail
{
    /// <summary>
    /// Simple wrapper to replace <c>Dictionary&lt;string,string&gt;</c>. 
    /// This helps limit the size of the values and convert to binary for the internal c/c++ API.
    /// </summary>
    [Serializable]
    public unsafe class KeyValueList : IEnumerable<KeyValueList.KeyValue>
    {
        #region Consts

        private const int DEFAULT_SIZE = 4;
        public const int KEY_SIZE = 63;
        public const int VALUE_SIZE = 255;

        #endregion

        #region Variables

        [UnityEngine.SerializeField]
        private List<KeyValue> keyValueList = new List<KeyValue>();

        #endregion

        #region Properties

        public KeyValue this[int index]
        {
            get
            {
                return keyValueList[index];
            }
            set
            {
                keyValueList[index] = value;
            }
        }

        public string this[string key]
        {
            get
            {
                for (int i = Length - 1; i >= 0; i--)
                {
                    if (keyValueList[i].Key == key)
                    {
                        return keyValueList[i].Value;
                    }
                }
                SDK.Log(LogLevel.Error, string.Format("Key does not exist '{0}'", key));
                return "";
            }
            set
            {
                for (int i = Length - 1; i >= 0; i--)
                {
                    if (keyValueList[i].Key == key)
                    {
                        keyValueList[i] = new KeyValue(key, value);
                        return;
                    }
                }
                SDK.Log(LogLevel.Error, string.Format("Key does not exist '{0}'", key));
            }
        }

        /// <summary>
        /// Returns the amount of items this keyValueList have.
        /// </summary>
        public int Length
        {
            get
            {
                return keyValueList.Count;
            }
        }

        /// <summary>
        /// Returns the amount of items this keyValueList have.
        /// </summary>
        public int Count
        {
            get
            {
                return keyValueList.Count;
            }
        }

        internal Binary[] Binaries
        {
            get
            {
                return keyValueList.Select(x => x.Binary).ToArray();
            }
        }

        /// <summary>
        /// Allocates unmanaged memory of all the binaries from the tags. This has to be unallocated using Marshal.FreeHGlobal(ptr) afterwards.
        /// </summary>
        internal IntPtr BinaryPtr
        {
            get
            {
                var count = keyValueList.Count;
                var size = Marshal.SizeOf(typeof(Binary));
                IntPtr array = Marshal.AllocHGlobal(count * size);
                for (int i = 0; i < count; i++)
                {
                    IntPtr offset = new IntPtr(array.ToInt64() + i * size);
                    Marshal.StructureToPtr(keyValueList[i].Binary, offset, false);
                }

                return array;
            }
        }

        #endregion

        #region Constructor

        public KeyValueList() : this(DEFAULT_SIZE) { }

        public KeyValueList(int size)
        {
            keyValueList = new List<KeyValue>(size);
        }

        public KeyValueList(IList<KeyValue> otherKeyValueList) : this(otherKeyValueList.Count)
        {
            foreach (var t in otherKeyValueList)
            {
                Add(t);
            }
        }

        public KeyValueList(string key, string value) : this(1)
        {
            Add(key, value);
        }

        public KeyValueList(IDictionary<string, string> otherKeyValueDictionary) : this(otherKeyValueDictionary.Count)
        {
            foreach (var t in otherKeyValueDictionary)
            {
                Add(t.Key, t.Value);
            }
        }

        #endregion

        #region Add

        /// <summary>
        /// Adds a new item to the KeyValueList. 
        /// </summary>
        /// <param name="key">The key for the item.</param>
        /// <param name="value">The value for the item.</param>
        /// <returns>Returns itself for linking. .Add().Add()</returns>
        public KeyValueList Add(string key, string value)
        {
            return Add(new KeyValue(key, value));
        }

        /// <summary>
        /// Adds a new item to the KeyValueList. 
        /// </summary>
        /// <param name="keyValue">A keyvalue item to add to this KeyValueList.</param>
        /// <returns>Returns itself for linking. .Add().Add()</returns>
        public KeyValueList Add(KeyValue keyValue)
        {
            if (Has(keyValue.Key))
            {
                throw new Exception("KeyValueList already contains a key: " + keyValue.Key);
            }
            keyValueList.Add(keyValue);
            return this;
        }

        #endregion

        #region Remove

        /// <summary>
        /// Removes an item from this KeyValueList.
        /// </summary>
        /// <param name="key">The key to look for and remove.</param>
        /// <returns>Whether it succeedes at removing an item from the KeyValueList.</returns>
        public bool Remove(string key)
        {
            for (int i = keyValueList.Count - 1; i >= 0; i--)
            {
                if (keyValueList[i].Key == key)
                {
                    ReplaceWithLastAndRemove(i);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes an item from this KeyValueList.
        /// </summary>
        /// <param name="keyValue">The keyvalue container to get the key from to remove.</param>
        /// <returns>Whether it succeedes at removing an item from the KeyValueList.</returns>
        public bool Remove(KeyValue keyValue)
        {
            return Remove(keyValue.Key);
        }

        // Internal faster list removal as it does not rely on ordering.
        private void ReplaceWithLastAndRemove(int index)
        {
            keyValueList[index] = keyValueList[keyValueList.Count - 1];
            keyValueList.RemoveAt(keyValueList.Count - 1);
        }

        #endregion

        #region Checks

        /// <summary>
        /// Checks whether it has an item with provided key.
        /// </summary>
        /// <param name="key">The key to check for.</param>
        /// <returns>Returns whether item exists with key.</returns>
        public bool Has(string key)
        {
            for (int i = Length - 1; i >= 0; i--)
            {
                if (keyValueList[i].Key == key)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks whether it has an item with provided key.
        /// </summary>
        /// <param name="key">The key to check for.</param>
        /// <returns>Returns whether item exists with key.</returns>
        public bool Contains(string key)
        {
            return Has(key);
        }

        #endregion

        #region SetValue
        
        /// <summary>
        /// Sets the value of provided key.
        /// </summary>
        /// <param name="key">The key to apply new value to.</param>
        /// <param name="value">The new value to apply</param>
        public void Set(string key, string value)
        {
            Set(key, value, false);
        }

        /// <summary>
        /// Sets the value of provided key.
        /// </summary>
        /// <param name="key">The key to apply new value to.</param>
        /// <param name="value">The new value to apply</param>
        /// <param name="addIfNotExist">Whether to add new item if key does not exist.</param>
        public void Set(string key, string value, bool addIfNotExist)
        {
            for (int i = Length - 1; i >= 0; i--)
            {
                if (keyValueList[i].Key == key)
                {
                    keyValueList[i] = new KeyValue(key, value);
                    return;
                }
            }
            if (addIfNotExist)
            {
                Add(key, value);
            }
            else
            {
                SDK.Log(LogLevel.Warning, "Cannot set value because the key does not exist");
            }
        }
        #endregion

        #region IEnumerable

        IEnumerator<KeyValue> IEnumerable<KeyValue>.GetEnumerator()
        {
            return keyValueList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return keyValueList.GetEnumerator();
        }

        #endregion


        [System.Serializable]
        public struct KeyValue
        {
            public string Key;
            public string Value;

            internal Binary Binary
            {
                get
                {
                    return new Binary(Key, Value);
                }
            }

            public KeyValue(string key, string value)
            {
                if (key.Length > KeyValueList.KEY_SIZE)
                {
                    SDK.Log(LogLevel.Error, "KeyValueList, key is to long! Max size = " + KeyValueList.KEY_SIZE);
                    this.Key = "";
                }
                else
                {
                    this.Key = key;
                }
                if (value.Length > KeyValueList.VALUE_SIZE)
                {
                    SDK.Log(LogLevel.Error, "KeyValueList, value is to long! Max size = " + KeyValueList.VALUE_SIZE);
                    this.Value = "";
                }
                else
                {
                    this.Value = value;
                }
            }
        }


        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct Binary
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = KeyValueList.KEY_SIZE + 1)]
            private byte[] key;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = KeyValueList.VALUE_SIZE + 1)]
            private byte[] value;

            /// <summary>
            /// Get or set the key of the binary keyvalue.
            /// </summary>
            public string Key
            {
                get { return Common.GetUTF8StringSafely(key); }
                set
                {
                    if (value.Length > KeyValueList.KEY_SIZE)
                    {
                        SDK.Log(LogLevel.Error, "KeyValueList, key is to long! Max size = " + KeyValueList.KEY_SIZE);
                        return;
                    }
                    Common.GetUTF8BytesSafely(value, key);
                }
            }

            /// <summary>
            /// Get or set the value of the binary keyvalue.
            /// </summary>
            public string Value
            {
                get { return Common.GetUTF8StringSafely(value); }
                set
                {
                    if (value.Length > KeyValueList.VALUE_SIZE)
                    {
                        SDK.Log(LogLevel.Error, "KeyValueList, value is to long! Max size = " + KeyValueList.VALUE_SIZE);
                        return;
                    }
                    Common.GetUTF8BytesSafely(value, this.value);
                }
            }

            public Binary(string key, string value)
            {
                this.key = new byte[KeyValueList.KEY_SIZE + 1];
                Common.GetUTF8BytesSafely(key, this.key);

                this.value = new byte[KeyValueList.VALUE_SIZE + 1];
                Common.GetUTF8BytesSafely(value, this.value);
            }
        }
    }
}
