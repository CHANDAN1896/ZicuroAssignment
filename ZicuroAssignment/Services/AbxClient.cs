using ZicuroAssignment.Models;
using ZicuroAssignment.Log;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json;



namespace ZicuroAssignment.Services
{
    public class AbxClient
    {
        private const int PacketSize = 17;
        private const string Host = "127.0.0.1";
        private const int Port = 3000;

        public async Task<List<TickerPacket>> FetchAllPacketsAsync()
        {
            try
            {
                var packets = await SendStreamAllPacketsRequest();
                var missingSequences = FindMissingSequences(packets);

                foreach (var seq in missingSequences)
                {
                    var resendPacket = await RequestMissingPacket(seq);
                    if (resendPacket != null)
                        packets.Add(resendPacket);
                }

                return packets.OrderBy(p => p.Sequence).ToList();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return new List<TickerPacket>(); // Fail gracefully
            }
        }


        private async Task<List<TickerPacket>> SendStreamAllPacketsRequest()
        {
            var packets = new List<TickerPacket>();
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(Host, Port);
                using var stream = client.GetStream();

                byte[] payload = new byte[] { 1, 0 };
                await stream.WriteAsync(payload, 0, payload.Length);

                var buffer = new byte[PacketSize * 10];
                int bytesRead;
                var dataBuffer = new List<byte>();

                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    dataBuffer.AddRange(buffer.Take(bytesRead));
                    while (dataBuffer.Count >= PacketSize)
                    {
                        var packetBytes = dataBuffer.Take(PacketSize).ToArray();
                        dataBuffer.RemoveRange(0, PacketSize);
                        packets.Add(ParsePacket(packetBytes));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }

            return packets;
        }


        private async Task<TickerPacket?> RequestMissingPacket(int sequence)
        {
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(Host, Port);
                using var stream = client.GetStream();

                byte[] payload = new byte[] { 2, (byte)sequence };
                await stream.WriteAsync(payload, 0, payload.Length);

                byte[] buffer = new byte[PacketSize];
                int bytesRead = await stream.ReadAsync(buffer, 0, PacketSize);

                return bytesRead == PacketSize ? ParsePacket(buffer) : null;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, nameof(RequestMissingPacket));
                return null; // Gracefully handle failure
            }
        }


        private TickerPacket ParsePacket(byte[] data)
        {
          
            try
            {
                string symbol = System.Text.Encoding.ASCII.GetString(data, 0, 4);
                string side = ((char)data[4]).ToString();
                int quantity = ReadInt32BigEndian(data, 5);
                int price = ReadInt32BigEndian(data, 9);
                int sequence = ReadInt32BigEndian(data, 13);
                return new TickerPacket
                {
                    Symbol = symbol,
                    Side = side,
                    Quantity = quantity,
                    Price = price,
                    Sequence = sequence
                };

            }
            catch (Exception ex) { 
            Logger.LogError(ex);
                return null;

            }
        
        }

        private int ReadInt32BigEndian(byte[] data, int offset)
        {
            return (data[offset] << 24) |
                   (data[offset + 1] << 16) |
                   (data[offset + 2] << 8) |
                   data[offset + 3];
        }

        private List<int> FindMissingSequences(List<TickerPacket> packets)
        {
            var missing = new List<int>();
            try
            {
                var received = packets.Select(p => p.Sequence).ToHashSet();
                int min = received.Min();
                int max = received.Max();


                for (int i = min; i <= max; i++)
                {
                    if (!received.Contains(i))
                        missing.Add(i);
                }
            }
            catch (Exception ex){
                Logger.LogError(ex);
            }
            return missing;
        }

        public void WriteToJson(List<TickerPacket> packets, string path = "output.json")
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(packets, options);
                File.WriteAllText(path, json);
            }
            catch(Exception ex) {
                Logger.LogError(ex);
            }
        }
    }
}
