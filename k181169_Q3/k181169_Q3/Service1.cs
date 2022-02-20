using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Timers;

namespace k181169_Q3
{
    public partial class Service1 : ServiceBase
    {
        Timer time;
        string folder1_path = ConfigurationManager.AppSettings["Path1"];
        string folder2_path = ConfigurationManager.AppSettings["Path2"];
        List<string> source = new List<string>();
        List<string> dest = new List<string>();
        int timewait = 1;
        int change = 1;

        public Service1()
        {
            InitializeComponent();
            time = new Timer();
            time.Interval = 15000;
            time.Elapsed += new System.Timers.ElapsedEventHandler(start);
        }

        public void OnDebug()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            time.Enabled = true;
        }

        public void start(object sender, System.Timers.ElapsedEventArgs e)
        {
            FilesReader(folder1_path, source);
            FilesReader(folder2_path, dest);
            CopyToDest();
            CheckTimer();
        }

        public void CopyToDest()
        {
            foreach (string file in source)
            {
                if(!File.Exists(Path.Combine(folder2_path, file)))
                {
                    File.Copy(Path.Combine(folder1_path,file), Path.Combine(folder2_path, file));
                    change++;
                }
            }
            foreach (string file in dest)
            {
                if (!File.Exists(Path.Combine(folder1_path,file)))
                {
                    File.Delete(Path.Combine(folder2_path, file));
                    change++;
                }
            }
            foreach (string file in source)
            {
                foreach (string file1 in dest)
                {
                    if (file == file1)
                    {
                        if ( System.IO.File.GetLastWriteTime(Path.Combine(folder1_path,file))  != System.IO.File.GetLastWriteTime(Path.Combine(folder2_path, file1)))
                        {
                            File.Copy(Path.Combine(folder1_path, file), Path.Combine(folder2_path, file1),true);
                            change++;
                        }
                    }
                }
                
            }

        }

        public void CheckTimer()
        {
            if (timewait < 59)
            {
                if (change != 1)
                {
                    timewait = timewait + 2;
                    time.Interval = timewait * 60000;
                }
                else
                {
                    change = 1;
                    time.Interval = 60000;
                }
            }

        }

        public void FilesReader(string path, List<string> filelist)
        {
            string folderPath = path;

            string[] files = Directory.GetFiles(folderPath);
            if (files.Length > 0)
            {
                foreach (var i in files)
                {
                    filelist.Add(Path.GetFileName(i));
                }
            }
            
        }

        protected override void OnStop()
        {
            time.Enabled = false;
        }
    }
}
