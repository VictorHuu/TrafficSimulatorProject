# 项目设计方案

## 任务描述

## 项目介绍

​     本次项目采用Unity 3D开发，较为生动地展示了十字路口的交通状况。

## 美术资源

1. [汽车](Assets/ARCADE%20-%20FREE%20Racing%20Car)：来源于Unity Asset Store的ARCADE库。大部分车为蓝色，一部分车为紫色，红色，黄色，分别表示警车，救护车，消防车。
2. 环境：地面采用Unity默认的Terrain，带有Terrain Collider防止携带物理引擎的游戏对象下沉。
3. 交通灯：创建一个空物体TrafficLight，然后再创建两个圆柱Cylinder作为子物体拼接起来，最后再加上球体Sphere作为指示灯。Cylinder材质设置为，Albebo Color为银灰色，带有很强的金属光泽，表面粗糙；Sphere材质设置为：Albedo Color通过脚本Signal设置，将在脚本部分详细介绍，表面光滑，emission light 发射光为白光。
4. 路点标志：本项目设置8个Waypoint，每个方向左右两个，结合脚本用于后续控制车的移动方向。
5. 路面：EWRoad与NSRoad，东西与南北方向的路各一条，各有白线两条，用于划分双车道。
6. MapCamera：小地图摄像头，通过设置摄像头大小为0.3*0.3，位置为（0.7，0）可以将该摄像头对应的图像置于左上角，深度Depth设置为1，可以保证该摄像头的内容位于主摄像头之上。摄像头的视角朝向十字路口的正中央垂直向下，并且有一定的高度。

## 脚本

​     本项目的所有脚本放于Asset文件夹下的Scripts文件夹，其中Common中存放枚举量。以下是各脚本的简要介绍：

​     [**Common/DirectionType**](Assets/Scripts/Common/DirectionType.cs)：放置行驶方向SteeringDirection枚举，有左右前后4种。

​     [**Common/Turn**](Assets/Scripts/Common/Turn.cs)：放置十字路口的行驶选择，有直走GoAhead,左转TurnLeft，右转TurnRight三种。

​     [**GUI**](Assets/Scripts/GUI.cs)：一些简单的按钮，如退出。通过Unity的脚本生命周期控制，具体来说指Unity的机制是事件触发，在从程序开始执行到结束的过程中一定会发生的一系列事件的规律。比如OnGUI就是指点击GUI上面的一些元素一定会触发的函数，我们可以在其中编写具体的代码，比如在OnGUI中首先判断是否点击了“退出”按钮，如果是，那么就退出程序。

​     [**Signal**](Assets/Scripts/Signal.cs)：用于控制信号灯颜色变化的脚本，挂载到信号灯上。首先是在脚本生命周期的Start函数中利用随机数初始化颜色（具体来说就是首先找到挂载物体的MeshRenderer网格渲染器，找到材料material，然后设置颜色color，即材料面板中的Albedo Color），因为Start在游戏开始执行时一定会被触发，因此常用来初始化。之后的AlterColor函数用于切换颜色，在Start的最后调用InvokeRepeating用于反复调用，其中InvokeRepeating的第一个参数为被调用函数名的字符串，第二个参数为调用的开始时间，第三个参数为调用频率，设置为8s。

​     [**WayPoint**](Assets/Scripts/Waypoint.cs)：路点属性，用于指示方向与辅助寻路。

​     [**Respawn**](Assets/Scripts/Respawn.cs)：车辆的重新生成，可以用于无限演示，主要是通过4个方向各有一个消失点与生成点来实现的，空白消失点设置一个盒子碰撞器并用射线进行碰撞检测，如果有车辆，那么就SetActive(false)注销该车辆,将车辆的位置设置为对应的生成点的位置，一般为一条路的另一端，再设置一些基本属性，最后SetActive(true)，这样的话就实现了无限车流。

​     接下来是本项目中最重要的两个脚本[**Run.cs**](Assets/Scripts/Run.cs)与[**Crossing.cs**](Assets/Scripts/Crossing.cs)：

###    车辆逻辑控制：

####     数据

​     首先介绍以下Run脚本的数据成员：

1. Priority 用于标识车辆是否为特种车辆，如果是那么可以无视红绿灯。
2. direction 车辆行驶的初始方向，用于和挂载到信号灯上的Signal中的行驶方向hinder配合来指定车辆需要注意的红绿灯。
3. wayPoints数组：用于表明车辆可能的行进方向所经过的路点。例如，对于一个马上就要过十字路口的车来说，需要设置三个路点，表明左转，右转，直行。
4. InCrossing：表明是否处于十字路口中，如果是，那么可以无视红绿灯。
5. crossing：车辆所要经过的十字路口，可以看做一个资源信号量，将在“信号量机制”部分说明。
6. wayPoint车辆在十字路口最终选择的行进方向对应的路点，从wayPoints数组中随机选取。
7. speed0与speed：车辆的行驶速度，其中speed0为车辆在直行时的行驶速度。
8. dir0与dir：车辆的行驶方向，其中dir0为车辆在直行时的行驶方向。
9. turn：用于指示车辆直行（0），左转（1）还是右转（2）。
10. WontGoThroughCrossing：用于指示车辆是否会经过十字路口，一般将沿着十字路口相反方向的车标记为true。

