using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Xml.Linq;
using System.Text;
using DeviceConnection;
using System.Threading;

namespace WpfBasicApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> job_xml = new List<string>();
        XmlInfo xmlinfo = new XmlInfo();
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
                    xmlinfo.readingPhase(Get_Folder_of_file(filename) + "\\Job.xml");
                    lbFiles.Items.Add("Reading Mode: " + xmlinfo.ReadingMode);
                    if (xmlinfo.ReadingMode == "PHASE MODE")
                    {
                        lbFiles.Items.Add("    " + xmlinfo.displayPhaseOn(Get_Folder_of_file(filename) + "\\Job.xml") + " - " + xmlinfo.phaseOnEdge);
                        lbFiles.Items.Add("    " + xmlinfo.displayPhaseOff(Get_Folder_of_file(filename) + "\\Job.xml") + " - " + xmlinfo.phaseOffEdge);
                        xmlinfo.ReadOutput(Get_Folder_of_file(filename) + "\\Job.xml");
                        lbFiles.Items.Add("***********************************************************");
                        lbFiles.Items.Add("Output: " + xmlinfo.OutputChannel);
                        lbFiles.Items.Add("    Number of Events: " + xmlinfo.NumOfEvents);
                        lbFiles.Items.Add("    Number of Phases: " + xmlinfo.NumOfPhases);
                        if (xmlinfo.activation != "TCP")
                            lbFiles.Items.Add("    Activation: " + xmlinfo.activation + " - " + xmlinfo.activationEdge);
                        else
                            lbFiles.Items.Add("    Activation: " + xmlinfo.activation + " - Command: " + xmlinfo.activationCommand);
                        if(xmlinfo.deactivation != "TCP")
                            lbFiles.Items.Add("    Deactivation: " + xmlinfo.deactivation + " - " + xmlinfo.deactivationEdge);
                        else
                            lbFiles.Items.Add("    Deactivation: " + xmlinfo.deactivation + " - Command: " + xmlinfo.deactivationCommand);

                    }
                    else if(xmlinfo.ReadingMode == "CONTINUOUS")
                    {
                        xmlinfo.ReadOutput(Get_Folder_of_file(filename) + "\\Job.xml");

                    }
                    generateTC(filename);
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

        public void generateTC(string configuration)
        {

            Form1 form1 = new Form1();
            DeviceInfo deviceInfo = new DeviceInfo();
            string IPAddress = (string)deviceInfo.IP;
            string Port = (string)deviceInfo.Port;

            //string configuration = @"C:\Users\hpham3\Downloads\phase-de-ac-tcp.dlcfg";

            configuration = configuration.ToString();
            ClientConnection clientConnection = new ClientConnection(IPAddress, int.Parse(Port));
            clientConnection.Open();
            bool isTCPClient = true;

            if (!form1.EnterHostMode(clientConnection))
            {
                lbFiles.Items.Add("Enter host mode error");
                return;
            }
            if (form1.SendConfigurationFile(clientConnection, isTCPClient, configuration))
            {
                    lbFiles.Items.Add("**************Send configuration successful***************");
                if (form1.setStartupCfg(clientConnection, isTCPClient, configuration))
                    lbFiles.Items.Add("********************Set startup successful*****************");
                else
                    lbFiles.Items.Add("**********************Set startup fail*********************");
            }
            else
            {
                lbFiles.Items.Add("Send configuration fail");

                // this line make exiting host mode procedure with out error
                string s1 = clientConnection.ReceiveString();
            }

            form1.ExitHostMode(clientConnection);
            lbFiles.Items.Add("************************************************************");

            //numOfPhase = xmlinfo.NumOfPhases;
            //numOfEvent = xmlinfo.NumOfEvents;
            //int.TryParse(numOfEvent, out int numOfEvent_INT);
            lbTC.Items.Add("***TC1:\n     -Active by " + xmlinfo.activation + " one time in phase 1, one time in phase 2\n     -> Expected: " + xmlinfo.OutputChannel + " ON");
            lbTC.Items.Add("***TC2:\n     -Active by " + xmlinfo.activation + " one time in phase 1, one time before phase 2\n     -> Expected: " + xmlinfo.OutputChannel + " ON");
            lbTC.Items.Add("***TC3:\n     -Active by " + xmlinfo.activation + " one time in phase 1, no active in 3 phase\n     -> Expected: " + xmlinfo.OutputChannel + " OFF");
            lbTC.Items.Add("     -Active by " + xmlinfo.activation + " one time\n     -> Expected: " + xmlinfo.OutputChannel + " ON");
            lbTC.Items.Add("***********************************************************");

            lbLog.Items.Add("***TC1:\n     -Active by " + xmlinfo.activation + " one time in phase 1, one time in phase 2\n     -> Actual: " + xmlinfo.OutputChannel + " ?");
            lbLog.Items.Add("***TC2:\n     -Active by " + xmlinfo.activation + " one time in phase 1, one time before phase 2\n     -> Actual: " + xmlinfo.OutputChannel + " ?");
            lbLog.Items.Add("***TC3:\n     -Active by " + xmlinfo.activation + " one time in phase 1, no active in 3 phase\n     -> Actual: " + xmlinfo.OutputChannel + " ?");
            lbLog.Items.Add("     -Active by " + xmlinfo.activation + " one time\n     -> Actual: " + xmlinfo.OutputChannel + " ?");
            lbLog.Items.Add("***********************************************************");


        }
        private void sendconfig()
        {
            lbLog.Items.Add("Sending....");
            //do something
            bool flag = false;
            int i = 0;
            if (flag == false)
            {
                i++;
                if( i == 1000)
                {
                    i = 0;
                    flag = true;
                }
            }
            lbTC.Items.Add("----->Actual result: Failed");
            lbLog.Items.Add("...OK!");



        }
        private void RunTest_Click(object sender, RoutedEventArgs e)

        {
            //List<string> TC = new List<string>();
            //foreach (string job in job_xml)
            //{
            //    lbTC.Items.Add("TC");

            MessageBox.Show("Nothing!");
            
            //}
            //generateTC();
        }
    }

    public class XmlInfo
    {
        public string NumOfPhases;
        public string NumOfEvents;
        public string ReadingMode;
        public string phaseOnEdge;
        public string phaseOffEdge;
        public string activation;
        public string deactivation;
        public string activationEdge;
        public string deactivationEdge;
        public string activationCommand;
        public string deactivationCommand;
        public string outputChannel_Con;
        public string outputCommand_Con;
        public string activeChannel_Con;
        public string activeCommand_Con;

        public string OutputChannel;


        public string phaseOnChannel, phaseOffChannel;
        public string phaseOnCommand, phaseOffCommand;

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

        public void readingPhase(string path)
        {
            var lines = XElement.Load(path);
            var phasemode = checkChannel(lines, "readingphaseon");
            var oneshot = checkChannel(lines, "acquisitiontrigger");
            if ((phasemode == null) & (oneshot == null))
            {
                ReadingMode = "CONTINUOUS";
            }
            else
            {
                if (phasemode == null)
                {
                }
                else if((phasemode.Element("inputevt") != null) | (phasemode.Element("channelevt") != null))
                {
                    ReadingMode = "PHASE MODE";
                }
                if (oneshot == null)
                { }
                else if((oneshot.Element("inputevt") != null) | (oneshot.Element("channelevt") != null))
                {
                    ReadingMode = "ONE SHOT";
                }
            }
;

        }

        public string ReadPhaseOn(string path)
        {

            var INPhaseOn = "";
            var lines = XElement.Load(path);
            var t = checkChannel(lines, "readingphaseon");
            if (t.Element("inputevt") != null)
            {
                INPhaseOn = t.Element("inputevt").Attribute("paramRefKey").Value;
                if (t.Element("inputevt").Element("edge").Value == "0")
                    phaseOnEdge = "Leading";
                else
                    phaseOnEdge = "Trailing";
            }
            else if (t.Element("channelevt") != null)
            {
                INPhaseOn = t.Element("channelevt").Attribute("paramRefKey").Value;
            }

            return INPhaseOn;
        }
        public string displayPhaseOn(string path)
        {
            int InPhaseOn_Integer = 0;
            var InPhaseOn = ReadPhaseOn(path);
            int.TryParse(InPhaseOn, out InPhaseOn_Integer);
            var lines = XElement.Load(path);
            var t = checkChannel(lines, "readingphaseon");
            if (InPhaseOn_Integer == (int)XmlCode.XmlInput1)
            {
                phaseOnChannel = "INPUT1";
                return "PhaseON INPUT1";
            }
            else if (InPhaseOn_Integer == (int)XmlCode.XmlInput2)
            {
                phaseOnChannel = "INPUT2";
                return "PhaseON INPUT2";
            }
            else if (InPhaseOn_Integer == (int)XmlCode.XmlTcp)
            {
                phaseOnChannel = "TCP";
                phaseOnCommand = t.Element("channelevt").Element("triggerstring").Value;
                phaseOnCommand = Base64Decode(phaseOnCommand);
                return "PhaseON TCP: " + phaseOnCommand;
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
                if (t.Element("inputevt").Element("edge").Value == "0")
                    phaseOffEdge = "Leading";
                else
                    phaseOffEdge = "Trailing";
            }
            else if (t.Element("channelevt") != null)
            {
                INPhaseOff = t.Element("channelevt").Attribute("paramRefKey").Value;
            }
            return INPhaseOff;
        }
        public string displayPhaseOff(string path)
        {
            int InPhaseOff_Integer = 0;
            var InPhaseOff = ReadPhaseOff(path);
            int.TryParse(InPhaseOff, out InPhaseOff_Integer);
            var lines = XElement.Load(path);
            var t = checkChannel(lines, "readingphaseoff");
            if (InPhaseOff_Integer == (int)XmlCode.XmlInput1)
            {
                phaseOffChannel = "INPUT1";
                return "PhaseOFF INPUT1";
            }
            else if (InPhaseOff_Integer == (int)XmlCode.XmlInput2)
            {
                phaseOffChannel = "INPUT2";
                return "PhaseOFF INPUT2";
            }
            else if (InPhaseOff_Integer == (int)XmlCode.XmlTcp)
            {
                phaseOffChannel = "TCP";
                phaseOffCommand = t.Element("channelevt").Element("triggerstring").Value;
                phaseOffCommand = Base64Decode(phaseOffCommand);
                return "PhaseOFF TCP: " + phaseOffCommand;
            }
            else
                return "PhaseOFF NONE";


        }
        public string ReadOutput(string path)
        {
            List<string> Output = new List<string>();
            List<string> ContinuousOut = new List<string>();
            var lines = XElement.Load(path);
            var t = lines.Descendants("outputinstance");
            foreach (var element in t)
            {
                if ((element.Element("risingedgeevent").Element("inputevt") != null) | (element.Element("risingedgeevent").Element("channelevt") != null))
                {
                    if(ReadingMode == "PHASE MODE")
                    {
                        if (element.Element("risingedgeevent").Element("inputevt") != null)
                        {
                            var a = element.Element("risingedgeevent").Element("inputevt");
                            if (a.Attribute("paramRefKey").Value == "440")
                                activation = "INPUT1";
                            else
                                activation = "INPUT2";
                            if (a.Element("edge").Value == "0")
                                activationEdge = "Leading";
                            else
                                activationEdge = "Trailing";
                        }
                        else
                        {
                            var a = element.Element("risingedgeevent").Element("channelevt");
                            activation = "TCP";
                            activationCommand = a.Element("triggerstring").Value;
                            activationCommand = Base64Decode(activationCommand);
                        }
                        Output.Add(element.Element("outputname").Value);
                        OutputChannel = element.Element("outputname").Value;
                        Output.Add(element.Element("numberofevents").Value);
                        NumOfEvents = element.Element("numberofevents").Value;
                        Output.Add(element.Element("numberofphases").Value);
                        NumOfPhases = element.Element("numberofphases").Value;
                    }
                    else if (ReadingMode == "ONE SHOT")
                    {

                    }
                    else
                    {
                        if (element.Element("risingedgeevent").Element("inputevt") != null)
                        {
                            var a = element.Element("risingedgeevent").Descendants("inputevt");
                            activeChannel_Con = "INPUT";
                            foreach (var b in a)
                            {
                                if (b.Attribute("paramRefKey").Value == "440")
                                    ContinuousOut.Add("INPUT1");
                                else
                                    ContinuousOut.Add("INPUT2");
                                if (b.Element("edge").Value == "0")
                                    ContinuousOut.Add("Leading");
                                else
                                    ContinuousOut.Add("Trailing");

                            }
                        }
                        else
                        {
                            var a = element.Element("risingedgeevent").Element("channelevt");
                            activeChannel_Con = "TCP";
                            activeCommand_Con = a.Element("triggerstring").Value;
                            activeCommand_Con = Base64Decode(activationCommand);
                        }
                        outputChannel_Con = element.Element("outputname").Value;
                        Output.Add(element.Element("numberofevents").Value);
                        NumOfEvents = element.Element("numberofevents").Value;
                    }

                }
                if ((element.Element("fallingedgeevent").Element("inputevt") != null) | (element.Element("fallingedgeevent").Element("channelevt") != null))
                {
                    if (ReadingMode == "PHASE MODE")
                    {
                        if (element.Element("fallingedgeevent").Element("inputevt") != null)
                        {
                            var a = element.Element("fallingedgeevent").Element("inputevt");
                            if (a.Attribute("paramRefKey").Value == "440")
                                deactivation = "INPUT1";
                            else
                                deactivation = "INPUT2";
                            if (a.Element("edge").Value == "0")
                                deactivationEdge = "Leading";
                            else
                                deactivationEdge = "Trailing";
                        }
                        else
                        {
                            var a = element.Element("fallingedgeevent").Element("channelevt");
                            deactivation = "TCP";
                            deactivationCommand = a.Element("triggerstring").Value;
                            deactivationCommand = Base64Decode(deactivationCommand);
                        }
                    }
                    else if (ReadingMode == "ONE SHOT")
                    {

                    }
                    else
                    {
                        if (element.Element("fallingedgeevent").Element("inputevt") != null)
                        {
                            var a = element.Element("fallingedgeevent").Descendants("inputevt");
                            activeChannel_Con = "INPUT";
                            foreach (var b in a)
                            {
                                if (b.Attribute("paramRefKey").Value == "440")
                                    ContinuousOut.Add("INPUT1");
                                else
                                    ContinuousOut.Add("INPUT2");
                                if (b.Element("edge").Value == "0")
                                    ContinuousOut.Add("Leading");
                                else
                                    ContinuousOut.Add("Trailing");

                            }
                        }
                        else
                        {
                            var a = element.Element("fallingedgeevent").Element("channelevt");
                            activeChannel_Con = "TCP";
                            activeCommand_Con = a.Element("triggerstring").Value;
                            activeCommand_Con = Base64Decode(activationCommand);
                        }
                        outputChannel_Con = element.Element("outputname").Value;
                        Output.Add(element.Element("numberofevents").Value);
                        NumOfEvents = element.Element("numberofevents").Value;

                    }

                }
            }
            return string.Concat(Output);
            //return Output.ToString();
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

    }

    public class Form1
    {

        public StreamWriter logger = null;
        public static string messageError;
        public bool EnterHostMode(ClientConnection tcpclient)
        {
            Byte[] message1, message2, message3, receivemessage, expectmessage;
            message1 = new Byte[] { 0x1B, 0x5B, 0x43, 0x0D };
            message2 = new Byte[] { 0x1B, 0x5B, 0x42, 0x0D };
            tcpclient.SendBytes(message1);
            Thread.Sleep(3000);
            receivemessage = tcpclient.ReceiveBytes();
            expectmessage = new Byte[] { 0x1B, 0x48, 0x0D, 0x0A };
            if (receivemessage == null) { MessageBox.Show("Can not enter host mode"); tcpclient.Close(); return false; }
            for (int i = 0; i < receivemessage.Length; i++)
            {
                if (receivemessage[i] == expectmessage[i]) { }
                else
                {
                    return false;
                }
            }
            tcpclient.SendBytes(message2);
            Thread.Sleep(3000);
            receivemessage = tcpclient.ReceiveBytes();
            expectmessage = new Byte[] { 0x1B, 0x53, 0x0D, 0x0A };
            for (int i = 0; i < receivemessage.Length; i++)
            {
                if (receivemessage[i] == expectmessage[i]) { }
                else
                {
                    return false;
                }
            }
            return true;
        }
        public void ExitHostMode(ClientConnection tcpclient)
        {
            Byte[] message3, receivemessage, expectmessage;

            message3 = new Byte[] { 0x1B, 0x5B, 0x41, 0x0D };
            tcpclient.SendBytes(message3);
            Thread.Sleep(3000);
            receivemessage = tcpclient.ReceiveBytes();
            expectmessage = new Byte[] { 0x1B, 0x5B, 0x58 };
            if (receivemessage == null) { MessageBox.Show("Can not exit host mode"); return; }

            for (int i = 0; i < receivemessage.Length; i++)
            {
                if (receivemessage[i] == expectmessage[i]) { }
                else
                {
                    MessageBox.Show("Exit host mode error!");
                    return;
                }
            }
            tcpclient.Close();
        }
        public bool SendConfigurationFile(ClientConnection tcpclient, bool isTCPClient, string ConfigurationFilePath)
        {
            messageError = null;
            bool rc = false;
            Byte[] streamtobesent, commandtobesent;

            // syntax: SEND_CFG<space><config_name><LF>
            string configname = Path.GetFileNameWithoutExtension(ConfigurationFilePath);
            string command = "SEND_CFG " + configname;
            Byte[] message = Encoding.UTF8.GetBytes(command);
            commandtobesent = new Byte[message.Length + 1];
            System.Array.Copy(message, commandtobesent, message.Length);
            commandtobesent[commandtobesent.Length - 1] = 0x0A;

            Byte[] filebin;
            if (File.Exists(ConfigurationFilePath) == false)
            {
                MessageBox.Show("File " + ConfigurationFilePath + "is not existing");
                return false;
            }

            try
            {
                filebin = File.ReadAllBytes(ConfigurationFilePath);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }

            Int32 streamlen = filebin.Length + 4; // file + CRC

            // pre-append 4 bytes for the file length
            streamtobesent = new Byte[filebin.Length + 8];
            System.Array.Copy(filebin, 0, streamtobesent, 4, filebin.Length);
            byte[] bytes = BitConverter.GetBytes(streamlen);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            System.Array.Copy(bytes, 0, streamtobesent, 0, 4);

            // calculate CRC32 checksum
            byte[] checksum = CalculateCRC32(streamtobesent, streamtobesent.Length - 4);

            // post-append 4 bytes for the CRC
            System.Array.Copy(checksum, 0, streamtobesent, streamtobesent.Length - 4, 4);

            if (isTCPClient)
                rc = tcpclient.SendBytes(commandtobesent);
            logBytes(commandtobesent, true);
            if (rc == false)
            {
                logString("Failed to send message");
                return rc;
            }

            if (isTCPClient)
                rc = tcpclient.SendBytes(streamtobesent);
            logBytes(streamtobesent, true);
            if (rc == false)
            {
                logString("Failed to send message");
                return rc;
            }
            // receive ACK / NACK response
            if (isTCPClient)
                message = tcpclient.ReceiveBytes();


            if (message == null || message.Length == 0)
                return false;

            return true;
        }
        public bool setStartupCfg(ClientConnection tcpclient, bool isTCPClient, string ConfigurationFilePath)
        {
            bool rc = false;
            Byte[] commandtobesent;
            string configname = Path.GetFileNameWithoutExtension(ConfigurationFilePath);
            string command = "CHANGE_CFG " + configname;
            Byte[] message = Encoding.UTF8.GetBytes(command);
            commandtobesent = new Byte[message.Length + 1];
            System.Array.Copy(message, commandtobesent, message.Length);
            commandtobesent[commandtobesent.Length - 1] = 0x0A;
            if (isTCPClient)
                rc = tcpclient.SendBytes(commandtobesent);
            logBytes(commandtobesent, true);
            if (rc == false)
            {
                logString("Failed to send message");
                return rc;
            }
            if (isTCPClient)
                message = tcpclient.ReceiveBytes();


            if (message == null || message.Length == 0)
                return false;

            return true;

            // syntax: SEND_CFG<space><config_name><LF>

        }
        public static readonly UInt32[] CRCTable =
 {
            0x00000000, 0x77073096, 0xee0e612c, 0x990951ba, 0x076dc419,
            0x706af48f, 0xe963a535, 0x9e6495a3, 0x0edb8832, 0x79dcb8a4,
            0xe0d5e91e, 0x97d2d988, 0x09b64c2b, 0x7eb17cbd, 0xe7b82d07,
            0x90bf1d91, 0x1db71064, 0x6ab020f2, 0xf3b97148, 0x84be41de,
            0x1adad47d, 0x6ddde4eb, 0xf4d4b551, 0x83d385c7, 0x136c9856,
            0x646ba8c0, 0xfd62f97a, 0x8a65c9ec, 0x14015c4f, 0x63066cd9,
            0xfa0f3d63, 0x8d080df5, 0x3b6e20c8, 0x4c69105e, 0xd56041e4,
            0xa2677172, 0x3c03e4d1, 0x4b04d447, 0xd20d85fd, 0xa50ab56b,
            0x35b5a8fa, 0x42b2986c, 0xdbbbc9d6, 0xacbcf940, 0x32d86ce3,
            0x45df5c75, 0xdcd60dcf, 0xabd13d59, 0x26d930ac, 0x51de003a,
            0xc8d75180, 0xbfd06116, 0x21b4f4b5, 0x56b3c423, 0xcfba9599,
            0xb8bda50f, 0x2802b89e, 0x5f058808, 0xc60cd9b2, 0xb10be924,
            0x2f6f7c87, 0x58684c11, 0xc1611dab, 0xb6662d3d, 0x76dc4190,
            0x01db7106, 0x98d220bc, 0xefd5102a, 0x71b18589, 0x06b6b51f,
            0x9fbfe4a5, 0xe8b8d433, 0x7807c9a2, 0x0f00f934, 0x9609a88e,
            0xe10e9818, 0x7f6a0dbb, 0x086d3d2d, 0x91646c97, 0xe6635c01,
            0x6b6b51f4, 0x1c6c6162, 0x856530d8, 0xf262004e, 0x6c0695ed,
            0x1b01a57b, 0x8208f4c1, 0xf50fc457, 0x65b0d9c6, 0x12b7e950,
            0x8bbeb8ea, 0xfcb9887c, 0x62dd1ddf, 0x15da2d49, 0x8cd37cf3,
            0xfbd44c65, 0x4db26158, 0x3ab551ce, 0xa3bc0074, 0xd4bb30e2,
            0x4adfa541, 0x3dd895d7, 0xa4d1c46d, 0xd3d6f4fb, 0x4369e96a,
            0x346ed9fc, 0xad678846, 0xda60b8d0, 0x44042d73, 0x33031de5,
            0xaa0a4c5f, 0xdd0d7cc9, 0x5005713c, 0x270241aa, 0xbe0b1010,
            0xc90c2086, 0x5768b525, 0x206f85b3, 0xb966d409, 0xce61e49f,
            0x5edef90e, 0x29d9c998, 0xb0d09822, 0xc7d7a8b4, 0x59b33d17,
            0x2eb40d81, 0xb7bd5c3b, 0xc0ba6cad, 0xedb88320, 0x9abfb3b6,
            0x03b6e20c, 0x74b1d29a, 0xead54739, 0x9dd277af, 0x04db2615,
            0x73dc1683, 0xe3630b12, 0x94643b84, 0x0d6d6a3e, 0x7a6a5aa8,
            0xe40ecf0b, 0x9309ff9d, 0x0a00ae27, 0x7d079eb1, 0xf00f9344,
            0x8708a3d2, 0x1e01f268, 0x6906c2fe, 0xf762575d, 0x806567cb,
            0x196c3671, 0x6e6b06e7, 0xfed41b76, 0x89d32be0, 0x10da7a5a,
            0x67dd4acc, 0xf9b9df6f, 0x8ebeeff9, 0x17b7be43, 0x60b08ed5,
            0xd6d6a3e8, 0xa1d1937e, 0x38d8c2c4, 0x4fdff252, 0xd1bb67f1,
            0xa6bc5767, 0x3fb506dd, 0x48b2364b, 0xd80d2bda, 0xaf0a1b4c,
            0x36034af6, 0x41047a60, 0xdf60efc3, 0xa867df55, 0x316e8eef,
            0x4669be79, 0xcb61b38c, 0xbc66831a, 0x256fd2a0, 0x5268e236,
            0xcc0c7795, 0xbb0b4703, 0x220216b9, 0x5505262f, 0xc5ba3bbe,
            0xb2bd0b28, 0x2bb45a92, 0x5cb36a04, 0xc2d7ffa7, 0xb5d0cf31,
            0x2cd99e8b, 0x5bdeae1d, 0x9b64c2b0, 0xec63f226, 0x756aa39c,
            0x026d930a, 0x9c0906a9, 0xeb0e363f, 0x72076785, 0x05005713,
            0x95bf4a82, 0xe2b87a14, 0x7bb12bae, 0x0cb61b38, 0x92d28e9b,
            0xe5d5be0d, 0x7cdcefb7, 0x0bdbdf21, 0x86d3d2d4, 0xf1d4e242,
            0x68ddb3f8, 0x1fda836e, 0x81be16cd, 0xf6b9265b, 0x6fb077e1,
            0x18b74777, 0x88085ae6, 0xff0f6a70, 0x66063bca, 0x11010b5c,
            0x8f659eff, 0xf862ae69, 0x616bffd3, 0x166ccf45, 0xa00ae278,
            0xd70dd2ee, 0x4e048354, 0x3903b3c2, 0xa7672661, 0xd06016f7,
            0x4969474d, 0x3e6e77db, 0xaed16a4a, 0xd9d65adc, 0x40df0b66,
            0x37d83bf0, 0xa9bcae53, 0xdebb9ec5, 0x47b2cf7f, 0x30b5ffe9,
            0xbdbdf21c, 0xcabac28a, 0x53b39330, 0x24b4a3a6, 0xbad03605,
            0xcdd70693, 0x54de5729, 0x23d967bf, 0xb3667a2e, 0xc4614ab8,
            0x5d681b02, 0x2a6f2b94, 0xb40bbe37, 0xc30c8ea1, 0x5a05df1b,
            0x2d02ef8d
        };
        public static byte[] CalculateCRC32(byte[] Value, Int32 length)
        {
            UInt32 CRCVal = 0xffffffff;
            Int32 len = length;
            if (Value.Length < len)
                len = Value.Length;

            for (int i = 0; i < len; i++)
            {
                CRCVal = (CRCVal >> 8) ^ CRCTable[(CRCVal & 0xff) ^ Value[i]];
            }
            CRCVal ^= 0xffffffff; // Toggle operation
            byte[] Result = new byte[4];

            Result[0] = (byte)(CRCVal >> 24);
            Result[1] = (byte)(CRCVal >> 16);
            Result[2] = (byte)(CRCVal >> 8);
            Result[3] = (byte)(CRCVal);

            return Result;
        }
        public const int maxloglength = 60;

        public void logBytes(Byte[] logstring, bool sending)
        {
            //dummy function
        }

        public void logString(string logstring)
        {
            // dummy function
        }

    }

}
