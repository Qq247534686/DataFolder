﻿// Type: System.Collections.Generic.IDictionary`2
// Assembly: mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// Assembly location: C:\Windows\Microsoft.NET\Framework\v4.0.30319\mscorlib.dll

using System.Collections;

namespace System.Collections.Generic
{
  [__DynamicallyInvokable]
  public interface IDictionary<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
  {
    [__DynamicallyInvokable]
    TValue this[TKey key] { [__DynamicallyInvokable] get; [__DynamicallyInvokable] set; }

    [__DynamicallyInvokable]
    ICollection<TKey> Keys { [__DynamicallyInvokable] get; }

    [__DynamicallyInvokable]
    ICollection<TValue> Values { [__DynamicallyInvokable] get; }

    [__DynamicallyInvokable]
    bool ContainsKey(TKey key);

    [__DynamicallyInvokable]
    void Add(TKey key, TValue value);

    [__DynamicallyInvokable]
    bool Remove(TKey key);

    [__DynamicallyInvokable]
    bool TryGetValue(TKey key, out TValue value);
  }
}
