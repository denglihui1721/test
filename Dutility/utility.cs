using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tl3d.Dutility
{
    public static class utility
    {
        public static string[] status = {
            "初始化最初状态",
            "模拟器启动完成",
            "启动app完",
            "进入游戏扫货",
            "正在掉线检测" };

        public static int StopAtAppStart = 0;
        public static int StartProcess(string filename, string[] args, bool wait = false)
        {
            try
            {
                string s = "";
                foreach (string arg in args)
                {
                    s = s + arg + " ";
                }
                s = s.Trim();
                Process myprocess = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo(filename, s);
                myprocess.StartInfo = startInfo;

                //通过以下参数可以控制exe的启动方式，具体参照 myprocess.StartInfo.下面的参数，如以无界面方式启动exe等
                myprocess.StartInfo.UseShellExecute = false;
                //myprocess.StartInfo.CreateNoWindow = true;//设置不显示窗口
                //myprocess.StartInfo.WindowStyle = ProcessWindowStyle.;
                myprocess.Start();
                int pid = myprocess.Id;
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid);
                if (wait) { myprocess.WaitForExit(); }
                //myprocess.Close();
                return myprocess.Id;
            }
            catch (Exception ex)
            {
                MessageBox.Show("启动应用程序时出错！原因：" + ex.Message);
            }
            return -1;
        }
        public static int StartProcessWithOutPut(string filename, string[] args, out string aProcessOutPut, bool wait = false)
        {
            aProcessOutPut = "nothing output！！！！！！！";
            try
            {
                string s = "";
                foreach (string arg in args)
                {
                    s = s + arg + " ";
                }
                s = s.Trim();
                Process myprocess = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo(filename, s);
                myprocess.StartInfo = startInfo;

                //通过以下参数可以控制exe的启动方式，具体参照 myprocess.StartInfo.下面的参数，如以无界面方式启动exe等
                myprocess.StartInfo.UseShellExecute = false; //关闭shell的使用  
                myprocess.StartInfo.RedirectStandardInput = true; //重定向标准输入  
                myprocess.StartInfo.RedirectStandardOutput = true; //重定向标准输出  
                myprocess.StartInfo.RedirectStandardError = true; //重定向错误输出  
                myprocess.StartInfo.CreateNoWindow = true;//设置不显示窗口  
                myprocess.Start();
                aProcessOutPut = myprocess.StandardOutput.ReadToEnd();
                if (wait) { myprocess.WaitForExit(); }
                myprocess.Close();
                 return myprocess.Id;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("启动应用程序时出错！原因：" + ex.Message);
            }
            return -1;
        }
        public static string AutoRegCom(string strCmd)
        {
            string rInfo;


            try
            {
                Process myProcess = new Process();
                ProcessStartInfo myProcessStartInfo = new ProcessStartInfo("cmd.exe");
                myProcessStartInfo.UseShellExecute = false;
                myProcessStartInfo.CreateNoWindow = true;
                myProcessStartInfo.RedirectStandardOutput = true;
                myProcess.StartInfo = myProcessStartInfo;
                myProcessStartInfo.Arguments = "/c " + strCmd;
                myProcess.Start();
                StreamReader myStreamReader = myProcess.StandardOutput;
                rInfo = myStreamReader.ReadToEnd();
                myProcess.Close();
                rInfo = strCmd + "\r\n" + rInfo;
                return rInfo;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static int IsIpPortUse(string ip, int port)
        {
            System.Net.IPAddress myIpAddress = IPAddress.Parse(ip);
            System.Net.IPEndPoint myIpEndPoint = new IPEndPoint(myIpAddress, port);
            try
            {
                System.Net.Sockets.TcpClient tcpClient = new TcpClient();
                tcpClient.Connect(myIpEndPoint);//对远程计算机的指定端口提出TCP连接请求
            }
            catch
            {
                return 0;
            }
            return 1;
        }

    }

}
