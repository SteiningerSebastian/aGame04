using System;
using System.Collections;
using System.Collections.Generic;

public class SuperDictionary<TKey, TValue>
{
    private Dictionary<TKey, TValue> dictionary;
    private Queue<TKey> keys;
    private int capacity;

    public int Count => dictionary.Count;



    public SuperDictionary(int capacity)
    {
        this.keys = new Queue<TKey>(capacity);
        this.capacity = capacity;
        this.dictionary = new Dictionary<TKey, TValue>(capacity);
    }

    public void Add(TKey key, TValue value)
    {
        if (dictionary.Count == capacity)
        {
            var oldestKey = keys.Dequeue();
            dictionary.Remove(oldestKey);
        }

        dictionary.Add(key, value);
        keys.Enqueue(key);
    }

    public bool ContainsKey(TKey key)
    {
        return dictionary.ContainsKey(key);
    }

    public TValue this[TKey key]
    {
        get { return dictionary[key]; }
    }
}