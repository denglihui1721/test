using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using tl3d.Dutility;
using tl3d.Dddm;
using System.Windows.Forms;
using System.Management;
using System.Diagnostics;

namespace tl3d.emulator
{

    public enum status {
        INIT =0,
        EM_STARTUP = 1,
        APP_STARTUP = 2,
        BUY=3,
        CHECK=4
    };
    public class em
    {
        public int pid { get; set; }
        public string ip { get; set; }
        public int port { get; set; }

        public string name { get; set; }

        public int hwnd;

        public string title { get; set; }

        public string emPath;
        public string mvPath;



        string appActivityName = "com.cyou.cx.mtlbb.cyou/com.cyou.cx.mtlbb.cyou.UnityPlayerNativeActivity";

        string appActivityNameStop = "com.cyou.cx.mtlbb.cyou";

        public string checkpross = "com.thirdparty.superuser";

        public string mWordPath = "tlbb启动.txt";

        public ddm mDm = new ddm();

        public status mStatus = status.INIT;   //0初始化最初状态，1模拟器启动完成， 2启动app完 3进入游戏扫货 4正在扫货检测
        public string mStatusDesc { get; set; }   //0初始化最初状态，1模拟器启动完成， 2启动app完 3进入游戏扫货 4正在扫货检测

        public bool mRun = true;

        public int SetStatus(status s)
        {
            mStatus = s;
            mStatusDesc = utility.status[(int)s];
            return 1;

        }

