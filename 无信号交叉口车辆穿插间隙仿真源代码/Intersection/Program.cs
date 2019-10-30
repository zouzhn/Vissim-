using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using VISSIM_COMSERVERLib;

namespace Intersection
{
    class Intersection
    {
        //定义相关对象
        string fPath = null;  //项目的当前相对父路径
        private Vissim vissim = null;  //Vissim软件对象
        private Net net = null;  //路网对象
        //private TravelTimeEvaluation travelTimeEvaluation = null;  //行程时间评价对象
        private TravelTimes traveltimes = null;  //行程时间对象
        private TravelTime[] traveltime = new TravelTime[8];  //各个行程时间对象
        private const int SIGNALGROUPNUM = 6;  //所有信号灯组的数目
        private SignalGroup[] signalGroup = new SignalGroup[SIGNALGROUPNUM];  //信号灯组对象
        private Simulation simulation = null;  //仿真对象
        private Links links = null;  //路段集合对象
        private const int DETSNUM = 40;  //所有线圈的数目
        private Detector[] dets = new Detector[DETSNUM];  //线圈检测器对象
        //private Vehicle[] vehicle = new Vehicle[1];  //车辆对象
        private int[] n0 = new int[7] { 0, 0, 0, 0, 0, 0, 0 };  //南进口直行车道的最小空隙间隔内的车辆数目
        private int[] n1 = new int[6] { 0, 0, 0, 0, 0, 0 };   //北进口左转车道的最小空隙间隔内的车辆数目
        private int[] n2 = new int[7] { 0, 0, 0, 0, 0, 0, 0 };  //南进口左转车道的最小空隙间隔内的车辆数目
        private int[] n3 = new int[6] { 0, 0, 0, 0, 0, 0 };   //北进口直行车道的最小空隙间隔内的车辆数目
        private int[] n4 = new int[7] { 0, 0, 0, 0, 0, 0, 0 };  //西进口左转车道的最小空隙间隔内的车辆数目
        private int[] n5 = new int[6] { 0, 0, 0, 0, 0, 0 };   //东进口左转车道的最小空隙间隔内的车辆数目

        //构造方法
        public Intersection()
        {
            fPath = System.Environment.CurrentDirectory;
            vissim = new Vissim();
            vissim.LoadNet(fPath + @"\Intersection\intersection.inp");
            vissim.LoadLayout(fPath + @"\Intersection\intersection.ini");
            net = vissim.Net;
            //vissim.Evaluation.set_AttValue("travelTime", true);  //在Evaluation对话框中选中TRAVELTIME
            //travelTimeEvaluation = vissim.Evaluation.TravelTimeEvaluation;
            //travelTimeEvaluation.set_AttValue("compiled", true);  //设置文件为.rsz格式（编译格式）
            //travelTimeEvaluation.set_AttValue("raw", false);  //设置.rsr文件不可写
            //travelTimeEvaluation.set_AttValue("file", true);  //设置文件可写
            traveltimes = net.TravelTimes;
            simulation = vissim.Simulation;
            links = net.Links;
            initializeSignalGroups();
            initializeDetectors();
            initializeTraveltimes();
            startSimulation();
        }

        //向txt中写入数据
        public void WriteToText(string contents)
        {
            FileStream fs = new FileStream(fPath + @"\Intersection\evaluation.txt", FileMode.Create);
            //获得字节数组
            byte[] data = System.Text.Encoding.Default.GetBytes(contents);
            //开始写入
            fs.Write(data, 0, data.Length);
            //清空缓冲区、关闭流
            fs.Flush();
            fs.Close();
        }

        //初始化信号灯组对象
        public void initializeSignalGroups()
        {
            for (int i = 0; i < SIGNALGROUPNUM; i++)
            {
                signalGroup[i] = net.SignalControllers.GetSignalControllerByNumber(i + 1).SignalGroups.GetSignalGroupByNumber(1);
                signalGroup[i].set_AttValue("type", 2); //初始状态为绿灯，即允许车辆通过
            }
        }

