﻿/*
 * PiSearch
 * MemoryEfficientComplementBigULongArray - an implementation of BigArray for the ulong data type
 *  Uses MemoryEfficientBigULongArray and BigBoolArray to allow for the memory-efficient storage
 *  of ulong values (up to a specifiec max) and their bitwise complements
 * Designed for use in suffix array generation with SAIS, which stores the bitwise complements of values
 * By Josh Keegan 12/01/2015
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringSearch.Collections
{
    public class MemoryEfficientComplementBigULongArray : BigArray<ulong>
    {
        //Private vars
        BigArray<ulong> values;
        BigArray<bool> complements;

        //Public accessors & modifiers
        public ulong this[long i]
        {
            get
            {
                //Validation
                if (i >= this.Length || i < 0)
                {
                    throw new IndexOutOfRangeException();
                }

                bool complement = complements[i];
                ulong val = values[i];

                if(complement)
                {
                    val = ~val;
                }
                return val;
            }
            set
            {
                //Validation
                if (i >= this.Length || i < 0)
                {
                    throw new IndexOutOfRangeException();
                }

                bool complement = value > MaxValue;
                ulong val = complement ? ~value : value;

                complements[i] = complement;
                values[i] = val;
            }
        }

        public long Length { get; private set; }
        public ulong MaxValue { get; private set; }

        //Constructor
        public MemoryEfficientComplementBigULongArray(long length, ulong maxValue, 
            BigArray<ulong> values, BigArray<bool> complements)
        {
            //Validation
            if(values == null)
            {
                throw new ArgumentNullException("values");
            }

            if(complements == null)
            {
                throw new ArgumentNullException("complements");
            }

            if(values.Length < length)
            {
                throw new ArgumentException("values length must be >= length");
            }

            if(complements.Length < length)
            {
                throw new ArgumentException("complements length must be >= length");
            }

            //TODO: Length validation?? (must be positive)

            this.Length = length;
            this.MaxValue = maxValue;

            this.values = values;
            this.complements = complements;
        }

        public MemoryEfficientComplementBigULongArray(long length, ulong maxValue,
            BigArray<ulong> values)
            : this(length, maxValue, values, new BigBoolArray(length)) {  }

        public MemoryEfficientComplementBigULongArray(long length, ulong maxValue)
            : this(length, maxValue, new MemoryEfficientBigULongArray(length, maxValue)) {  }

        public MemoryEfficientComplementBigULongArray(long length)
            : this(length, ulong.MaxValue) {  }

        public IEnumerator<ulong> GetEnumerator()
        {
            for (long i = 0; i < this.Length; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }
    }
}