#### 脚本生命周期

​     在Start函数中，我们利用随机数生成一个介于[5，7.5）的速度，然后再根据direction确定方向dir，与direction不同的是，dir是向量，用于直接控制物体的移动。并且dir会随着物体的运动实时改变。最后是在WayPoints数组中随机挑选一个路点作为在十字路口选择方向的标识点wayPoint。

​     在Update函数中，首先判断资源信号量十字路口Crossing的值是否小于0，车辆是否会在十字路口内，如果是，那么直接返回，表示等待。这是因为Update在脚本生命周期中默认是每0.02s执行一次（在程序中可以通过指定Time.deltaTime来修改Update 的执行频率），通过设置为立即返回，那么Run将不会执行后续函数，车辆也不会行驶。后续的所有的非脚本生命周期函数可以通过在脚本生命周期函数中调用来起作用，因此我们也可以把脚本生命周期理解为必然事件，充分利用脚本生命周期，会让我们对Unity开发有一个高屋建瓴的认识。

​     在之后的Update函数中，还要调整变道后的移动方向，即判断现在的速度speed与原始速度speed0是否一致。然后使用线性插值来调整车头朝向，目标车头朝向为dir0，通过四元数的Lerp进行调整，最后一个参数用于表示车辆旋转的时间。

之后判断车辆是否为特种车辆，如果是那么执行CheckSignal函数，用于判断是否因为信号灯而停下。最后执行CheckCar函数，用于车辆与其他车辆的碰撞检测。

#### 检测信号灯CheckSignal

​     我们可以利用Unity自带的射线机制来判断一个物体附近的碰撞体。这里使用一个OverlapSphere函数，用于判断一定半径内的球体范围是否有碰撞体，并返回符合条件的碰撞体数组colliders。之后遍历colliders，只有标签为“Signal”（预先指定所有信号灯的标签）的才能进行后续操作。如果车辆需要注意的信号灯是红色，并且车辆位与信号灯相对（可以通过判断行驶方向与车辆到信号灯的矢量的点积是否大于0，如果大于0，说明相对），接下来进行具体分析：如果车辆不在十字路口中并且作为资源信号量的CrossingAvailable小于等于0，说明有阻碍，车辆不得向前，如果车辆在十字路口中，那么可以无视信号灯。最后，对于剩余的车辆，判断车辆距离相应信号灯的距离，如果距离太小（比如在信号灯的正下方），那么也可以无视信号灯。如果没有信号灯碰撞体，那么可以直接通行。

#### 检测汽车碰撞CheckCar

​     检测车辆碰撞，我们同样可以用射线，本次使用Raycast函数，用与判断某一方向上是否有碰撞体。如果有，那么判断本车辆与另一碰撞体对应车辆的关系。如果前方有同行车，并且自身车速较快，那么需要减速，如果过近，需要停止。接下来进行路线选择，如果车辆位于十字路口中，那么根据turn值确定直行还是转弯，通过利用欧拉角将原本的行驶方向顺/逆时针旋转90度来确定右/左转。再次进行碰撞检测，如果前方有车辆先暂停，否则同StartCoroutine开启一个协程函数TurnLane用于执行变道。

​     简要介绍以下Unity中的协程Coroutine：相较线程，协程更灵活，有多个入口和出口点，因此协程函数的返回值必须是可枚举的，因此设置为IEnumerator接口。协程是单线程的，它只是将一个很长的函数分成多次执行，减轻系统负担，每一分段通过yield return 返回，如果要等待几秒后再次执行，可以返回new WaitForSeconds(seconds)，下一次执行将从yield处继续执行。

​     接下来说明TurnLane变道函数。首先判断车辆是否位于路点前，即车辆是否完成变道。每一次循环，都要利用线性插值将车辆旋转以面向路点，这里不再赘述。并更改当前的速度与方向。然后再利用射线的RayCast判断前方是否有车辆，如果有，那么暂停，否则继续转弯。在执行完循环，即到达路点后，恢复之前的速度与时间。

### UI界面

​     首先再次创建一个新场景Begin，新场景所要包含的UI有：“退出程序”按钮，位于左上方，控制十字路口车流量的滑动条，与“开始演示”按钮，同时中间还有项目名称“Traffic Simulator”。我们可以给Begin场景中的主摄像头添加一个GUI脚本，其中可以通过SceneManager.GetActiveScene().name == "Begin"来判断当前场景是否为开始界面.如果是，那么可以通过GUILayout.Button添加按钮，通过GUI.HorizontalSlider添加滑动条。在点击开始演示按钮时，我们会调用SceneManager.LoadScene(“SampleScene”)来获取主场景。

​     在主场景中，左上角是返回开始界面的按钮，以此实现场景的灵活切换。有关十字路口的最大车流量的水平滑动条可以控制本项目中最关键的信号量CrossingAvailable的最大值Capacity，从而研究信号量的影响。左下角是倍速的滑动条，通过控制Time.timeScale大小来实现，Time.timeScale一般会影响Update函数里面的内容，尤其是有关Time.deltaTime的操作，从而实现暂停。右上角是暂停演示，同样是将Time.timeScale设置为0.

​     最后，从交互性上说，两个场景的相互转化可以让操作更灵活，倍速可以让玩家查看细节或者是节省时间，暂停演示同理。

###    资源信号量Crossing

​     现在来到了本次项目中最关键的部分。我们将十字路口视作一个资源信号量，只能进行三种操作，初始化用Start实现，P操作用OnTriggerEnter实现，V操作用OnTriggerExit实现。

简要介绍一下触发器Trigger。Trigger可以看做一种特殊的碰撞器Collider，它不参与物理引擎中的活动，但是当有碰撞体进入Trigger时，会触发脚本生命周期的OnTriggerEnter函数，当有碰撞体离开时，会触发OnTriggerExit函数。

我们首先来看一下老师课件中的信号量成员与P /V操作：

#### 1. 数据成员与初始化：
```C#
Typedef struct{

  Int value;

  Struct process*list;

}semaphore;
```
对应到脚本中：
```C#
internal int CrossingAvailable=2;

internal List<Collider> list;
```
list用于保存等待的车辆对应的碰撞体，CrossingAvailable为资源信号量(以下简称S)。我们有：

S>0表示十字路口可以再行驶S辆车；

S=0表示十字路口不可再行驶车辆；

S<0表示在十字路口中有|S|辆车在等待。

#### 2. V原语：
```C#
signal(semaphore* s){

 S->value++;

If(S->value<=0){

   Remove a process P from S->list;

   Wakeup(P);

}

}
```
  V操作用于释放资源（或使用权），执行V原语时可能唤醒一个阻塞进程。以下是本项目中的OnTriggerExit部分：
```C#
  if (other != null && other.gameObject != null && other.gameObject.layer == 3)

​      {

​        if (!colliders.ContainsKey(other.name))

​        {

​          mutex3.ReleaseMutex();

​          return;

​        }else

​          colliders.Remove(other.name);

​        other.GetComponent<Run>().InCrossing = false;

 

​        mutex3.WaitOne();

​        \#region Critical Section

​        CrossingAvailable++;

​        Run test;

​        if (CrossingAvailable <= 0)

​        {

​          int index = Random.Range(0, list.Count);

​          Collider collider = list[index];

​          test = collider.GetComponent<Run>();

​          test.crossing.mutex3.ReleaseMutex();

​          list.RemoveAt(index);

​        }else

​          mutex3.ReleaseMutex();

​        \#endregion

 

​       }
```
第一行用于判断是否为车辆对应碰撞体离开触发器。Mutex3为互斥锁，WaitOne函数与ReleaseMutex函数中间的为关键部分。关键部分的首先将资源信号量的值CrossingAvailable加1，对应S->value++;如果CrossingAvailable小于等于0，那么就从list队列中随机移除一个进程对应的Run（表示车辆）对应Remove a process P from S->list。与此同时就唤醒了一个进程，因为Unity3D本身就是多线程的，将对应的Run从list队列中移除后Update函数的第一个if语句

if (crossing.list.Contains(GetComponent<Collider>()))恒为否，对应线程不再等待，对应WakeUp(P)。如果CrossingAvailable大于0，那么就直接让经过的碰撞体的InCrossing为false，对应车辆继续行驶。

#### 3. P原语：
```C#
  Wait(semaphore* s){

​    S->value--;

​    If(S->value<0){

​      Add this process to S->list;

​      Block();

}

}
```
P操作用于申请资源（或使用权），进程执行P原语时，可能阻塞自己。对应到crossing脚本中：
```C#
if (other != null && other.gameObject != null && other.gameObject.layer == 3)

​      {

​        if (colliders.ContainsKey(other.name))

​          return;

​        else

​          colliders.Add(other.name, other);

​        other.GetComponent<Run>().InCrossing = true;

​        mutex.WaitOne();

​        #region critical section

​        CrossingAvailable--;

​        if (CrossingAvailable < 0)

​        {

​          if (other != null)

​            list.Add(other);

​          //等待，通过停止脚本生命周期的Update函数的执行

​        }

​        #endregion

​         mutex.ReleaseMutex();

​      }
```
首先确认是车辆的碰撞体进入十字路口，然后再判断每辆车只能作为一个碰撞体进入十字路口，通过对字典colliders的操作来实现，具体来说，就是如果字典colliders中存在other，说明之前other对应的车辆已经进入十字路口，再次进入是不合理的，所以直接返回，否则将该碰撞体添加到字典中，然后将该辆车标记为位于十字路口中，即Incrossing为真。来到关键部分，首先资源量-1，接着将碰撞体添加到等待队列中，这样的话Update函数的第一个if语句

if (crossing.list.Contains(GetComponent<Collider>()))恒为真，对应线程受到阻塞。

最后OnTriggerExit与OnTriggerEnter的关键代码前后分别用互斥锁mutex与mutex3保证原子性。