        //初始化线圈对象
        public void initializeDetectors()
        {
            for (int i = 0; i < DETSNUM; i++)
            {
                dets[i] = net.SignalControllers.GetSignalControllerByNumber(1).Detectors.GetDetectorByNumber(i + 1);
            }
        }

        //初始化行程时间对象
        public void initializeTraveltimes()
        {
            for (int i = 0; i < 8; i++)
            {
                traveltime[i] = traveltimes.GetTravelTimeByNumber(i+1);
            }
        }

        //在指定时间段内获取各个进口道的车辆数和平均行程时间并写入txt文档中
        public void getVehAndTraveltime(int i, double interval)
        {
            if (i * 1.0 / simulation.Resolution == interval)
            {
                /*西进口直行车道*/
                Console.WriteLine("西进口直行车道的车辆数目： " + traveltime[0].GetResult(interval, "NVEHICLES", "", 0));
                Console.WriteLine("西进口直行车道的平均行程时间： " + traveltime[0].GetResult(interval, "TRAVELTIME", "", 0));
                /*西进口直行车道*/
                Console.WriteLine("西进口左转车道的车辆数目： " + traveltime[1].GetResult(interval, "NVEHICLES", "", 0));
                Console.WriteLine("西进口左转车道的平均行程时间： " + traveltime[1].GetResult(interval, "TRAVELTIME", "", 0));
                Console.WriteLine();
                //WriteToText("西进口直行车道的车辆数目： " + traveltime[0].GetResult(interval, "NVEHICLES", "", 0)+"\r\n");
                //WriteToText("西进口直行车道的平均行程时间： " + traveltime[0].GetResult(interval, "TRAVELTIME", "", 0) + "\r\n");
                //WriteToText("西进口左转车道的车辆数目： " + traveltime[1].GetResult(interval, "NVEHICLES", "", 0) + "\r\n");
                //WriteToText("西进口左转车道的平均行程时间： " + traveltime[1].GetResult(interval, "TRAVELTIME", "", 0) + "\r\n");

                /*东进口直行车道*/
                Console.WriteLine("东进口直行车道的车辆数目： " + traveltime[4].GetResult(interval, "NVEHICLES", "", 0));
                Console.WriteLine("东进口直行车道的平均行程时间： " + traveltime[4].GetResult(interval, "TRAVELTIME", "", 0));
                /*东进口左转车道*/
                Console.WriteLine("东进口左转车道的车辆数目： " + traveltime[5].GetResult(interval, "NVEHICLES", "", 0));
                Console.WriteLine("东进口左转车道的平均行程时间： " + traveltime[5].GetResult(interval, "TRAVELTIME", "", 0));
                Console.WriteLine();
                //WriteToText("东进口直行车道的车辆数目： " + traveltime[4].GetResult(interval, "NVEHICLES", "", 0) + "\r\n");
                //WriteToText("东进口直行车道的平均行程时间： " + traveltime[4].GetResult(interval, "TRAVELTIME", "", 0) + "\r\n");
                //WriteToText("东进口左转车道的车辆数目： " + traveltime[5].GetResult(interval, "NVEHICLES", "", 0) + "\r\n");
                //WriteToText("东进口左转车道的平均行程时间： " + traveltime[5].GetResult(interval, "TRAVELTIME", "", 0) + "\r\n");

                /*南进口直行车道*/
                Console.WriteLine("南进口直行车道的车辆数目： " + traveltime[2].GetResult(interval, "NVEHICLES", "", 0));
                Console.WriteLine("南进口直行车道的平均行程时间： " + traveltime[2].GetResult(interval, "TRAVELTIME", "", 0));
                /*南进口左转车道*/
                Console.WriteLine("南进口左转车道的车辆数目： " + traveltime[3].GetResult(interval, "NVEHICLES", "", 0));
                Console.WriteLine("南进口左转车道的平均行程时间： " + traveltime[3].GetResult(interval, "TRAVELTIME", "", 0));
                Console.WriteLine();
                //WriteToText("南进口直行车道的车辆数目： " + traveltime[2].GetResult(interval, "NVEHICLES", "", 0) + "\r\n");
                //WriteToText("南进口直行车道的平均行程时间： " + traveltime[2].GetResult(interval, "TRAVELTIME", "", 0) + "\r\n");
                //WriteToText("南进口左转车道的车辆数目： " + traveltime[3].GetResult(interval, "NVEHICLES", "", 0) + "\r\n");
                //WriteToText("南进口左转车道的平均行程时间： " + traveltime[3].GetResult(interval, "TRAVELTIME", "", 0) + "\r\n");

                /*北进口直行车道*/
                Console.WriteLine("北进口直行车道的车辆数目： " + traveltime[7].GetResult(interval, "NVEHICLES", "", 0));
                Console.WriteLine("北进口直行车道的平均行程时间： " + traveltime[7].GetResult(interval, "TRAVELTIME", "", 0));
                /*北进口左转车道*/
                Console.WriteLine("北进口左转车道的车辆数目： " + traveltime[6].GetResult(interval, "NVEHICLES", "", 0));
                Console.WriteLine("北进口左转车道的平均行程时间： " + traveltime[6].GetResult(interval, "TRAVELTIME", "", 0));
                //WriteToText("北进口直行车道的车辆数目： " + traveltime[7].GetResult(interval, "NVEHICLES", "", 0) + "\r\n");
                //WriteToText("北进口直行车道的平均行程时间： " + traveltime[7].GetResult(interval, "TRAVELTIME", "", 0) + "\r\n");
                //WriteToText("北进口左转车道的车辆数目： " + traveltime[6].GetResult(interval, "NVEHICLES", "", 0) + "\r\n");
                //WriteToText("北进口左转车道的平均行程时间： " + traveltime[6].GetResult(interval, "TRAVELTIME", "", 0) + "\r\n");
                string contents = "西进口直行车道的车辆数目： " + traveltime[0].GetResult(interval, "NVEHICLES", "", 0) + "\r\n" 
                    + "西进口直行车道的平均行程时间： " + traveltime[0].GetResult(interval, "TRAVELTIME", "", 0) + "\r\n" 
                    + "西进口左转车道的车辆数目： " + traveltime[1].GetResult(interval, "NVEHICLES", "", 0) + "\r\n" 
                    + "西进口左转车道的平均行程时间： " + traveltime[1].GetResult(interval, "TRAVELTIME", "", 0) + "\r\n" 
                    + "东进口直行车道的车辆数目： " + traveltime[4].GetResult(interval, "NVEHICLES", "", 0) + "\r\n" 
                    + "东进口直行车道的平均行程时间： " + traveltime[4].GetResult(interval, "TRAVELTIME", "", 0) + "\r\n" 
                    + "东进口左转车道的车辆数目： " + traveltime[5].GetResult(interval, "NVEHICLES", "", 0) + "\r\n" 
                    + "东进口左转车道的平均行程时间： " + traveltime[5].GetResult(interval, "TRAVELTIME", "", 0) + "\r\n" 
                    + "南进口直行车道的车辆数目： " + traveltime[2].GetResult(interval, "NVEHICLES", "", 0) + "\r\n" 
                    + "南进口直行车道的平均行程时间： " + traveltime[2].GetResult(interval, "TRAVELTIME", "", 0) + "\r\n" 
                    + "南进口左转车道的车辆数目： " + traveltime[3].GetResult(interval, "NVEHICLES", "", 0) + "\r\n" 
                    + "南进口左转车道的平均行程时间： " + traveltime[3].GetResult(interval, "TRAVELTIME", "", 0) + "\r\n" 
                    + "北进口直行车道的车辆数目： " + traveltime[7].GetResult(interval, "NVEHICLES", "", 0) + "\r\n" 
                    + "北进口直行车道的平均行程时间： " + traveltime[7].GetResult(interval, "TRAVELTIME", "", 0) + "\r\n" 
                    + "北进口左转车道的车辆数目： " + traveltime[6].GetResult(interval, "NVEHICLES", "", 0) + "\r\n" 
                    + "北进口左转车道的平均行程时间： " + traveltime[6].GetResult(interval, "TRAVELTIME", "", 0) + "\r\n";
                WriteToText(contents);
            }
        }

