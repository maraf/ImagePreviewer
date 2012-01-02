using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DesktopCore;
using System.IO.IsolatedStorage;
using System.Resources;
using System.IO;
using System.Xml;

namespace ImagePreviewer
{
    public enum IndexBy
    {
        Guid, Number
    }

    public class Manager : NotifyPropertyChanged
    {
        public static readonly string DefaultLocation = "ImagePreviewer.xml";

        private Images images = new Images();

        private ObservableCollection<string> formats = new ObservableCollection<string>();
        private string currentFormat;
        private string exampleFormat = "http://media1.rozzlobenimuzi.com/privileg/belza/images/picicky/heather_vandeven_08/{0:D3}.jpg";
        private int startIndex;
        private int endIndex;

        private string baseFileName;
        private IndexBy indexBy = IndexBy.Guid;
        private string selectedPath;

        public Images Images
        {
            get { return images; }
            set
            {
                images = value;
                FirePropertyChanged("Images");
            }
        }

        public ObservableCollection<string> Formats
        {
            get { return formats; }
            set
            {
                formats = value;
                FirePropertyChanged("Formats");
            }
        }

        public string CurrentFormat
        {
            get { return currentFormat; }
            set
            {
                currentFormat = value;
                FirePropertyChanged("CurrentFormat");
            }
        }

        public string ExampleFormat
        {
            get { return exampleFormat; }
            set
            {
                exampleFormat = value;
                FirePropertyChanged("ExampleFormat");
            }
        }

        public int StartIndex
        {
            get { return startIndex; }
            set
            {
                startIndex = value;
                FirePropertyChanged("StartIndex");
            }
        }

        public int EndIndex
        {
            get { return endIndex; }
            set
            {
                endIndex = value;
                FirePropertyChanged("EndIndex");
            }
        }

        public string BaseFileName
        {
            get { return baseFileName; }
            set
            {
                baseFileName = value;
                FirePropertyChanged("BaseFileName");
            }
        }

        public IndexBy IndexBy
        {
            get { return indexBy; }
            set
            {
                indexBy = value;
                FirePropertyChanged("IndexBy");
            }
        }

        public string SelectedPath
        {
            get { return selectedPath; }
            set
            {
                selectedPath = value;
                FirePropertyChanged("SelectedPath");
            }
        }

        public void Load(string location)
        {
            IsolatedStorageFile f = IsolatedStorageFile.GetUserStoreForAssembly();
            if (f.FileExists(location))
            {
                try
                {
                    using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(location, FileMode.OpenOrCreate, f))
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(sr.ReadToEnd());

                        XmlElement fmts = doc.GetElementsByTagName("Formats")[0] as XmlElement;
                        if (fmts != null)
                        {
                            int index;
                            if (Int32.TryParse(GetAttributeValue(fmts, "StartIndex"), out index))
                            {
                                StartIndex = index;
                            }
                            if (Int32.TryParse(GetAttributeValue(fmts, "EndIndex"), out index))
                            {
                                EndIndex = index;
                            }
                            ExampleFormat = GetAttributeValue(fmts, "Example");
                            CurrentFormat = GetAttributeValue(fmts, "Current");
                            foreach (XmlElement fmt in fmts.GetElementsByTagName("Format"))
                            {
                                Formats.Add(fmt.InnerText);
                            }
                        }
                    }
                }
                catch (Exception e) { }
            }
        }

        public void Save(string location)
        {
            IsolatedStorageFile f = IsolatedStorageFile.GetUserStoreForAssembly();
            using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(location, FileMode.Create, f))
            {
                XmlDocument doc = new XmlDocument();
                XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                doc.AppendChild(dec);

                XmlElement root = doc.CreateElement("ImagePreviewer");
                doc.AppendChild(root);

                XmlElement conf = doc.CreateElement("Configuration");
                root.AppendChild(conf);

                XmlElement fmts = doc.CreateElement("Formats");
                root.AppendChild(fmts);

                XmlAttribute att = doc.CreateAttribute("Current");
                att.Value = CurrentFormat;
                fmts.Attributes.Append(att);

                att = doc.CreateAttribute("Example");
                att.Value = ExampleFormat;
                fmts.Attributes.Append(att);

                att = doc.CreateAttribute("StartIndex");
                att.Value = StartIndex.ToString();
                fmts.Attributes.Append(att);

                att = doc.CreateAttribute("EndIndex");
                att.Value = EndIndex.ToString();
                fmts.Attributes.Append(att);

                foreach (string item in formats)
                {
                    XmlElement fmt = doc.CreateElement("Format");
                    fmt.InnerText = item;
                    fmts.AppendChild(fmt);
                }

                doc.Save(stream);
            }
        }

        private string GetAttributeValue(XmlElement el, string name)
        {
            if (el.Attributes[name] != null && !String.IsNullOrEmpty(el.Attributes[name].Value))
                return el.Attributes[name].Value;

            return null;
        }
    }
}
