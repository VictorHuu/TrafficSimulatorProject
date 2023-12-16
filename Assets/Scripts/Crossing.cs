using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Timeline;
namespace TrafficBase
{
    public class Crossing:MonoBehaviour
    {
        //semaphore
        Mutex mutex = new Mutex(false);
        internal Mutex mutex3 = new Mutex(false);
        internal int CrossingAvailable=2;
        public static int Capacity = 2;
        internal List<Collider> list=new List<Collider>();
        Dictionary<string,Collider> colliders = new Dictionary<string, Collider>();
        private void Start()
        {
            CrossingAvailable =Capacity;
            GetComponent<BoxCollider>().isTrigger= true;
        }
        //P operation
        void OnTriggerEnter(Collider other)
        {
            if (other != null && other.gameObject != null && other.gameObject.layer == 3)
            {
                if (colliders.ContainsKey(other.name))
                    return;
                else
                    colliders.Add(other.name, other);
                other.GetComponent<Run>().InCrossing = true;
                mutex.WaitOne();
                #region critical section
                CrossingAvailable--;
                if (CrossingAvailable < 0)
                {
                    if (other != null)
                        list.Add(other);
                    //等待，通过停止脚本生命周期的Update函数的执行
                }
                #endregion
                mutex.ReleaseMutex();
            }
            
        }
        //V operation
        private void OnTriggerExit(Collider other)
        {
            if (other != null && other.gameObject != null && other.gameObject.layer == 3)
            {
                if (!colliders.ContainsKey(other.name))
                {
                    mutex3.ReleaseMutex();
                    return;
                }else
                    colliders.Remove(other.name);
                other.GetComponent<Run>().InCrossing = false;

                mutex3.WaitOne();
                #region Critical Section
                CrossingAvailable++;
                Run test;
                if (CrossingAvailable <= 0)
                {
                    int index = Random.Range(0, list.Count);
                    Collider collider = list[index];
                    test = collider.GetComponent<Run>();
                    test.crossing.mutex3.ReleaseMutex();
                    list.RemoveAt(index);
                }else
                    mutex3.ReleaseMutex();
                #endregion

            }
            
        }
    }
}