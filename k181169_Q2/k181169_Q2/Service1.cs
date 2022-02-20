using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace k181169_Q2
{
    public partial class Service1 : ServiceBase
    {
        List<Feed> feeds = new List<Feed>();
        //List<Feed> Sorted = new List<Feed>();
        Timer time;

        public Service1()
        {
            InitializeComponent();
            time = new Timer();
            time.Interval = 5*60000;
            time.Elapsed += new System.Timers.ElapsedEventHandler(PopulateRssFeed);
        }

        protected override void OnStart(string[] args)
        {
            time.Enabled = true;
        }
        public void OnDebug()
        {
            OnStart(null);
        }
        protected override void OnStop()
        {
            time.Enabled = false;
        }
          
        public void SaveInXML()
        {
            string newfile = ConfigurationManager.AppSettings["Path1"];

            if (File.Exists(newfile))
            {
                File.Delete(newfile);
            }
 
            foreach (Feed x in feeds)
            {
                //TextWriter tw = new StreamWriter(@"D:\afsds.txt", append: true);
                //tw.WriteLine(x.PubDate + "      => " + x.datetosort);
                //tw.Close();
                if (!File.Exists(newfile))
                {
                    XDocument doc = new XDocument(
                        new XDeclaration("1.0", "gb2312", string.Empty),
                        new XElement("NewsItems",
                            new XElement("NewsItem",
                                new XElement("Title", x.Title),
                                new XElement("Description", x.Description),
                                new XElement("PublishedDate", x.PubDate),
                                new XElement("NewsChannel", x.Channelname))
                                ));
                    doc.Save(newfile);
                }
                else
                {
                    XDocument doc = XDocument.Load(newfile);
                    XElement News = doc.Element("NewsItems");
                    News.Add(new XElement("NewsItem",
                                new XElement("Title", x.Title),
                                new XElement("Description", x.Description),
                                new XElement("PublishedDate", x.PubDate),
                                new XElement("NewsChannel", x.Channelname))
                                );

                    doc.Save(newfile);
                }
            }
           
        }


        private void PopulateRssFeed(object sender, System.Timers.ElapsedEventArgs e)
        {
            string[] RssFeedUrl = { "https://www.geo.tv/rss/1/2", "https://www.thenews.com.pk/rss/1/3" };
            
            try
            {
                foreach(var url in RssFeedUrl)
                {

                    XDocument xDoc = new XDocument();
                    xDoc = XDocument.Load(url);
                    var items = (from x in xDoc.Descendants("item")
                                 select new
                                 {
                                     title = x.Element("title").Value,
                                     channelname = x.Element("guid").Value,
                                     pubDate = x.Element("pubDate").Value,
                                     description = x.Element("description").Value
                                 });
                    if (items != null)
                    {
                        foreach (var i in items)
                        {
                            Feed f = new Feed
                            {
                                Title = i.title,
                                Channelname = i.channelname,
                                PubDate = i.pubDate,
                                Description = i.description
                            };
                            string[] temp = f.PubDate.Split(' ');
                            string tempdate = temp[1]+ "/" + temp[2] + "/" + temp[3] + " " + temp[4];
                            f.datetosort = DateTime.ParseExact(tempdate,"dd/MMM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                            feeds.Add(f);
                        }

                    }
                      
                }
                
                feeds.Sort((x, y) => DateTime.Compare(y.datetosort, x.datetosort));
                //feeds.Sort((x, y) => y.datetosort.CompareTo(x.datetosort));
                //Sorted = feeds.OrderBy(o => o.datetosort).ToList();
                SaveInXML();
                feeds.Clear();
            }
            catch (Exception ex)
            {
                string errorfile = ConfigurationManager.AppSettings["errorpath"];
                TextWriter tw = new StreamWriter(errorfile);
                tw.WriteLine("Something wrong." + ex);
                tw.Close();
            }
        }


    }
}
