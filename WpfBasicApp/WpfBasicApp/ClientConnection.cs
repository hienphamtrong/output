using System;
using System.Text;
using System.Net.Sockets;



namespace DeviceConnection
{
    
    public class ClientConnection
    {
        public string name;
        private TcpClient tcpClient;
        private NetworkStream tcpStream;
        private Int32 peerPort;
        private string peerAddr;
        public string OpenEX, SendStringEX, SendByEx, ReceipString, ReceipByte;
        // constructor
        public ClientConnection(string ipAddr, Int32 port)
        {
            peerPort = port;
            peerAddr = ipAddr;
        }
        /// <summary>
        /// function return true if everything is ok, otherwise return false
        /// </summary>
        /// <returns></returns>
        public bool Open()
        {
            bool rc = true;
            if (tcpClient == null)
                tcpClient = new TcpClient();
            int count = 0;
            while (tcpClient.Connected == false && count < 3)
            {
                count++;
                try
                {
                    tcpClient.Connect(peerAddr, peerPort);
                }
                catch (System.Exception ex)
                {
                    OpenEX = ex.ToString();
                }
                if (tcpClient == null)
                {
                    rc = false;
                    break;
                }
                else if (tcpClient.Connected)
                {
                    break;
                }
                System.Threading.Thread.Sleep(500);
            }
            if (tcpClient.Connected == false)
            {
                rc = false;
            }
            else
            {
                // Get a client stream for reading and writing.
                tcpStream = tcpClient.GetStream();
            }
               

            
            return rc;
        }
        /// <summary>
        /// function return true if everything is ok, otherwise return false
        /// </summary>
        /// <returns></returns>
        public void Close()
        {
            if (tcpStream != null)
                tcpStream.Close();
            tcpStream = null;
            if (tcpClient != null)
            {
                tcpClient.Close();
                tcpClient.Dispose();
            }
                
            
            tcpClient = null;
        }
        /// <summary>
        /// function return true if everything is ok, otherwise return false
        /// in case of return false, please see in SendStringEX
        /// <para> string message</para>
        /// </summary>
        /// <returns></returns>
        public bool SendString(string msg2send)
        {
            if (tcpClient == null /*|| tcpClient.Connected == false*/)
            {
                if (Open() == false)
                {
                    SendStringEX = "Error opening TCP connection!";
                    return false;
                }
            }

            if (tcpStream.CanWrite)
            {
                Byte[] sendBytes = Encoding.UTF8.GetBytes(msg2send);
                try
                {
                    tcpStream.Write(sendBytes, 0, sendBytes.Length);
                }
                catch (System.Exception ex)
                {
                    SendStringEX = ex.ToString();
                    return false;
                }
            }
            else
            {
                SendStringEX = "You cannot write data to this stream.";
                return false;
            }
            return true;
        }
        public bool SendBytes(Byte[] sendBytes)
        {
            if (tcpClient == null /*|| tcpClient.Connected == false*/)
            {
                if (Open() == false)
                {
                    SendByEx = "Error opening TCP connection!";
                    return false;
                }
            }

            if (tcpStream.CanWrite)
            {
                try
                {
                    tcpStream.Write(sendBytes, 0, sendBytes.Length);
                }
                catch (System.Exception ex)
                {
                    SendByEx = ex.ToString();
                    return false;
                }
            }
            else
            {
                SendByEx = "You cannot write data to this stream.";
                return false;
            }
            return true;
        }
        public string ReceiveString()
        {
            if (tcpStream == null)
                return string.Empty;

            string recv = string.Empty;

            if (tcpStream.CanRead)
            {
                int recvlen = 0;

                // Reads NetworkStream into a byte buffer.
                byte[] bytes = new byte[tcpClient.ReceiveBufferSize];
                try
                {
                    // Read can return anything from 0 to numBytesToRead. 
                    // This method blocks until at least one byte is read.
                    // Or timeout (500mSec) elapses.
                    tcpStream.ReadTimeout = 50;
                    recvlen = tcpStream.Read(bytes, 0, (int)tcpClient.ReceiveBufferSize);
                }
                catch (System.IO.IOException) { }
                catch (System.Exception ex)
                {
                    //MessageBox.Show(ex.Message);
                    ReceipString = ex.ToString();
                    return string.Empty;
                }

                if (recvlen > 0)
                {
                    recv = Encoding.UTF8.GetString(bytes);
                    int len = recv.IndexOf('\0');
                    recv = recv.Substring(0, len);
                }
                return recv;
            }
            else
            {
                //MessageBox.Show("You cannot read data from this stream.");
                ReceipString = "You cannot read data from this stream.";
            }

            return string.Empty;
        }
        public Byte[] ReceiveBytes()
        {
            if (tcpStream == null)
                return null;

            string recv = string.Empty;

            if (tcpStream.CanRead)
            {
                int recvlen = 0;

                // Reads NetworkStream into a byte buffer.
                byte[] bytes = new byte[tcpClient.ReceiveBufferSize];
                try
                {
                    // Read can return anything from 0 to numBytesToRead. 
                    // This method blocks until at least one byte is read.
                    // Or timeout (500mSec) elapses.
                    tcpStream.ReadTimeout = 40000;
                    recvlen = tcpStream.Read(bytes, 0, (int)tcpClient.ReceiveBufferSize);
                }
                catch (System.IO.IOException) { }
                catch (System.Exception ex)
                {
                    ReceipByte = ex.ToString();
                    return null;
                }
                if (recvlen > 0)
                {
                    byte[] recvmsg = new byte[recvlen];
                    for (int i = 0; i < recvlen; i++)
                        recvmsg[i] = bytes[i];
                    return recvmsg;
                }
                return null;
            }
            else
            {
                ReceipByte = "You cannot read data from this stream.";
            }
            return null;
        }
    }
 
}