        //仿真入口
        public void startSimulation()
        {
            Console.WriteLine("仿真执行中...");
            vissim.ShowMaximized(); //最大化窗口
            //开始仿真
            for (int i = 0; i < simulation.Period * simulation.Resolution + 1; i++)
            {
                simulation.RunSingleStep();  //单步仿真
                /****************
                 * 通行策略的实现 *
                 ****************/
                southS();
                northL();
                southL();
                northS();
                westL();
                eastL();
                //获取平均行程时间和交通量
                getVehAndTraveltime(i, 3600);
            }
            simulation.Stop();
        }

        /**
         * 南进口直行方向通行策略
         */
        public void southS()
        {
            /*东进口左转*/
            if (dets[0].get_AttValue("vehicleid") != 0)
            {
                n0[0]++;
            }
            if (dets[1].get_AttValue("vehicleid") != 0)
            {
                n0[0]--;
            }
            /*西进口直行*/
            if (dets[2].get_AttValue("vehicleid") != 0)
            {
                n0[1]++;
            }
            if (dets[3].get_AttValue("vehicleid") != 0)
            {
                n0[1]--;
            }
            /*东进口直行*/
            if (dets[4].get_AttValue("vehicleid") != 0)
            {
                n0[2]++;
            }
            if (dets[5].get_AttValue("vehicleid") != 0)
            {
                n0[2]--;
            }
            //各个最小空隙之间没有车辆的那个时刻就让南进口直右方向车辆通过（考虑北进口左转的影响）
            if (n0[0] == 0 && n0[1] == 0 && n0[2] == 0 && dets[6].get_AttValue("vehicleid")== 0)
            {
                signalGroup[0].set_AttValue("type", 2);  //允许通行
            }
            else
            {
                signalGroup[0].set_AttValue("type", 3);  //不允许通行
            }
        }

