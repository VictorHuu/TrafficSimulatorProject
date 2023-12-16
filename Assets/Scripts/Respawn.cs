using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///
///</summary>
namespace TrafficBase
{
    public class Respawn : MonoBehaviour
    {
        public Transform spawnPoint;
        private void FixedUpdate()
        {
            Collider[] raycastHits=Physics.OverlapSphere(transform.position, 2);
            foreach (var other in raycastHits)
            {
                if (other.gameObject.layer == 3)
                {
                    other.GetComponent<Transform>().position = spawnPoint.position;
                    this.gameObject.SetActive(false);
                    #region initialization 初始化
                    other.GetComponent<Run>().WontGoThroughCrossing = false;
                    if (other.GetComponent<Run>().InCrossing)
                        other.GetComponent<Run>().InCrossing = false;
                    if (other.GetComponent<Run>().crossing.mutex3.WaitOne(0) == true)
                        other.GetComponent<Run>().crossing.mutex3.ReleaseMutex();
                    #endregion
                    this.gameObject.SetActive(true);
                }
            }
        }
    }
}
