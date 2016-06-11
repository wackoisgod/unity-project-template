using System.IO;
using System.Text;
using LibCommon.Network.Types;
using Thrift.Protocol;
using Thrift.Transport;

namespace LibCommon.Network
{
    public class ThriftMessageSerialize
    {
        public static TBase CreateMessage(MessageType inType)
        {
            switch (inType)
            {
                case MessageType.TestEvent:
                    return new Types.TestEvent();
                default:
                    break;
            }

            return null;
        }

        public static GameMessage CreateGameMessage(MessageType inType, TBase content, string inNetworkId)
        {
            var msg = new GameMessage
            {
                EventType = inType,
                Content = Serialize(content),
                NetworkId = inNetworkId
            };

            return msg;
        }

        private static ThriftMessageSerialize _serialize = new ThriftMessageSerialize();

        private ThriftMessageSerialize()
        {
        }

        public static string Serialize(TBase tbase)
        {
            if (tbase == null)
            {
                return null;
            }

            using (MemoryStream outputStream = new MemoryStream())
            {
                TStreamTransport transport = new TStreamTransport(null, outputStream);
                TProtocol protocol = new TJSONProtocol(transport);
                tbase.Write(protocol);
                return Encoding.UTF8.GetString(outputStream.ToArray());
            }
        }

        public static void DeSerialize<T>(T tbase, string inJSON) where T : TBase
        {
            if (tbase == null || inJSON == null)
            {
                return;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(inJSON);
            using (MemoryStream inputStream = new MemoryStream())
            {
                TStreamTransport transport = new TStreamTransport(inputStream, null);
                TProtocol protocol = new TJSONProtocol(transport);

                inputStream.Write(bytes, 0, bytes.Length);
                inputStream.Seek(0, SeekOrigin.Begin);

                tbase.Read(protocol);
            }
        }

        public static byte[] SerializeCompact(TBase tbase)
        {
            if (tbase == null)
            {
                return null;
            }

            using (MemoryStream outputStream = new MemoryStream())
            {
                TStreamTransport transport = new TStreamTransport(null, outputStream);
                TProtocol protocol = new TCompactProtocol(transport);
                tbase.Write(protocol);
                return outputStream.ToArray();
            }
        }

        public static void DeSerializeCompact<T>(T tbase, byte[] inJSON) where T : TBase
        {
            if (tbase == null || inJSON == null)
            {
                return;
            }

            byte[] bytes = inJSON;

            using (MemoryStream inputStream = new MemoryStream())
            {
                TStreamTransport transport = new TStreamTransport(inputStream, null);
                TProtocol protocol = new TCompactProtocol(transport);

                inputStream.Write(bytes, 0, bytes.Length);
                inputStream.Seek(0, SeekOrigin.Begin);

                tbase.Read(protocol);
            }
        }
    }
}