        /**
         * 北进口左转方向通行策略
         */
        public void northL()
        {
            /*西进口左转*/
            if (dets[7].get_AttValue("vehicleid") != 0)
            {
                n1[0]++;
            }
            if (dets[8].get_AttValue("vehicleid") != 0)
            {
                n1[0]--;
            }
            /*南进口直行*/
            if (dets[9].get_AttValue("vehicleid") != 0)
            {
                n1[1]++;
            }
            if (dets[10].get_AttValue("vehicleid") != 0)
            {
                n1[1]--;
            }
            /*东进口直行*/
            if (dets[11].get_AttValue("vehicleid") != 0)
            {
                n1[2]++;
            }
            if (dets[12].get_AttValue("vehicleid") != 0)
            {
                n1[2]--;
            }
            //各个最小空隙之间没有车辆的那个时刻就让南进口直右方向车辆通过
            if (n1[0] == 0 && n1[1] == 0 && n1[2] == 0 && dets[13].get_AttValue("vehicleid") == 0)
            {
                signalGroup[3].set_AttValue("type", 2);  //允许通行
            }
            else
            {
                signalGroup[3].set_AttValue("type", 3);  //不允许通行
            }
        }

        /**
         * 南进口左转方向通行策略
         */
        public void southL()
        {
            /*东进口左转*/
            if (dets[14].get_AttValue("vehicleid") != 0)
            {
                n2[0]++;
            }
            if (dets[15].get_AttValue("vehicleid") != 0)
            {
                n2[0]--;
            }
            /*北进口直行*/
            if (dets[16].get_AttValue("vehicleid") != 0)
            {
                n2[1]++;
            }
            if (dets[17].get_AttValue("vehicleid") != 0)
            {
                n2[1]--;
            }
            /*西进口直行*/
            if (dets[18].get_AttValue("vehicleid") != 0)
            {
                n2[2]++;
            }
            if (dets[19].get_AttValue("vehicleid") != 0)
            {
                n2[2]--;
            }
            //各个最小空隙之间没有车辆的那个时刻就让南进口直右方向车辆通过
            if (n2[0] == 0 && n2[1] == 0 && n2[2] == 0 && dets[20].get_AttValue("vehicleid") == 0)
            {
                signalGroup[1].set_AttValue("type", 2);  //允许通行
            }
            else
            {
                signalGroup[1].set_AttValue("type", 3);  //不允许通行
            }
        }

