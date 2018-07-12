using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;
using NetworkCommsDotNet.Connections.UDP;
using NetworkCommsDotNet.DPSBase;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TIPimpl
{
    class Networking
    {
        ConnectionInfo connInfo = null;
        Connection newUDPConn = null;
        bool initialised = false;
        string ip = "";
        public void Initializecon(string ip)
        {
            this.ip = ip;
            if (initialised) return;
            //connInfo = new ConnectionInfo(ip, 10000);
            //newUDPConn = UDPConnection.GetConnection(connInfo, UDPOptions.None);
            initialised = true;
        }
        
        public void Closeconn()
        {
            //newUDPConn.CloseConnection(false);
            NetworkComms.CloseAllConnections();
            NetworkComms.RemoveGlobalIncomingPacketHandler<byte[]>("icecream", Unamangedbytevoice);
            NetworkComms.RemoveGlobalIncomingPacketHandler();
            Debug.WriteLine(NetworkComms.GlobalIncomingPacketHandlerExists("icecream"));
            
            NetworkComms.Shutdown();
            
        }

        public void Datasend(byte[] buff)
        {
            // NetworkComms.SendObject("VOICE", buff, newUDPConn);
            //newUDPConn.SendObject<byte[]>("icecream", buff);
            //UDPConnection.SendObject<byte[]>("icecream", buff, new IPEndPoint(IPAddress.Parse("127.0.0.1"), 10000));
            UDPConnection.SendObject("icecream", buff, new IPEndPoint(IPAddress.Parse(ip), 10000));
            //Debug.WriteLine("sent");

        }
        public void Datalisten()
        {
            //listener.AppendIncomingUnmanagedPacketHandler(Unamangedbytevoice);
            //newUDPConn.AppendIncomingPacketHandler<byte[]>("icecream", Unamangedbytevoice);
            Connection.StartListening(ConnectionType.UDP, new IPEndPoint(IPAddress.Parse(ip), 10000));
            NetworkComms.AppendGlobalIncomingPacketHandler<byte[]>("icecream", Unamangedbytevoice);
            //newUDPConn.AppendIncomingPacketHandler<byte[]>("icecream", Unamangedbytevoice);
        }
        private void Unamangedbytevoice(PacketHeader packetHeader, Connection connection, byte[] incomingObject)
        {
            VoiceHandling.decode(incomingObject, incomingObject.Length);
        }  
    }
}
