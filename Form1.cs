using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using dm;
using System.Management;
using tl3d.emulator;
using System.Configuration;
using System.Threading;
using System.Xml;
using System.Net;
using System.Net.Sockets;
using Microsoft.Win32;
using tl3d.Dutility;

namespace tl3d
{
    public partial class Form1 : Form
    {
        private bool isConfig = false;
        ArrayList devices = new ArrayList();
        ArrayList emList = new ArrayList();
        ArrayList emNameList = new ArrayList();
        int sMEmuConsolePid = 0;
        string appActivityName = "com.cyou.cx.mtlbb.cyou/com.cyou.cx.mtlbb.cyou.UnityPlayerNativeActivity";
        dmsoft sDm;
        System.Configuration.Configuration config;
        ArrayList th = new ArrayList();
        int work = 0;
        public Form1()
        {
            InitializeComponent();
            LoadInfo();

            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.DataSource = emList;
            timer1.Start();
            //initDM();
            //textBox1.Text = "C:\\Program Files\\Microvirt\\MEmu\\";
        }


        public int LoadConfig()
        {
            string file = System.Windows.Forms.Application.ExecutablePath;
            config = ConfigurationManager.OpenExeConfiguration(file);
            //config.AppSettings.Settings.Add("run", "1");
            //config.Save();
            textBox1.Text = config.AppSettings.Settings["EmPath"].Value;
           
            checkBox1.Checked = config.AppSettings.Settings["run"].Value.Equals("1");

            isConfig = true;
            return 1;
        }

        public int LoadEmInfo()
        {
            string EmPath = textBox1.Text + "MemuHyperv VMs\\";
            DirectoryInfo TheFolder = new DirectoryInfo(EmPath);
            if (!TheFolder.Exists)
            {
                MessageBox.Show("路径错误");
                return -1;
            }
            foreach (DirectoryInfo NextFolder in TheFolder.GetDirectories())
            {
                string name = NextFolder.Name;
                em sEm = new emulator.em();
                sEm.name = name;
                sEm.pid = 0;
                sEm.emPath = textBox1.Text;
                sEm.mvPath = textBox1.Text.Replace("MEmu", "MEmuHyperv");
                LoadEMConfig(ref sEm);
                emList.Add(sEm);
            }
                return 1;
        }
        public int LoadInfo()
        {
            LoadConfig();
            LoadEmInfo();
            return 1;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;//该值确定是否可以选择多个文件
            dialog.Title = "请选择文件夹";
            dialog.Filter = "所有文件(MEmuConsole.exe)|MEmuConsole.exe";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = dialog.FileName.Replace(exec.MC, "");
                config.AppSettings.Settings["EmPath"].Value = textBox1.Text;
                config.Save();
            }

        }


        public int StartProcess(string filename, string[] args, bool wait=false)
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


