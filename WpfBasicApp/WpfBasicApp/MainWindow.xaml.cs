using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Xml.Linq;

namespace WpfBasicApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> job_xml = new List<string>();
        public MainWindow()
        {
            InitializeComponent();
        
        }

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Configuration (*.dlcfg)|*.dlcfg|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filename in openFileDialog.FileNames)
                {
                    lbFiles.Items.Add(filename);
                    unzipfile(filename, Get_Folder_of_file(filename));
                    job_xml.Add(Get_Folder_of_file(filename) + "\\Job.xml");
                    XmlInfo xmlinfo = new XmlInfo();


                    lbFiles.Items.Add(xmlinfo.displayPhaseOn(Get_Folder_of_file(filename) + "\\Job.xml"));
                    lbFiles.Items.Add(xmlinfo.displayPhaseOff(Get_Folder_of_file(filename) + "\\Job.xml"));
                    lbFiles.Items.Add(xmlinfo.ReadOutput(Get_Folder_of_file(filename) + "\\Job.xml"));
                }
            }



        }
        private string Get_Folder_of_file(string filePath)
        {
            string filename = Path.GetFileNameWithoutExtension(filePath);
            return filePath.Replace(Path.GetFileName(filePath), "") + filename;
        }
        private void unzipfile(string zipFile, string folderUnZip)
        {
            ZipFile.ExtractToDirectory(zipFile, folderUnZip, overwriteFiles: true);
        }

        public void generateTC()
        {
            XmlInfo xmlinfo = new XmlInfo();
            string output = "";
            char numOfPhase, numOfEvent;

            for (int i = 0; i < job_xml.Count; i++)
            {
                output = xmlinfo.ReadOutput(job_xml[i]);
                int t = 0;
                numOfPhase = output[7];
                numOfEvent = output[8];
                lbTC.Items.Add("1.Trigger " + numOfEvent + " events in " + numOfPhase + " phases.");
                lbTC.Items.Add("Expected result: Output1 is activated after " + numOfEvent + " event ");
                sendconfig();
                lbTC.Items.Add("2.Trigger event 1 time, then acive " + numOfPhase + " phases, trigger"  +
                lbTC.Items.Add("Expected result: Output1 is not activated");


            }
        }
        private void sendconfig()
        {
            lbLog.Items.Add("Sending....");
            lbLog.Items.Add("...OK!");

        }
        private void RunTest_Click(object sender, RoutedEventArgs e)

        {
            //List<string> TC = new List<string>();
            //foreach (string job in job_xml)
            //{
            //    lbTC.Items.Add("TC");


            //}
            generateTC();
        }
    }

    public class XmlInfo
    {
        string NumOfPhases;
        string NumOfEvents;
        string Input;
        string Output;
        string XmlPath;


        string phaseOnChannel, phaseOffChannel;
        string phaseOnCommand, phaseOffCommand;

        public XElement checkChannel(XElement data, string channel)
        {
            var elements = data.Descendants(channel);
            string[] xmlChannels = { "channelevt", "inputevt" };
            XElement xmlChannel = null;
            foreach (var element in elements)
            {
                foreach (string c in xmlChannels)
                {
                    if (element.Element(c) == null)
                    {
                        continue;
                    }
                    else
                    {
                        xmlChannel = element;
                        break;

                    }
                }
                if (xmlChannel != null)
                {
                    break;
                }
            }
            return xmlChannel;
        }


        public string ReadPhaseOn(string path)
        {

            var INPhaseOn = "";
            var lines = XElement.Load(path);
            var t = checkChannel(lines, "readingphaseon");
            if (t.Element("inputevt") != null)
            {
                INPhaseOn = t.Element("inputevt").Attribute("paramRefKey").Value;
            }
            else if (t.Element("channelevt") != null)
            {
                INPhaseOn = t.Element("channelevt").Attribute("paramRefKey").Value;
            }

            return INPhaseOn;
        }
        public string displayPhaseOn(string path)
        {
            var PhaseOnCommand = "";
            int InPhaseOn_Integer = 0;
            var InPhaseOn = ReadPhaseOn(path);
            int.TryParse(InPhaseOn, out InPhaseOn_Integer);
            var lines = XElement.Load(path);
            var t = checkChannel(lines, "readingphaseon");
            if (InPhaseOn_Integer == (int)XmlCode.XmlInput1)
                return "PhaseON INPUT1";
            else if (InPhaseOn_Integer == (int)XmlCode.XmlInput2)
                return "PhaseON INPUT2";
            else if (InPhaseOn_Integer == (int)XmlCode.XmlTcp)
            {
                PhaseOnCommand = t.Element("channelevt").Element("triggerstring").Value;
                PhaseOnCommand = Base64Decode(PhaseOnCommand);
                return "PhaseON TCP:" + PhaseOnCommand;
            }
            return "PhaseON NONE";

        }
        public string ReadPhaseOff(string path)
        {
            var INPhaseOff = "";
            var lines = XElement.Load(path);
            var t = checkChannel(lines, "readingphaseoff");
            if (t.Element("inputevt") != null)
            {
                INPhaseOff = t.Element("inputevt").Attribute("paramRefKey").Value;
            }
            else if (t.Element("channelevt") != null)
            {
                INPhaseOff = t.Element("channelevt").Attribute("paramRefKey").Value;
            }
            return INPhaseOff;
        }
        public string displayPhaseOff(string path)
        {
            var PhaseOffCommand = "";
            int InPhaseOff_Integer = 0;
            var InPhaseOff = ReadPhaseOff(path);
            int.TryParse(InPhaseOff, out InPhaseOff_Integer);
            var lines = XElement.Load(path);
            var t = checkChannel(lines, "readingphaseoff");
            if (InPhaseOff_Integer == (int)XmlCode.XmlInput1)
                return "PhaseOFF INPUT1";
            else if (InPhaseOff_Integer == (int)XmlCode.XmlInput2)
                return "PhaseOFF INPUT2";
            else if (InPhaseOff_Integer == (int)XmlCode.XmlTcp)
            {
                PhaseOffCommand = t.Element("channelevt").Element("triggerstring").Value;
                PhaseOffCommand = Base64Decode(PhaseOffCommand);
                return "PhaseOFF TCP:" + PhaseOffCommand;
            }
            else
                return "PhaseOFF NONE";
            //switch(generalTab)
            //         {
            //	case "readingPhaseOn":
            //	case "readingPhaseOff":

            //         }

        }
        public string ReadOutput(string path)
        {
            List<string> Output = new List<string>();
            var lines = XElement.Load(path);
            var t = lines.Descendants("outputinstance");
            foreach (var element in t)
            {
                Output.Add(element.Element("outputname").Value);
                Output.Add(element.Element("numberofevents").Value);
                Output.Add(element.Element("numberofphases").Value);
            }
            return string.Concat(Output);
            //return ;
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

    }



}
