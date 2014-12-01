﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringSearch.Collections
{
    public class FixedLengthQueue<T>
    {
        //Private Vars
        private T[] array;
        private int head;

        //Public vars
        public int Count
        {
            get
            {
                return this.array.Length;
            }
        }

        public int Length
        {
            get
            {
                return this.Count;
            }
        }

        public T this[int i]
        {
            get
            {
                //Validation
                if(i < 0 || i >= this.array.Length)
                {
                    throw new IndexOutOfRangeException("index must be >=0 and < Length");
                }

                int arrayIdx = (i + this.head) % this.array.Length;

                return this.array[arrayIdx];
            }
        }

        //Constructors
        public FixedLengthQueue(int length)
            : this(new T[length]) { }
        
        public FixedLengthQueue(T[] initialValues)
            : this(initialValues, 0) { }

        public FixedLengthQueue(T[] initialValues, int head)
        {
            //Validation
            if(head < 0 || head >= initialValues.Length)
            {
                throw new ArgumentOutOfRangeException("head must be >= 0 and < initialValues.Length");
            }

            this.array = initialValues;
            this.head = head;
        }

        //Public methods
        public T DequeueEnqueue(T t)
        {
            T toRet = this.array[this.head];
            this.Enqueue(t);
            return toRet;
        }

        public void Enqueue(T t)
        {
            this.array[this.head] = t;
            this.head++;

            //Roll the head around if necessary
            this.head %= this.array.Length;
        }
    }
}
