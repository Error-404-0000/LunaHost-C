using System.Net;

namespace SWH
{
    public class TcpPacket
    {
        // IP Header Fields
        public byte Version { get; private set; }
        public byte HeaderLength { get; private set; }
        public byte TypeOfService { get; private set; }
        public ushort TotalLength { get; private set; }
        public ushort Identification { get; private set; }
        public bool DontFragment { get; private set; }
        public bool MoreFragments { get; private set; }
        public ushort FragmentOffset { get; private set; }
        public byte TimeToLive { get; private set; }
        public byte Protocol { get; private set; }
        public IPAddress SourceIpAddress { get; private set; }
        public IPAddress DestinationIpAddress { get; private set; }

        // TCP Header Fields
        public ushort SourcePort { get; private set; }
        public ushort DestinationPort { get; private set; }
        public uint SequenceNumber { get; private set; }
        public uint AcknowledgmentNumber { get; private set; }
        public byte DataOffset { get; private set; }
        public byte Flags { get; private set; }
        public ushort WindowSize { get; private set; }
        public ushort Checksum { get; private set; }
        public ushort UrgentPointer { get; private set; }

        // Raw buffer containing both headers
        public TcpPacket(byte[] rawBuffer)
        {
            if (rawBuffer == null || rawBuffer.Length < 20)
                throw new ArgumentException("Invalid raw buffer provided");

            ParseIpHeader(rawBuffer);
            ParseTcpHeader(rawBuffer, HeaderLength * 4);  // IP header length in bytes
        }

        // Method to parse IP header
        private void ParseIpHeader(byte[] buffer)
        {
            Version = (byte)(buffer[0] >> 4);   // First 4 bits represent the version
            HeaderLength = (byte)(buffer[0] & 0x0F);  // Next 4 bits represent the header length
            TypeOfService = buffer[1];
            TotalLength = (ushort)((buffer[2] << 8) + buffer[3]);
            Identification = (ushort)((buffer[4] << 8) + buffer[5]);

            ushort flagsAndOffset = (ushort)((buffer[6] << 8) + buffer[7]);
            DontFragment = (flagsAndOffset & 0x4000) != 0;
            MoreFragments = (flagsAndOffset & 0x2000) != 0;
            FragmentOffset = (ushort)(flagsAndOffset & 0x1FFF);

            TimeToLive = buffer[8];
            Protocol = buffer[9];
            SourceIpAddress = new IPAddress(new byte[] { buffer[12], buffer[13], buffer[14], buffer[15] });
            DestinationIpAddress = new IPAddress(new byte[] { buffer[16], buffer[17], buffer[18], buffer[19] });
        }

        // Method to parse TCP header (offset is where the TCP header starts in the raw buffer)
        private void ParseTcpHeader(byte[] buffer, int offset)
        {
            SourcePort = (ushort)((buffer[offset] << 8) + buffer[offset + 1]);
            DestinationPort = (ushort)((buffer[offset + 2] << 8) + buffer[offset + 3]);
            SequenceNumber = (uint)((buffer[offset + 4] << 24) + (buffer[offset + 5] << 16) + (buffer[offset + 6] << 8) + buffer[offset + 7]);
            AcknowledgmentNumber = (uint)((buffer[offset + 8] << 24) + (buffer[offset + 9] << 16) + (buffer[offset + 10] << 8) + buffer[offset + 11]);

            byte offsetAndFlags = buffer[offset + 12];
            DataOffset = (byte)((offsetAndFlags >> 4) * 4);  // Data offset is the upper 4 bits (in words, so multiply by 4 to get bytes)
            Flags = buffer[offset + 13];

            WindowSize = (ushort)((buffer[offset + 14] << 8) + buffer[offset + 15]);
            Checksum = (ushort)((buffer[offset + 16] << 8) + buffer[offset + 17]);
            UrgentPointer = (ushort)((buffer[offset + 18] << 8) + buffer[offset + 19]);
        }

        public override string ToString()
        {
            return $"TCP Packet: {SourceIpAddress}:{SourcePort} -> {DestinationIpAddress}:{DestinationPort}\n" +
                   $"Sequence: {SequenceNumber}, Acknowledgment: {AcknowledgmentNumber}\n" +
                   $"Flags: {Flags}, Window Size: {WindowSize}, Checksum: {Checksum}\n";
        }
    }


}
