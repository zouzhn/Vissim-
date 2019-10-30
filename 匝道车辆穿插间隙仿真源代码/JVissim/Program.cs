using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VISSIM_COMSERVERLib;

namespace JVissim
{
    class JVissim
    {
        //定义相关对象
        public Vissim vissim = null;  //Vissim软件对象
        public Net net = null;  //路网对象
        public SignalGroup[] signalGroup = new SignalGroup[2];  //信号灯组对象
        public Simulation simulation = null;  //仿真对象
        public Links links = null;  //路段集合对象
        public Detector[] dets = new Detector[6];  //线圈检测器对象
        public Link[] link = new Link[3]; //路段对象
        public Vehicle[] vehicle = new Vehicle[4];  //车辆对象

        //初始化各个对象并设置相关参数
        public JVissim()
        {
            vissim = new Vissim();
            vissim.LoadNet(System.Environment.CurrentDirectory + @"\JVissim\jvissim.inp");
            vissim.LoadLayout(System.Environment.CurrentDirectory + @"\JVissim\jvissim.ini");
            net = vissim.Net;
            simulation = vissim.Simulation;
            links = net.Links;
            signalGroup[0] = net.SignalControllers.GetSignalControllerByNumber(1).SignalGroups.GetSignalGroupByNumber(1);
            signalGroup[0].set_AttValue("type", 2); //初始状态为绿灯，即允许车辆通过
            signalGroup[1] = net.SignalControllers.GetSignalControllerByNumber(2).SignalGroups.GetSignalGroupByNumber(1);
            signalGroup[1].set_AttValue("type", 2); //初始状态为绿灯，即允许车辆通过
            initializeLinks();
            initializeDetectors();
            startSimulation();
        }

        //初始化路段对象
        public void initializeLinks()
        {
            for (int i = 0; i < 3; i++)
            {
                link[i] = links.GetLinkByNumber(i+1);
            }
        }

        //初始化线圈对象
        public void initializeDetectors()
        {
            for (int i = 0; i < 6; i++)
            {
                dets[i] = net.SignalControllers.GetSignalControllerByNumber(1).Detectors.GetDetectorByNumber(i + 1);
            }
        }

        //仿真和策略控制
        public void startSimulation()
        {
            Console.WriteLine("仿真执行中...");
            int[] n = new int[2] { 0, 0 };  //最小空隙间隔内（也就是det_1与det_2之间的距离以及det_4与det_5之间的距离）的车辆数目
            vissim.ShowMaximized(); //最大化窗口

            //开始仿真
            for (int i = 0; i < simulation.Period * simulation.Resolution + 1; i++ )
            {
                simulation.RunSingleStep();  //单步仿真
                
                /****************
                 * 控制策略的实现 *
                 ****************/

                /*第二个匝道*/
                //西进口（主路）到达车辆自由行驶（让det_1和det_2两个线圈固定）
                if (dets[0].get_AttValue("vehicleid") != 0)  //有车进入线圈det_1，则n加1
                {
                    n[0]++;  //通过线圈det_1的车辆数目
                    //vehicle[0] = link[0].GetVehicles().GetVehicleByNumber(dets[0].get_AttValue("vehicleid"));
                    //vehicle[0].set_AttValue("speed", 10); //给车辆减速（要保证det_1处的来车不会与南进口的车冲突）
                    //vehicle[0].set_AttValue("desiredspeed", 10);
                }
                if (dets[1].get_AttValue("vehicleid") != 0)  //有车出线圈det_2则减1
                {
                    n[0]--;
                }
                //当det_1与det_2之间没有车辆且det_2处的车辆已经通过了det_3的那个时刻就让支路车辆通过
                if (n[0] == 0 && dets[2].get_AttValue("vehicleid") != 0)
                {
                    signalGroup[0].set_AttValue("type", 2);  //允许南进口车辆通行
                }
                else if (n[0] != 0)
                {
                    signalGroup[0].set_AttValue("type", 3);  //不允许南进口车辆通行
                }
                //给经过线圈det_3的车辆加速（无论是主路还是支路的车，均设置为相同的速度）
                if (dets[2].get_AttValue("vehicleid") != 0)
                {
                    vehicle[0] = link[0].GetVehicles().GetVehicleByNumber(dets[2].get_AttValue("vehicleid"));
                    vehicle[0].set_AttValue("speed", 30);
                    vehicle[0].set_AttValue("desiredspeed", 30);
                }

                //*第一个匝道*/
                //西进口（主路）到达车辆自由行驶（让det_4和det_5两个线圈固定）
                if (dets[3].get_AttValue("vehicleid") != 0)  //有车进入线圈det_4，则n加1
                {
                    n[1]++;  //通过线圈det_4的车辆数目
                    //vehicle[2] = link[0].GetVehicles().GetVehicleByNumber(dets[3].get_AttValue("vehicleid"));
                    //vehicle[2].set_AttValue("speed", 10); //给车辆减速（要保证det_4处的来车不会与南进口的车冲突）
                    //vehicle[2].set_AttValue("desiredspeed", 10);
                }
                if (dets[4].get_AttValue("vehicleid") != 0)  //有车出线圈det_5则减1
                {
                    n[1]--;
                }
                //当det_4与det_5之间没有车辆且det_5处的车辆已经通过了det_6的那个时刻就让支路车辆通过
                if (n[1] == 0 && dets[5].get_AttValue("vehicleid") != 0)
                {
                    signalGroup[1].set_AttValue("type", 2);  //允许南进口车辆通行
                }
                else if (n[1] != 0)
                {
                    signalGroup[1].set_AttValue("type", 3);  //不允许南进口车辆通行
                }
                //给经过线圈det_6的车辆加速（无论是主路还是支路的车，均设置为相同的速度）
                if (dets[5].get_AttValue("vehicleid") != 0)
                {
                    vehicle[2] = link[0].GetVehicles().GetVehicleByNumber(dets[5].get_AttValue("vehicleid"));
                    vehicle[2].set_AttValue("speed", 30);
                    vehicle[2].set_AttValue("desiredspeed", 30);
                }
            }
            simulation.Stop();
            Console.WriteLine("仿真结束！");
        }

        static void Main(string[] args)
        {
            new JVissim();
        }
    }
}
