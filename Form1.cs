using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        Thread threadCPU;
        Thread threadRAM;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Генерация текста в label1
            GetOSIfo();
           
            // Генерация текста в label2
            GetHDDInfo();
            // генерация текста для label3
            GetVideoInfo();
            // генерация текста для label4
            GetBIOSInfo();
            // генерация текста для label5
            GetNetworkInfo();

            label6.Text = "Загрузка ЦП:";
            label7.Text = "Загрузка RAM";

            threadCPU = new Thread(CPUMonitor);
            threadCPU.Start();
            threadRAM = new Thread(RAMMonitor);
            threadRAM.Start();
        }

        void GetOSIfo()
        {
            String OSversion;
            var name = (from x in new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem").Get().OfType<ManagementObject>()
                        select x.GetPropertyValue("Caption")).First();
            OSversion = "Версия ОС: " + name.ToString();
            if (Environment.Is64BitOperatingSystem)
            {
                OSversion += " x64";
            }
            else
            {
                OSversion += " x32";
            }
            label1.Text = OSversion;
            label1.Text += "\n";
            label1.Text += "Имя локальной машины: ";
            label1.Text += Environment.MachineName;
            String CPUName = "";
            label1.Text += "\n";
            List<String> userName = new List<string>();
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
            foreach (ManagementObject obj in searcher.Get())
            {
                CPUName += "Модель Процессора: ";
                CPUName += obj["Name"];
                CPUName += "\n";
                CPUName += "Кличество ядер: ";
                CPUName += obj["NumberOfCores"];
            }
            label1.Text += "\n";
            label1.Text += CPUName;
            label1.Text += "\n";
            searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
            UInt64 Capacity = 0;
            label1.Text += "Объем ОЗУ: ";
            foreach (ManagementObject obj in searcher.Get())
            {
                Capacity += Convert.ToUInt64(obj.Properties["Capacity"].Value);
            }
            Capacity /= 1048576;
            Capacity /= 1024;
            label1.Text += Capacity;
            label1.Text += " GB\n";
        }

        void GetHDDInfo()
        {
            DriveInfo[] allDrives;
            label2.Text = "Общая емкость HDD: ";
            allDrives = DriveInfo.GetDrives();
            Double FullCapacity = 0;
            foreach (DriveInfo obj in allDrives)
            {
                if (obj.IsReady && Convert.ToString(obj.DriveType) == "Fixed")
                {
                    FullCapacity += obj.TotalSize / 1073741824;
                }
            }
            label2.Text += FullCapacity + " GB\n";
            label2.Text += "Список разделов HDD\n";
            foreach (DriveInfo obj in allDrives)
            {
                if (obj.IsReady && Convert.ToString(obj.DriveType) == "Fixed")
                {
                    label2.Text += "Метка тома: ";
                    label2.Text += obj.Name;
                    label2.Text += "\n";
                    label2.Text += "Тип файловой системы: ";
                    label2.Text += obj.DriveFormat;
                    label2.Text += "\n";
                    label2.Text += "Размер тома: ";
                    Double CapacityHDD = 0;
                    CapacityHDD = obj.TotalSize / 1073741824;
                    label2.Text += CapacityHDD + " GB\n";
                    label2.Text += "Своодно: ";
                    Double UsedSpace = 0;
                    Double FreeSpace = 0;
                    FreeSpace = obj.AvailableFreeSpace / 1073741824;
                    label2.Text += FreeSpace + " GB\n";
                    label2.Text += "Занято:";
                    UsedSpace = CapacityHDD - FreeSpace;
                    label2.Text += UsedSpace + " GB\n";
                    label2.Text += "\n";
                }
            }
            label2.Text += "Список дисководов:\n";
            foreach (DriveInfo obj in allDrives)
            {

                if (!obj.IsReady && (Convert.ToString(obj.DriveType) == "CDRom"))
                {
                    label2.Text += "Имя дисковода:\n";
                    label2.Text += obj.Name;
                }
                else if (obj.IsReady && (Convert.ToString(obj.DriveType) == "CDRom"))
                {
                    label2.Text += "Тип файловой системы: ";
                    label2.Text += obj.DriveFormat + "\n";
                }

            }
        }

        void GetVideoInfo()
        {
            label3.Text = "Информация о видеопроцессорах установленных в системе:\n";
            label3.Text += "\n";
            List<String> videoAdapterInfo = new List<string>();
           ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_VideoController");
            foreach (ManagementObject obj in searcher.Get())
            {
                videoAdapterInfo.Add("Имя видеоадаптера: ");
                videoAdapterInfo.Add(obj["Name"].ToString().Trim());
                videoAdapterInfo.Add("\n");
                videoAdapterInfo.Add("Версия дарайвера: ");
                videoAdapterInfo.Add(obj["DriverVersion"].ToString().Trim());
                videoAdapterInfo.Add("\n");
                videoAdapterInfo.Add("Объем видеопамяти: ");
                Double VideoMemory;
                VideoMemory = (UInt32)obj["AdapterRAM"];
                VideoMemory /= 1073741824;
                videoAdapterInfo.Add(VideoMemory + " GB\n");
                videoAdapterInfo.Add("\n");
            }
            foreach (String str in videoAdapterInfo)
            {
                label3.Text += str;
            }
        }
        void GetBIOSInfo()
        {
            label4.Text = "Информация о BIOS:\n";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BIOS");
            foreach (ManagementObject obj in searcher.Get())
            {
                label4.Text += "Название BIOS: ";
                label4.Text += obj["Name"].ToString().Trim() + "\n";
                label4.Text += "Версия: ";
                label4.Text += obj["Version"].ToString().Trim() + "\n";
                label4.Text += "Производитель: ";
                label4.Text += obj["Manufacturer"].ToString().Trim() + "\n";
            }
        }

        void GetNetworkInfo()
        {
            label5.Text = "Список сетевых интерфейсов:\n\n";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration");
            String netInfo = "";
            foreach (ManagementObject obj in searcher.Get())
            {
                netInfo += "Имя интерфейса: ";
                netInfo += obj["Description"].ToString().Trim() + "\n";
                if (obj["IPAddress"] != null)
                {
                    String[] ipList = (String[])obj["IPAddress"];
                    netInfo += "IP адрес: ";
                    foreach (String str in ipList)
                    {
                        netInfo += str + "\n";
                    }
                }
                else
                {
                    netInfo += "IP адрес: Нет данных\n";
                }
                if (obj["MACAddress"] != null)
                {
                    netInfo += "MAC aдрес: ";
                    netInfo += obj["MACAddress"].ToString().Trim() + "\n";
                }
                else
                {
                    netInfo += "MAC aдрес: Нет информации\n";
                }
                if (obj["DHCPServer"] != null)
                {
                    netInfo += "DHCP сервер: ";
                    netInfo += obj["DHCPServer"].ToString().Trim() + "\n";
                }
                else
                {
                    netInfo += "DHCP сервер: Нет информации\n";
                }

                netInfo += "\n";
            }
            label5.Text += netInfo;
            
        }
        private void CPUMonitor()
        {
            while (true)
            {
                try
                {
                    if (label8.InvokeRequired)
                         label8.Invoke(new Action(() => {
                             ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
                             foreach (ManagementObject obj in searcher.Get())
                             {
                                 label8.Text = obj["LoadPercentage"].ToString();
                                 label8.Text += "%";
                             }           
                         }
                            ));
                        Thread.Sleep(10);
                }
                catch (ThreadAbortException)
                {
                    break;
                }
            }
        }

        private void RAMMonitor()
        {
            while (true)
            {
                try
                {
                    if (label9.InvokeRequired)
                        label9.Invoke(new Action(() =>
                        {
                            PerformanceCounter _ramCounter = new PerformanceCounter("Memory", "Available MBytes");
                            label9.Text =  _ramCounter.NextValue() + "Мб";
                        }
                           ));
                    Thread.Sleep(10);
                }
                catch (ThreadAbortException)
                {
                    break;
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            threadCPU.Abort();
            threadRAM.Abort();
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }
    }
}
