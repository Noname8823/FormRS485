using System;
using System.IO.Ports;
using System.Text;

namespace projectRS485
{
    public class RS485Port
    {
        private SerialPort serialPort;
        public event Action<string> DataReceived;        // Dữ liệu dạng string
        public event Action<byte[]> DataReceivedBytes;   // Dữ liệu dạng byte array
        public event Action<string> DataReceivedHex;     // Dữ liệu dạng hex string

        public RS485Port()
        {
            serialPort = new SerialPort();
            serialPort.DataBits = 8;
            serialPort.Parity = Parity.None;
            serialPort.StopBits = StopBits.One;
            serialPort.DataReceived += SerialPort_DataReceived;
        }

        public string[] GetPortNames()
        {
            return SerialPort.GetPortNames();
        }

        public bool IsOpen => serialPort.IsOpen;

        public void Open(string portName, int baudRate)
        {
            if (serialPort.IsOpen) serialPort.Close();
            serialPort.PortName = portName;
            serialPort.BaudRate = baudRate;
            serialPort.Open();
        }

        public void Close()
        {
            if (serialPort.IsOpen)
                serialPort.Close();
        }

        // Gửi dữ liệu dạng byte array
        public void Send(byte[] data)
        {
            if (serialPort.IsOpen)
                serialPort.Write(data, 0, data.Length);
        }

        // Gửi dữ liệu từ hex string (ví dụ: "FE 02 00 00 00 04 6D C6")
        public void SendHex(string hexString)
        {
            try
            {
                byte[] data = HexStringToByteArray(hexString);
                Send(data);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Invalid hex string: {ex.Message}");
            }
        }

        // Chuyển đổi hex string thành byte array
        public static byte[] HexStringToByteArray(string hexString)
        {
            // Loại bỏ khoảng trắng và ký tự không cần thiết
            hexString = hexString.Replace(" ", "").Replace("-", "").Replace(":", "");

            if (hexString.Length % 2 != 0)
                throw new ArgumentException("Hex string must have even number of characters");

            byte[] bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return bytes;
        }

        // Chuyển đổi byte array thành hex string (phiên bản đơn giản)
        public static string ByteArrayToHexString(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                if (i > 0) sb.Append(" ");
                sb.Append(bytes[i].ToString("X2"));
            }
            return sb.ToString();
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                // Đọc tất cả dữ liệu có sẵn
                int bytesToRead = serialPort.BytesToRead;
                byte[] buffer = new byte[bytesToRead];
                serialPort.Read(buffer, 0, bytesToRead);

                // Kích hoạt các event khác nhau
                DataReceivedBytes?.Invoke(buffer);
                DataReceivedHex?.Invoke(ByteArrayToHexString(buffer));

                // Nếu muốn hiển thị dưới dạng string (có thể có ký tự đặc biệt)
                string stringData = Encoding.ASCII.GetString(buffer);
                DataReceived?.Invoke(stringData);
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần
                Console.WriteLine($"Error reading data: {ex.Message}");
            }
        }

        // Phương thức để hiển thị thông tin debug
        public void LogSentData(byte[] data)
        {
            Console.WriteLine($"Sent: {ByteArrayToHexString(data)}");
        }

        public void LogSentHex(string hexString)
        {
            Console.WriteLine($"Sent Hex: {hexString}");
        }
    }
}