using System.Collections.Generic;
using System.Drawing;

namespace QuazalWV
{
    public class StationUrl
    {
        /// <summary>
        /// Protocol type (udp/prudp/prudps).
        /// </summary>
        public string Protocol { get; set; }
        /// <summary>
        /// IP address attribute.
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// Port number attribute.
        /// </summary>
        public ushort Port { get; set; }
        /// <summary>
        /// Connection ID attribute.
        /// </summary>
        public uint CID { get; set; }
        /// <summary>
        /// Principal (player) ID attribute.
        /// </summary>
        public uint PID { get; set; }
        /// <summary>
        /// Stream ID attrbute.
        /// </summary>
        public byte SID { get; set; }
        /// <summary>
        /// RDV connection ID attribute.
        /// </summary>
        public uint RVCID { get; set; }
        /// <summary>
        /// VStream attribute.
        /// </summary>
        public byte Stream { get; set; }
        /// <summary>
        /// Stream type attribute.
        /// </summary>
        public byte Type { get; set; }

        /// <summary>
        /// Constructs TTL session URL for specified address.
        /// </summary>
        public StationUrl(ClientInfo client, string addr)
        {
            Protocol = "prudp";
            Address = addr;
            Port = 4659;
            CID = 1;
            PID = client.PID;
            SID = 1;
            RVCID = client.rvCID;
            Stream = 3;
            Type = 2;
        }

        /// <summary>
        /// Parses a PRUDP URL string.
        /// </summary>
        /// <param name="url"></param>
        public StationUrl(string url)
        {
            string noProtoUrl = ConsumeProtocol(url);
            string[] attrs = noProtoUrl.Split(';');
            KeyValuePair<string, string> pair;
            foreach (string attr in attrs)
            {
                pair = ReadAttr(attr);
                switch (pair.Key)
                {
                    case "address":
                        Address = pair.Value;
                        break;
                    case "port":
                        Port = ushort.Parse(pair.Value);
                        break;
                    case "CID":
                        CID = uint.Parse(pair.Value);
                        break;
                    case "PID":
                        PID = uint.Parse(pair.Value);
                        break;
                    case "sid":
                        SID = byte.Parse(pair.Value);
                        break;
                    case "RVCID":
                        RVCID = uint.Parse(pair.Value);
                        break;
                    case "stream":
                        Stream = byte.Parse(pair.Value);
                        break;
                    case "type":
                        Type = byte.Parse(pair.Value);
                        break;
                    default:
                        Log.WriteLine(1, $"[StationURL] Unknown attribute: {pair.Key}", Color.Red);
                        break;
                }
            }

        }

        /// <summary>
        /// Builds a prudp URL string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // Mandatory
            string result = $"{Protocol}:/address={Address};port={Port}";
            // Optional (required for TTL session URL)
            if (CID > 0) result += $";CID={CID}";
            if (PID > 0) result += $";PID={PID}";
            if (SID > 0) result += $";sid={SID}";
            if (RVCID > 0) result += $";RVCID={RVCID}";
            if (Stream > 0) result += $";stream={Stream}";
            if (Type > 0) result += $";type={Type}";

            return result;
        }

        private string ConsumeProtocol(string url)
        {
            int endIndex = url.IndexOf(":/");
            if (endIndex == -1)
                Log.WriteLine(1, $"[StationURL] Protocol not found in URL: ${url}", Color.Red);
            Protocol = url.Substring(0, endIndex);
            return url.Remove(0, Protocol.Length + 2);
        }

        private KeyValuePair<string, string> ReadAttr(string attr)
        {
            string[] keyValue = attr.Split('=');
            KeyValuePair<string, string> attribute = new KeyValuePair<string, string>(keyValue[0], keyValue[1]);
            return attribute;

        }
    }
}
