using System.Collections.Generic;

namespace LevelImposter.Shop.Utils;

public abstract class ObjectPool<T>
{
    private readonly List<T> _activePoolItems = [];
    private readonly Queue<T> _inactivePoolItems = new();

    /// <summary>
    ///     Appends the capacity of the pool by creating new items and adding them to the inactive pool.
    /// </summary>
    /// <param name="capacity">The number of items to add to the pool.</param>
    public void AppendCapacity(int capacity)
    {
        for (var i = 0; i < capacity; i++)
        {
            var item = InitializePoolItem();
            _inactivePoolItems.Enqueue(item);
        }
    }

    protected abstract T InitializePoolItem();

    protected virtual void OnPoolItemCreated(T item)
    {
    }

    protected virtual void OnPoolItemDestroyed(T item)
    {
    }

    /// <summary>
    ///     Gets a new, inactive item from the pool
    /// </summary>
    /// <returns>A new, inactive item from the pool</returns>
    public T Get()
    {
        if (_inactivePoolItems.Count == 0)
            _inactivePoolItems.Enqueue(InitializePoolItem());

        var poolItem = _inactivePoolItems.Dequeue();
        _activePoolItems.Add(poolItem);
        OnPoolItemCreated(poolItem);
        return poolItem;
    }

    /// <summary>
    ///     Returns the given pool item to the pool
    /// </summary>
    /// <param name="item">The pool item to return</param>
    public void Return(T item)
    {
        _activePoolItems.Remove(item);
        _inactivePoolItems.Enqueue(item);
        OnPoolItemDestroyed(item);
    }

    /// <summary>
    ///     Returns all active pool items to the pool
    /// </summary>
    public void ReturnAll()
    {
        while (_activePoolItems.Count > 0)
            Return(_activePoolItems[0]);
    }
}