        /**
         * 北进口直行方向通行策略
         */
        public void northS()
        {
            /*西进口左转*/
            if (dets[21].get_AttValue("vehicleid") != 0)
            {
                n3[0]++;
            }
            if (dets[22].get_AttValue("vehicleid") != 0)
            {
                n3[0]--;
            }
            /*东进口直行*/
            if (dets[23].get_AttValue("vehicleid") != 0)
            {
                n3[1]++;
            }
            if (dets[24].get_AttValue("vehicleid") != 0)
            {
                n3[1]--;
            }
            /*西进口直行*/
            if (dets[25].get_AttValue("vehicleid") != 0)
            {
                n3[2]++;
            }
            if (dets[26].get_AttValue("vehicleid") != 0)
            {
                n3[2]--;
            }
            //各个最小空隙之间没有车辆的那个时刻就让南进口直右方向车辆通过
            if (n3[0] == 0 && n3[1] == 0 && n3[2] == 0 && dets[27].get_AttValue("vehicleid") == 0)
            {
                signalGroup[2].set_AttValue("type", 2);  //允许通行
            }
            else
            {
                signalGroup[2].set_AttValue("type", 3);  //不允许通行
            }
        }

        /**
         * 西进口左转方向通行策略
         */
        public void westL()
        {
            /*南进口左转*/
            if (dets[28].get_AttValue("vehicleid") != 0)
            {
                n4[0]++;
            }
            if (dets[29].get_AttValue("vehicleid") != 0)
            {
                n4[0]--;
            }
            /*东进口直行*/
            if (dets[30].get_AttValue("vehicleid") != 0)
            {
                n4[1]++;
            }
            if (dets[31].get_AttValue("vehicleid") != 0)
            {
                n4[1]--;
            }
            //各个最小空隙之间没有车辆的那个时刻就让南进口直右方向车辆通过
            if (n4[0] == 0 && n4[1] == 0 && dets[32].get_AttValue("vehicleid") == 0 && dets[33].get_AttValue("vehicleid") == 0)
            {
                signalGroup[4].set_AttValue("type", 2);  //允许通行
            }
            else
            {
                signalGroup[4].set_AttValue("type", 3);  //不允许通行
            }
        }

        /**
         * 东进口左转方向通行策略
         */
        public void eastL()
        {
            /*北进口左转*/
            if (dets[34].get_AttValue("vehicleid") != 0)
            {
                n5[0]++;
            }
            if (dets[35].get_AttValue("vehicleid") != 0)
            {
                n5[0]--;
            }
            /*西进口直行*/
            if (dets[36].get_AttValue("vehicleid") != 0)
            {
                n5[1]++;
            }
            if (dets[37].get_AttValue("vehicleid") != 0)
            {
                n5[1]--;
            }
            //各个最小空隙之间没有车辆的那个时刻就让南进口直右方向车辆通过
            if (n5[0] == 0 && n5[1] == 0 && dets[38].get_AttValue("vehicleid") == 0 && dets[39].get_AttValue("vehicleid") == 0)
            {
                signalGroup[5].set_AttValue("type", 2);  //允许通行
            }
            else
            {
                signalGroup[5].set_AttValue("type", 3);  //不允许通行
            }
        }

        static void Main(string[] args)
        {
            new Intersection();
        }
    }
}