        public int Init()
        {
            int ret = mDm.getinstance().SetDict(0, mWordPath);
            return 1;
        }
        public void Work()
        {
            Init();
            //启动模拟器
            while (mRun)
            {
                if (mStatus == status.INIT)
                {
                    if (-1 == StartupEmEx())
                    {
                        //todo 通知用户
                        return;
                    }
                    SetStatus(status.EM_STARTUP);
                }
                if (mStatus == status.EM_STARTUP)
                {
                    StopApp(appActivityNameStop);
                    Thread.Sleep(2000);
                    //启动游戏
                    StartApp(appActivityName);
                    SetStatus(status.APP_STARTUP);
                    while (utility.StopAtAppStart == 1)
                    {
                        Thread.Sleep(2000);
                    }
                }
                if (mStatus == status.APP_STARTUP)
                {
                    ProcessLogic();

                }
                if (mStatus == status.BUY)
                {
                    CheckOnline();
                }
            }


        }
        public int Reset()
        {
            pid = 0;
            hwnd = 0;
            SetStatus(status.INIT);
            mDm.getinstance().UnBindWindow();


            return 1;
        }
        public int StopEmEx()
        {
            return 1;
        }
        public int StartupEmEx()
        {

            for (int i = 0; i <= 10; i++)
            {
                StopEm();
                if (1 == StartupEm())
                {

                    //MessageBox.Show("w, h:" + (int)width + "," + (int)height);

                    break;
                }
                else if (i == 10)
                {
                    //todo 关闭模拟器
                    return -1;
                }
            }

            return 1;
        }
        public int StopEm()
        {

            string[] arg = new string[3];
            arg[0] = "controlvm";
            arg[1] = name;
            arg[2] = "poweroff";
            utility.StartProcess(@mvPath + exec.MM, arg);

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_Process where Name ='MEmu.exe'");
            ManagementObjectCollection moc = searcher.Get();
            string value = "";
            try
            {
                foreach (ManagementObject mo in moc)
                {
                    if (mo["CommandLine"] != null)
                    {
                        value = mo["CommandLine"].ToString();
                        string vmname = value.Remove(0, (value.Length - name.Length));
                        if (vmname.Equals(name))
                        {
                            Process proc = Process.GetProcessById(Convert.ToInt32(mo["ProcessID"]));
                            Console.WriteLine(pid);
                            proc.Kill();
                        }
                    }
                    //KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("searcher！原因：" + ex.Message);
            }


            if (pid != 0)
            {
                string[] arg1 = new string[3];
                arg1[0] = "/pid";
                arg1[1] = "" + pid;
                pid = utility.StartProcess("taskkill", arg1);
            }

            return 1;

        }
        public int StartupEm()
        {
            string[] arg = new string[1];
            arg[0] = name;
            pid = utility.StartProcess(@emPath + exec.EM, arg);
            Thread.Sleep(10000);
            //hwnd = mDm.getinstance().FindWindowByProcessId(pid, "Qt5QWindowIcon", "RenderWindowWindow");
            string hh = mDm.getinstance().EnumWindowByProcessId(pid, "RenderWindowWindow", "Qt5QWindowIcon", 3);
            hwnd = int.Parse(hh);
            for (int i = 0; i <= 10; i++)
            {
                if (1 == IsStartup())
                {
                    break;
                }
                else if (i == 10)
                {
                    return -1;
                }
                Thread.Sleep(5000);
            }

            return 1;
        }
        public int IsStartup()
        {
            string[] arg = new string[3];
            arg[0] = "-s";
            arg[1] = ip + ":" + port;
            arg[2] = "shell ps";
            string info = "";
            object width = 0;
            object height = 0;
            utility.StartProcessWithOutPut(@emPath + exec.ADB, arg, out info, false);
            if (info.Contains(checkpross))
            {
                mDm.getinstance().UnBindWindow();
                int ret = mDm.getinstance().BindWindowEx(hwnd, "dx.graphic.opengl", "windows", "windows", "dx.public.graphic.protect|dx.public.anti.api", 0);
                mDm.getinstance().GetClientSize(hwnd, out width, out height);

                string world = mDm.getinstance().Ocr(210, 547, 229, 571, "b9b9b9-303030|f3f3f3-303030", 1.0);
                if (world.Equals("ES"))
                {
                    return 1;
                }

            }
            return 0;
        }
        public int StartApp(string appName)
        {
            string[] arg = new string[4];
            arg[0] = "-s";
            arg[1] = ip + ":" + port;
            arg[2] = "shell am start -n";
            arg[3] = appName;
            string output = "";
            utility.StartProcessWithOutPut(@emPath + exec.ADB, arg, out output, true);
            return 0;
        }

        public int StopApp(string appName)
        {
            string[] arg = new string[4];
            arg[0] = "-s";
            arg[1] = ip + ":" + port;
            arg[2] = "shell am force-stop ";
            arg[3] = appName;
            string output = "";
            utility.StartProcessWithOutPut(@emPath + exec.ADB, arg, out output, true);
            return 0;
        }

        public int ProcessLogic()
        {
            string world = "";
            int count = 0;
            while (mStatus == status.APP_STARTUP)
            {

                //laucher3确定
                //world = mDm.getinstance().Ocr(936, 608, 987, 639, "b9b9b9-303030|f3f3f3-303030", 1.0);
                //if (world.Equals("确定"))
                //{
                //    //MessageBox.Show("world：" + world);
                //    mDm.getinstance().MoveTo((936 + 987) / 2, (608 + 639) / 2);
                //    mDm.getinstance().LeftClick();
                //    Thread.Sleep(2000);
                //}


                //登录公告
                world = mDm.getinstance().Ocr(349, 509, 379, 546, "fefefe-303030", 1.0);
                if (world.Equals("我"))
                {
                    //MessageBox.Show("world：" + world);
                    mDm.getinstance().MoveTo((349 + 379) / 2, (509 + 546) / 2);
                    mDm.getinstance().LeftClick();
                    Thread.Sleep(2000);
                }

                //进入角色
                world = mDm.getinstance().Ocr(359, 495, 402, 528, "fefefe-303030", 1.0);
                if (world.Equals("入"))
                {
                    //MessageBox.Show("world：" + world);
                    mDm.getinstance().MoveTo((359 + 402) / 2, (495 + 528) / 2);
                    mDm.getinstance().LeftClick();
                    Thread.Sleep(10000);
                }

                //进入游戏
                world = mDm.getinstance().Ocr(612, 417, 653, 450, "ffffee-303030", 1.0);
                if (world.Equals("二入"))
                {
                    //MessageBox.Show("world：" + world);
                    mDm.getinstance().MoveTo((612 + 653) / 2, (417 + 450) / 2);
                    mDm.getinstance().LeftClick();
                    Thread.Sleep(10000);
                }

                //已登录
                world = mDm.getinstance().Ocr(317, 260, 344, 292, "f9f9c7-303030", 1.0);
                if (world.Equals("已"))
                {
                    //MessageBox.Show("world：" + world);
                    mDm.getinstance().MoveTo(387, 363);
                    mDm.getinstance().LeftClick();
                    Thread.Sleep(4000);
                }

                //游戏公告
                world = mDm.getinstance().Ocr(775, 43, 788, 78, "75130f-303030", 1.0);
                if (world.Equals("xx"))
                {
                    //MessageBox.Show("world：" + world);
                    mDm.getinstance().MoveTo((775 + 788) / 2, (43 + 78) / 2);
                    mDm.getinstance().LeftClick();
                    Thread.Sleep(4000);
                }

                //点头像
                world = mDm.getinstance().Ocr(2, 43, 26, 58, "c7b669-303030", 1.0);
                if (world.Equals("头像"))
                {
                    //MessageBox.Show("world：" + world);
                    mDm.getinstance().MoveTo((2 + 26) / 2, (43 + 58) / 2);
                    mDm.getinstance().LeftClick();
                    Thread.Sleep(4000);
                }

                //寄售
                world = mDm.getinstance().Ocr(437, 124, 473, 163, "dad6d2-303030", 1.0);
                if (world.Equals("寄"))
                {
                    //MessageBox.Show("world：" + world);
                    mDm.getinstance().MoveTo((437 + 473) / 2, (124 + 163) / 2);
                    mDm.getinstance().LeftClick();
                    Thread.Sleep(4000);
                    SetStatus(status.BUY);
                    break;
                }
                Thread.Sleep(5000);
                count++;
                if (count == 150)
                {
                    SetStatus(status.EM_STARTUP);
                    break;
                }

            }
            return 1;
        }

        public int CheckOnline()
        {
            bool sOnline = true;
            string world = "";
            int i = 0;
            while (sOnline)
            {
                //寄售
                world = mDm.getinstance().Ocr(653, 248, 685, 278, "f0e301-303030", 1.0);
                if (world.Equals("试"))
                {
                    //MessageBox.Show("world：" + world);
                    //mDm.getinstance().MoveTo((653 + 685) / 2, (248 + 278) / 2);
                    //mDm.getinstance().LeftClick();
                    Thread.Sleep(2000);
                    i = 0;
                    //sOnline = false;
                }
                else
                {
                    i++;
                }
                if (i >=10)
                {
                    sOnline = false;
                    SetStatus(status.EM_STARTUP);
                }
                Thread.Sleep(10000);
            }
            return 1;
        }
    }
}
