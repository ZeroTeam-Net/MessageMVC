﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{

    class ObjectPoolGroup<T>
    {
        private readonly List<ObjectPool<T>> objectPools = new();

        private long mIndex = 0;

        public ObjectPoolGroup(int maxItem = 5000)
        {
            for (int i = 0; i < Math.Min(Environment.ProcessorCount, 16); i++)
            {
                objectPools.Add(new ObjectPool<T>(maxItem));
            }
        }
        public bool Push(T data)
        {
            return objectPools[Math.Abs(data.GetHashCode()) % objectPools.Count].Push(data);
        }
        public bool TryPop(out T data)
        {
            return objectPools[(int)(++mIndex % objectPools.Count)].TryPop(out data);
        }

    }

    class ObjectPool<T>
    {

        public ObjectPool(int maxItems = 5000)
        {
            mMaxItems = maxItems;

        }

        private readonly int mMaxItems;

        private readonly System.Collections.Concurrent.ConcurrentStack<T> mQueues = new();

        private int mCount;

        public bool Push(T data)
        {
            int value = System.Threading.Interlocked.Increment(ref mCount);
            if (value < mMaxItems)
            {
                mQueues.Push(data);
                return true;
            }
            else
            {
                System.Threading.Interlocked.Decrement(ref mCount);
                return false;
            }
        }

        public bool TryPop(out T data)
        {
            bool result;
            result = mQueues.TryPop(out data);
            if (result)
                System.Threading.Interlocked.Decrement(ref mCount);
            return result;
        }
    }
}
