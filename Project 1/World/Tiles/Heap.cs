using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Tiles
{
    internal class Heap<T> where T : IHeapItem<T>
    {
        List<T> items;
        public int Count => count;
        int count;

        public Heap()
        {
            items = new List<T>();
            count = 0;
        }

        public T PopFirst()
        {
            if (items.Count == 0) return default(T);

            T returnable = items[0];
            count--;
            items[0] = items[count];
            items[0].HeapIndex = 0;
            items.RemoveAt(count);
            returnable.HeapIndex = 0;
            if (count == 0) return returnable;
            SortDown(items[0]);
            return returnable;
        }

        public bool Contains(T aItem) => items[aItem.HeapIndex].Equals(aItem);

        public void Add(T aValue)
        {
            aValue.HeapIndex = count;
            items.Add(aValue);
            
            SortUp(aValue);
            count++;
        }

        public void Clear()
        {
            count = 0;
            items.Clear();
        }


        int ParentIndex(int aIndex) => (int)Math.Round((aIndex - 1) / 2d, MidpointRounding.ToZero);
        int FirstChildIndex(int aIndex) => aIndex * 2 + 1;
        int SecondChildIndex(int aIndex) => aIndex * 2 + 2;

        void SortUp(T item)
        {
            if (item.HeapIndex == 40) 
                item.HeapIndex = 40;
            int parentIndex = ParentIndex(item.HeapIndex);
            while (true)
            {
                T parentItem = items[parentIndex];
                if (item.CompareTo(parentItem) <= 0) break;
                Swap(item, parentItem);
                parentIndex = ParentIndex(item.HeapIndex);

            }



            //if (aIndex == 0)
            //{
            //    SortDown(aIndex);
            //    return;
            //}

            //int parentIndex = ParentIndex(aIndex);
            //if (items[aIndex].CompareTo(items[parentIndex]) >= 0)
            //{
            //    SortDown(aIndex);
            //    return;
            //}

            //Swap(aIndex, parentIndex);
            //SortDown(aIndex); //Pushes the parentIndex which is now in the child down
            //SortUp(parentIndex); //Pushes the aIndex which is now in the parent up
        }

        private void SortDown(T aItem)
        {
            while (true)
            {
                int leftChildIndex = FirstChildIndex(aItem.HeapIndex);
                int rightChildIndex = SecondChildIndex(aItem.HeapIndex);

                int biggerChild = 0;

                if (leftChildIndex >= count) return;
                biggerChild = leftChildIndex;

                if (rightChildIndex < count && items[leftChildIndex].CompareTo(items[rightChildIndex]) < 0) biggerChild = rightChildIndex;

                if (aItem.CompareTo(items[biggerChild]) >= 0) return;

                Swap(aItem, items[biggerChild]);
            }


            //int leftChildIndex = FirstChildIndex(aIndex);
            //int rightChildIndex = SecondChildIndex(aIndex);
            //if (leftChildIndex >= count) return;
            //int biggerChildIndex = leftChildIndex;

            //if (rightChildIndex < count - 1 && items[leftChildIndex].CompareTo(items[rightChildIndex]) >= 0) biggerChildIndex = rightChildIndex;

            //if (items[aIndex].CompareTo(items[biggerChildIndex]) >= 0) return;

            //Swap(items[biggerChildIndex], items[aIndex]);

            //SortUp(aIndex);
            //SortDown(biggerChildIndex);
        }

        public void UpdateItem(T aItem)
        {
            SortUp(aItem);
            SortDown(aItem);
        }

        void Swap(T aItem, T bItem)
        {
            items[aItem.HeapIndex] = bItem;
            items[bItem.HeapIndex] = aItem;
            int temp = aItem.HeapIndex;
            aItem.HeapIndex = bItem.HeapIndex;
            bItem.HeapIndex = temp;
        }
    }

    public interface IHeapItem<T> : IComparable<T>
    {
        int HeapIndex { get; set; }
    }
}
