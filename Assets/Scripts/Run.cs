using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
///<summary>
///
///</summary>
namespace TrafficBase
{
    public class Run : MonoBehaviour
    {
        #region Data
        public bool Priority = false;
        public SteeringDirection direction;
        public WayPoint[] wayPoints;
        [HideInInspector]
        public bool InCrossing = false;
        public Crossing crossing;
        private WayPoint wayPoint;
        private float speed0;
        [SerializeField]
        private float speed;
        private Vector3 dir0;
        private Vector3 dir;
        private int turn;
        private bool FollowedPath;
        public bool WontGoThroughCrossing = false;
        
        #endregion
        #region Script Life Cycle
        /// <summary>
        /// 数据初始化
        /// </summary>
        private void Start()
        {
           
            //随机生成一个速度
            int seed = System.DateTime.Now.Millisecond;
            Random.InitState(seed);
            speed = Random.Range(10f, 12f);
            //确定方向
            switch (direction)
            {
                case SteeringDirection.forward:
                    dir = Vector3.forward;
                    break;
                case SteeringDirection.backward:
                    dir = Vector3.back;
                    break;
                case SteeringDirection.left:
                    dir = Vector3.left;
                    break;
                case SteeringDirection.right:
                    dir = Vector3.right;
                    break;
            }
            
            dir0 = dir;
            speed0=speed;
            //PointSelected = wayPoints[Random.Range(0, wayPoints.Length)];
            seed = System.DateTime.Now.Millisecond;
            Random.InitState(seed);
            turn = Random.Range(0, wayPoints.Length);
            wayPoint = wayPoints[turn];
        }
        public void Update()
        {
            if (crossing.list.Contains(GetComponent<Collider>()))
                return;

            if (speed > 12f)
                speed = 12f;
            if (speed0 > 12f)
                speed0 = 12f;
            //变道后调整移动方向
            if (InCrossing == false && speed != speed0)
                speed = speed0;
            //变道后使用线性插值调整车头朝向
            if (transform.rotation != Quaternion.LookRotation(dir0))
            {
                Quaternion targetRotation = Quaternion.LookRotation(dir0);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 1.5f * Time.deltaTime);
            }
            if ((!Priority) && (turn!=2))
            {
                bool signal = CheckSignal();
                if (signal) return;
            }
            CheckCar();

        }
#endregion
        //检测交通信号灯
        private bool CheckSignal()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, 25);
            foreach (var collider in colliders)
            {
                GameObject tmp = collider.gameObject;
                if (tmp.tag != "Signal")
                    continue;
                
                Signal redgreen = tmp.GetComponent<Signal>();
                if ((redgreen.color == Color.red && redgreen.hinder == direction &&
                    Vector3.Dot(dir, redgreen.GetComponent<Transform>().position - this.transform.position) > 0f))
                {
                    if (crossing.CrossingAvailable <= 0 && InCrossing == false)
                        return true;
                    else if (InCrossing)
                        return false;
                    return Vector3.Distance(redgreen.GetComponent<Transform>().position, this.transform.position) >= 3;
                }
            }
            return false;
        }
        /// <summary>
        /// 检测汽车碰撞
        /// </summary>
        private void CheckCar()
        {
            RaycastHit hit;
            bool obstacle = Physics.Raycast(this.transform.position, dir, out hit, 12);
            if (obstacle&&hit.collider!=null&& 
                (hit.collider.gameObject ==null|| 
                (hit.collider.gameObject != null&&hit.collider.gameObject.layer== 3)))
            {
                Run run = hit.collider.GetComponent<Run>();
                //前方有同方向车
                float distance = Vector3.Distance(run.transform.position, transform.position);

                if (Vector3.Dot(run.dir, dir) >= 0)
                {
                    if (distance < 8) return;
                    if (speed > run.speed)
                        speed = run.speed;
                }
                else
                    speed = speed0 * 0.5f;
            }
            transform.position += dir * Time.deltaTime * speed;
            #region select route
            if (InCrossing)
            {
                switch (turn)
                {
                    case 0:
                        break;
                    case 1:
                        dir0 = Quaternion.Euler(0, -90, 0) * dir0; // rotate the vector using the quaternion
                        turn = 0;
                        break;
                    case 2:
                        dir0 = Quaternion.Euler(0, 90, 0) * dir0; // rotate the vector using the quaternion
                        turn = 0;
                        break;
                }

                Physics.Raycast(wayPoint.transform.position, dir, out hit, 12);
                if (hit.collider != null && hit.collider.gameObject.layer == 3) return;
                StartCoroutine(TurnLane());
            }
            #endregion
        }


        /// <summary>
        /// 变道
        /// </summary>
        /// <returns></returns>
        private IEnumerator TurnLane()
        {
            
            while (Vector3.Distance(transform.position, wayPoint.transform.position) > 0.5f
                && Vector3.Dot(wayPoint.transform.position- transform.position,dir)>0)
            {
                //transform.LookAt(wayPoint.transform.position);
                dir = wayPoint.transform.position - transform.position;
                dir.y = dir0.y;
                speed = 0.6f * speed0;
                yield return new WaitForSeconds(Time.deltaTime);
            }
            
            if (dir != dir0)
            {
                dir = dir0;
            }
            #region Follow Pattern
                RaycastHit[] carsToFollow = Physics.RaycastAll(this.transform.position, dir, 100);
                foreach (var carToFollow in carsToFollow)
                {
                    if (carToFollow.collider.tag == "Nav")
                    {
                        NavWaypoint temp = carToFollow.collider.GetComponent<NavWaypoint>();
                        for (int i = 0; i < 3; i++)
                        {
                            this.wayPoints[i] = temp.nav[i];
                            this.wayPoints[i].direction = temp.direction;
                        
                        }
                    break;
                    }
                }
                this.gameObject.SetActive(false);
                this.gameObject.SetActive(true);
                #endregion
            yield return null;
        }
    }
}
