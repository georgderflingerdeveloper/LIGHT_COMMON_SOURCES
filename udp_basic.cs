using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SystemServices;

namespace Communication
{
    namespace UDP
    {
        // some predefined default settings
        static class UDPConfig
        {
            public static void GetIPAdr ( string hostname )
            {
                IPAddress[] addresslist = Dns.GetHostAddresses( hostname );
            }
            public static int    port              =  7000;
            public static string IpAdressBroadcast =  "192.168.0.255";
            public static string IpAdress          =  "127.0.0.1";       //"192.168.0.105";
            public static int TimeOutCycles        = 10;
        }

        class UdpReceive
        {
            Thread     receiveThread;
            UdpClient  client;
            IPEndPoint anyIP;

            public int port; // define > init

            public UdpReceive ( int port_ )
            {
                port = port_;
                // ----------------------------
                // Abhören
                // ----------------------------
                // Lokalen Endpunkt definieren (wo Nachrichten empfangen werden).
                // Einen neuen Thread für den Empfang eingehender Nachrichten erstellen.
                client = new UdpClient( port );
                anyIP = new IPEndPoint( IPAddress.Any, port );
                receiveThread = new Thread( new ThreadStart( ReceiveData ) );
                receiveThread.IsBackground = true;
                receiveThread.Start( );
         }

            // infos
            public string lastReceivedUDPPacket="";
            public string allReceivedUDPPackets=""; // clean up this from time to time!
            string _receivedText;

            public delegate void DataReceived ( string e );
            public event         DataReceived EDataReceived;

            private void ReceiveData ( )
            {
                _receivedText = "";
                if( client != null )
                {
                    while( true )
                    {
                        try
                        {
                            if( anyIP != null )
                            {
                                // Bytes empfangen.
                                byte[] data = client.Receive( ref anyIP );

                                if( (data != null) && (data.Length > 0) )
                                {
                                    // Bytes mit der UTF8-Kodierung in das Textformat kodieren.
                                    _receivedText = Encoding.UTF8.GetString( data );
                                    if( EDataReceived != null )
                                    {
                                        if( !String.IsNullOrEmpty( _receivedText ) )
                                        {
                                            EDataReceived( _receivedText );
                                        }
                                    }
                                    // latest UDPpacket
                                    lastReceivedUDPPacket = _receivedText;
                                    allReceivedUDPPackets = allReceivedUDPPackets + _receivedText;
                                }
                           }
                        }
                        catch( Exception err )
                        {
                            Services.TraceMessage_( err.Message );
                            Console.WriteLine( TimeUtil.GetTimestamp( ) + " " + err.Data );
                            Console.WriteLine( TimeUtil.GetTimestamp( ) + " " + err.Source );
                            Console.WriteLine( TimeUtil.GetTimestamp( ) + " " + err.InnerException );
                        }
                    }
                }
            }

            public string ReceivedText
            {
                get
                {
                    return _receivedText;
                }
            }

            public void Abort( )
            {
                if( client != null )
                {
                    client.Close();
                    receiveThread.Abort();
                }
            }
        }

        class UdpSend
        {
            // prefs 
            private string IP = UDPConfig.IpAdress;  // define in init
            public int port   = UDPConfig.port;  // define in init
            // "connection" things
            IPEndPoint remoteEndPoint;
            UdpClient client;

            public UdpSend ( )
            {
                remoteEndPoint = new IPEndPoint( IPAddress.Parse( IP ), port );
                client         = new UdpClient( );
            }

            public UdpSend ( string ip_, int port_ )
            {
                remoteEndPoint = new IPEndPoint( IPAddress.Parse( ip_ ), port_ );
                client         = new UdpClient(  );
            }
            // sendData
            private void sendString ( string message )
            {
                if( String.IsNullOrWhiteSpace( message ) )
                {
                    Services.TraceMessage_( "Try to send empty string!" );
                    return;
                }
                try
                {
                    // Daten mit der UTF8-Kodierung in das Binärformat kodieren.
                    byte[] data = Encoding.UTF8.GetBytes( message );
                    if( client != null )
                    {
                        // Daten message zum Remote-Client senden.
                        client.Send( data, data.Length, remoteEndPoint );
                    }
                }
                catch( Exception err )
                {
                    Services.TraceMessage_( err.Message + " " + err.Data + " " );
                }
            }

            public void SendString( string message )
            {
                sendString ( message );
            }

            string _SendText;
            public string SendText
            {
                set
                {
                    _SendText = value;
                    sendString( value );
                }
            }
        }
    }
}
