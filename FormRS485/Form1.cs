using FormRS485;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace projectRS485
{
    public partial class Form1 : Form
    {
        private RS485Port rs485 = new RS485Port();
        private InputManager inputManager;
        private OutputManager outputManager;

        private bool isWaitingForResponse = false;
        private string lastSentCommand = "";

        public Form1()
        {
            InitializeComponent();

            // Khởi tạo RS485 events
            rs485.DataReceived += RS485_DataReceived;
            rs485.DataReceivedHex += RS485_DataReceivedHex;
            rs485.DataReceivedBytes += RS485_DataReceivedBytes;

            // Khởi tạo các manager sau khi InitializeComponent
            InitializeManagers();
        }

        private void InitializeManagers()
        {
            // Khởi tạo InputManager
            inputManager = new InputManager(rs485, txtIn1, txtIn2, txtIn3, txtIn4);
            inputManager.OnSendCommand += HandleSendCommand;

            // Khởi tạo OutputManager
            outputManager = new OutputManager(rs485,
                btnOut1, btnOut2, btnOut3, btnOut4,
                txtOut1, txtOut2, txtOut3, txtOut4);
            outputManager.OnSendCommand += HandleSendCommand;
        }

        private void HandleSendCommand(string command, string commandId)
        {
            if (isWaitingForResponse) return;

            try
            {
                isWaitingForResponse = true;
                lastSentCommand = commandId;
                rs485.SendHex(command);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi gửi lệnh: {ex.Message}");
                isWaitingForResponse = false;
                lastSentCommand = "";
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBoxComPort.Items.Clear();
            comboBoxComPort.Items.AddRange(rs485.GetPortNames());
            if (comboBoxComPort.Items.Count > 0) comboBoxComPort.SelectedIndex = 0;
            comboBoxBaudRate.SelectedIndex = 3;
            lblStatus.Text = "Chưa kết nối";
        }

        private void RS485_DataReceived(string data)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => RS485_DataReceived(data)));
                return;
            }

            if (!string.IsNullOrEmpty(data) && !ContainsBinaryData(data))
            {
                // Xử lý data string nếu cần
            }
        }

        private void RS485_DataReceivedHex(string hexData)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => RS485_DataReceivedHex(hexData)));
                return;
            }

            Console.WriteLine("HEX Data Received: " + hexData);
            AnalyzeHexResponse(hexData);
        }

        private void AnalyzeHexResponse(string hexData)
        {
            if (string.IsNullOrEmpty(hexData)) return;

            try
            {
                // Phân phối xử lý dữ liệu cho các manager tương ứng
                if (lastSentCommand.StartsWith("READ_INPUT_"))
                {
                    inputManager.ProcessInputResponse(hexData, lastSentCommand);
                }
                else if (lastSentCommand.StartsWith("RELAY_"))
                {
                    outputManager.ProcessOutputResponse(hexData, lastSentCommand);
                }

                isWaitingForResponse = false;
                lastSentCommand = "";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi phân tích dữ liệu hex: {ex.Message}");
                isWaitingForResponse = false;
            }
        }

        private void RS485_DataReceivedBytes(byte[] data)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => RS485_DataReceivedBytes(data)));
                return;
            }

            Console.WriteLine("Bytes Received: " + BitConverter.ToString(data));
        }

        private bool ContainsBinaryData(string data)
        {
            foreach (char c in data)
            {
                if (c < 32 && c != '\r' && c != '\n' && c != '\t')
                    return true;
            }
            return false;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            comboBoxComPort.Items.Clear();
            comboBoxComPort.Items.AddRange(rs485.GetPortNames());
            if (comboBoxComPort.Items.Count > 0) comboBoxComPort.SelectedIndex = 0;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (!rs485.IsOpen)
                {
                    string port = comboBoxComPort.SelectedItem.ToString();
                    int baud = int.Parse(comboBoxBaudRate.SelectedItem.ToString());
                    rs485.Open(port, baud);
                    UpdateConnectionUI(true);

                    // Bắt đầu đọc input khi kết nối thành công
                    inputManager.StartReading();

                    MessageBox.Show($"Kết nối thành công {port}!");
                }
                else
                {
                    // Dừng đọc input khi ngắt kết nối
                    inputManager.StopReading();

                    rs485.Close();
                    UpdateConnectionUI(false);
                    MessageBox.Show("Đã ngắt kết nối!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối: " + ex.Message);
            }
        }

        private void UpdateConnectionUI(bool connected)
        {
            btnConnect.Text = connected ? "Ngắt kết nối" : "Kết nối";
            lblStatus.Text = connected ? "Đã kết nối" : "Chưa kết nối";
            lblStatus.ForeColor = connected ? Color.Green : Color.Red;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Dọn dẹp resources
            inputManager?.Dispose();

            if (rs485.IsOpen) rs485.Close();
        }

        // Empty handlers to avoid designer errors
        private void listBoxLog_SelectedIndexChanged(object sender, EventArgs e) { }
        private void comboBoxBaudRate_SelectedIndexChanged(object sender, EventArgs e) { }
        private void comboBoxComPort_SelectedIndexChanged(object sender, EventArgs e) { }

        // Các method này có thể được gọi từ bên ngoài để truy cập trạng thái
        public bool GetInputState(int inputNumber)
        {
            switch (inputNumber)
            {
                case 1: return inputManager.Input1State;
                case 2: return inputManager.Input2State;
                case 3: return inputManager.Input3State;
                case 4: return inputManager.Input4State;
                default: return false;
            }
        }

        public bool GetRelayState(int relayNumber)
        {
            switch (relayNumber)
            {
                case 1: return outputManager.Relay1State;
                case 2: return outputManager.Relay2State;
                case 3: return outputManager.Relay3State;
                case 4: return outputManager.Relay4State;
                default: return false;
            }
        }

        public void SetRelayState(int relayNumber, bool state)
        {
            outputManager.SetRelayState(relayNumber, state);
        }
    }
}