        public int StartProcessWithOutPut(string filename, string[] args, out string aProcessOutPut, bool wait = false)
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
                if (wait) { myprocess.WaitForExit(); }
                aProcessOutPut = myprocess.StandardOutput.ReadToEnd();
                myprocess.Close();
                return myprocess.Id;
            }
            catch (Exception ex)
            {
                MessageBox.Show("启动应用程序时出错！原因：" + ex.Message);
            }
            return -1;
        }
        public  void GetAllEmProc(int pid)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                Process p2 = Process.GetProcessById(Convert.ToInt32(mo["ProcessID"]));
                textbox_output.Text += p2.ProcessName;
            }
            
        }
        public static void KillProcessAndChildren(int pid)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                string arg = mo["CommandLine"].ToString();
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                Process proc = Process.GetProcessById(pid);
                Console.WriteLine(pid);
                proc.Kill();
            }
            catch (ArgumentException)
            {
                /* process already exited */
            }
        }
         public int StartupEm(string aEmName)
        {
            em sEm = new em();
            string[] arg = new string[1];
            arg[0] = aEmName;
            sEm.pid = StartProcess(@textBox1.Text + exec.EM, arg);
            Thread.Sleep(5000);
            int hwnd = sDm.FindWindowByProcessId(sEm.pid, "", "");
            //string title = sDm.GetWindowTitle(hwnd);
            sEm.name = aEmName;
            sEm.hwnd = hwnd;
            //sEm.title = title;
            LoadEMConfig(ref sEm);
            emList.Add(sEm);
            return sEm.pid;
        }

        public int LoadEMConfig(ref em sEm)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(@textBox1.Text+ "MemuHyperv VMs\\"+ sEm.name + "\\"+ sEm.name + ".memu");
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsMgr.AddNamespace("ns", "http://www.innotek.de/MemuHyperv-settings");
            //XmlNode node = xmlDoc.SelectSingleNode("/ns:MemuHyperv/ns:Machine/ns:Hardware/ns:Network/ns:NAT/ns:Forwarding", nsMgr);
            XmlNodeList node = xmlDoc.SelectNodes("/ns:MemuHyperv/ns:Machine/ns:Hardware/ns:Network/ns:Adapter/ns:NAT/ns:Forwarding", nsMgr);
            sEm.ip = node[0].Attributes["hostip"].Value;
            sEm.port = Convert.ToInt32(node[0].Attributes["hostport"].Value);
            XmlNodeList node2 = xmlDoc.SelectNodes("/ns:MemuHyperv/ns:Machine/ns:Hardware/ns:GuestProperties/ns:GuestProperty", nsMgr);
            foreach (XmlNode n in node2)
            {
                if (n.Attributes["name"].Value == "name_tag")
                {
                    sEm.title = n.Attributes["value"].Value;
                }
            }
            return 1;
        }
        public int StartGame()
        {
            foreach (em sEm in emList)
            {
                StartApp(sEm.ip+":"+sEm.port,appActivityName);
            }
            return 1;
        }
        private void StartupEm_Click(object sender, EventArgs e)
        {

            if (work == 0)
            {
                button2.Text = "停止";
                //string[] arg1 = new string[3];
                //arg1[0] = "/f";
                //arg1[1] = "/m";
                //arg1[2] = exec.EM;
                //utility.StartProcess("taskkill", arg1);
                foreach (em sEm in emList)
                {
                    Thread t = new Thread(new ThreadStart(sEm.Work));
                    t.IsBackground = true;
                    t.Start();
                    th.Add(t);
                    Thread.Sleep(1000);
                }
                work = 1;
                
            }
            else
            {
                
                foreach (Thread t in th)
                {
                    t.Abort();
                }

                foreach (em sEm in emList)
                {
                    sEm.StopEm();
                    sEm.Reset();

                }

                //string[] arg1 = new string[3];
                //arg1[0] = "/f";
                //arg1[1] = "/m";
                //arg1[2] = exec.EM;
                //utility.StartProcess("taskkill", arg1);

                work = 0;
                button2.Text = "启动"; 
            }
            
            
            


        }
        private static IEnumerable<string> GetCommandLines(int  aParentID)
        {
            List<string> results = new List<string>();
            string wmiQuery = string.Format("select CommandLine from Win32_Process where ParentProcessID=", aParentID);
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(wmiQuery))
            {
                using (ManagementObjectCollection retObjectCollection = searcher.Get())
                {
                    foreach (ManagementObject retObject in retObjectCollection)
                    {
                        results.Add((string)retObject["CommandLine"]);
                    }
                }
            }
            return results;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            string[] arg = new string[2];
            arg[0] = "clone";
            arg[1] = "MEmu";
            StartProcess(@textBox1.Text+exec.MC, arg, true);
            MessageBox.Show("克隆完成", "系统提示");

        }

        private void button4_Click(object sender, EventArgs e)
        {
            string[] arg = new string[1];
            arg[0] = "shell ps";
            string sOutPut;
            StartProcessWithOutPut(@textBox1.Text+ exec.ADB, arg, out sOutPut, true);
            string[] lines = sOutPut.Split('\n');
            textbox_output.Text = "";
            foreach (string i in lines)
            {
                if (i.Contains("127"))
                {
                    textbox_output.Text += i.Replace("	device\r", "")+"\n";
                    string tmp = i.Replace("	device\r", "");
                    devices.Add(tmp);
                    StartApp(tmp, appActivityName);
                }
            }
            

        }
        public int StartApp(string vmIpPort, string appActivityName)
        {

            string[] arg = new string[4];
            arg[0] = "-s";
            arg[1] = vmIpPort;
            arg[2] = "shell am start -n";
            arg[3] = appActivityName;

            string sOutPut;
            StartProcessWithOutPut(@textBox1.Text + exec.ADB, arg, out sOutPut, true);
            return 0;
        }
        //>"C:\Program Files\Microvirt\MEmu\adb.exe" -s 127.0.0.1:21503 shell am start -n com.cyou.cx.mtlbb.cyou/com.cyou.cx.mtlbb.cyou.UnityPlayerNativeActivity


        private void button5_Click(object sender, EventArgs e)
        {
            //AutoRegCom("regsvr32 -s C:\\Users\\dlh\\Documents\\Visual Studio 2015\\Projects\\tl3d\\tl3d\\bin\\Debug\\dm.dll");
            //dmsoft sb = new dmsoft();
            //sb.MoveTo(30, 30);
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_Process where Name='MEmu.exe'");
            ManagementObjectCollection moc = searcher.Get();
            string arg = "";
            foreach (ManagementObject mo in moc)
            {
                if (mo["CommandLine"] != null)
                {
                    arg = mo["CommandLine"].ToString();
                    if (arg.Contains("MEmu.exe"))
                    {
                        arg = mo["CSName"].ToString();
                        arg = mo["Name"].ToString();
                        arg = mo["OSName"].ToString();
                        arg = mo["Status"].ToString();
                    }
                }
                //KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }

        }

        private void emStop(object sender, EventArgs e)
        {
            foreach (em sEm in emList)
            {
                KillProcessAndChildren(sEm.pid);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (!isConfig) { return; }
            if (checkBox1.Checked) //设置开机自启动  
            {
                MessageBox.Show("设置开机自启动，需要修改注册表", "提示");
                string path = Application.ExecutablePath;
                RegistryKey rk = Registry.LocalMachine;
                RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                rk2.SetValue("JcShutdown", path);
                rk2.Close();
                rk.Close();
                config.AppSettings.Settings["run"].Value = "1";
            }
            else //取消开机自启动  
            {
                MessageBox.Show("取消开机自启动，需要修改注册表", "提示");
                string path = Application.ExecutablePath;
                RegistryKey rk = Registry.LocalMachine;
                RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                rk2.DeleteValue("JcShutdown", false);
                rk2.Close();
                rk.Close();
                config.AppSettings.Settings["run"].Value = "0";
            }
            config.Save();
            
        }

        private void button7_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("shutdown", @"/r");
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = emList;

        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (utility.StopAtAppStart == 0)
            {
                utility.StopAtAppStart = 1;
                button8.Text = "继续";
                StartupEm_Click(sender, e);
            }
            else
            {
                utility.StopAtAppStart = 0;
                button8.Text = "停在app";
                

            }
        }
    }


}
