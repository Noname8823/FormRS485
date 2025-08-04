using FormRS485;
using System;
using System.Drawing;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;

namespace projectRS485
{
    public partial class Form1 : Form
    {
        private RS485Port rs485 = new RS485Port();
        private bool relay1State = false, relay2State = false, relay3State = false, relay4State = false;
        private bool input1State = false, input2State = false, input3State = false, input4State = false;
        private bool isWaitingForResponse = false;
        private string lastSentCommand = "";

        // Timer cho việc đọc input
        private System.Windows.Forms.Timer inputReadTimer;
        private int currentInputToRead = 1; // Biến để theo dõi input nào đang được đọc

        public Form1()
        {
            InitializeComponent();
            rs485.DataReceived += RS485_DataReceived;
            rs485.DataReceivedHex += RS485_DataReceivedHex;
            rs485.DataReceivedBytes += RS485_DataReceivedBytes;

            // Khởi tạo timer
            InitializeInputTimer();
        }

        private void InitializeInputTimer()
        {
            inputReadTimer = new System.Windows.Forms.Timer();
            inputReadTimer.Interval = 2000; // 2 giây
            inputReadTimer.Tick += InputReadTimer_Tick;
        }

        private void InputReadTimer_Tick(object sender, EventArgs e)
        {
            if (!rs485.IsOpen || isWaitingForResponse) return;

            try
            {
                // Đọc từng input theo chu kỳ
                string readCommand = GetInputReadCommand(currentInputToRead);
                if (!string.IsNullOrEmpty(readCommand))
                {
                    isWaitingForResponse = true;
                    lastSentCommand = $"READ_INPUT_{currentInputToRead}";
                    rs485.SendHex(readCommand);
                }

                // Chuyển sang input tiếp theo
                currentInputToRead = (currentInputToRead % 4) + 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi đọc input: {ex.Message}");
                isWaitingForResponse = false;
            }
        }

        private string GetInputReadCommand(int inputNumber)
        {
            // Lệnh Modbus đọc trạng thái input (function code 02 - Read Discrete Inputs)
            // Format: Device_ID Function_Code Start_Address_Hi Start_Address_Lo Quantity_Hi Quantity_Lo CRC_Lo CRC_Hi
            switch (inputNumber)
            {
                case 1: return "FE 02 00 00 00 01 A9 D6"; // Đọc input 1
                case 2: return "FE 02 00 01 00 01 F8 16"; // Đọc input 2  
                case 3: return "FE 02 00 02 00 01 08 16"; // Đọc input 3
                case 4: return "FE 02 00 03 00 01 59 D6"; // Đọc input 4
                default: return "";
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBoxComPort.Items.Clear();
            comboBoxComPort.Items.AddRange(rs485.GetPortNames());
            if (comboBoxComPort.Items.Count > 0) comboBoxComPort.SelectedIndex = 0;
            comboBoxBaudRate.SelectedIndex = 3;
            lblStatus.Text = "Chưa kết nối";
            InitializeControlStates();
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
                // Loại bỏ khoảng trắng và chuyển về chữ hoa
                string cleanHex = hexData.Replace(" ", "").ToUpper();

                // Kiểm tra nếu là phản hồi đọc input
                if (lastSentCommand.StartsWith("READ_INPUT_") && cleanHex.Length >= 10)
                {
                    // Phản hồi đọc input có format: FE 02 01 XX CRC_LO CRC_HI
                    // Byte thứ 4 (XX) chứa trạng thái input
                    if (cleanHex.StartsWith("FE02"))
                    {
                        string statusByte = cleanHex.Substring(6, 2); // Lấy byte trạng thái
                        int inputNumber = int.Parse(lastSentCommand.Replace("READ_INPUT_", ""));

                        // Chuyển đổi hex thành int để kiểm tra bit
                        int status = Convert.ToInt32(statusByte, 16);
                        bool inputState = (status & 0x01) != 0; // Kiểm tra bit 0

                        UpdateInputState(inputNumber, inputState);
                    }
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

        private void UpdateInputState(int inputNumber, bool state)
        {
            TextBox targetTextBox = null;

            switch (inputNumber)
            {
                case 1:
                    input1State = state;
                    targetTextBox = txtIn1;
                    break;
                case 2:
                    input2State = state;
                    targetTextBox = txtIn2;
                    break;
                case 3:
                    input3State = state;
                    targetTextBox = txtIn3;
                    break;
                case 4:
                    input4State = state;
                    targetTextBox = txtIn4;
                    break;
            }

            if (targetTextBox != null)
            {
                targetTextBox.Text = state ? "ON" : "OFF";
                UpdateInputColors(targetTextBox);
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

        private void InitializeControlStates()
        {
            // Khởi tạo trạng thái output
            InitializeTextBox(txtOut1, "OFF", UpdateOutputColors);
            InitializeTextBox(txtOut2, "OFF", UpdateOutputColors);
            InitializeTextBox(txtOut3, "OFF", UpdateOutputColors);
            InitializeTextBox(txtOut4, "OFF", UpdateOutputColors);

            // Khởi tạo trạng thái input
            InitializeTextBox(txtIn1, "OFF", UpdateInputColors);
            InitializeTextBox(txtIn2, "OFF", UpdateInputColors);
            InitializeTextBox(txtIn3, "OFF", UpdateInputColors);
            InitializeTextBox(txtIn4, "OFF", UpdateInputColors);
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

        private void InitializeTextBox(TextBox box, string text, Action<TextBox> colorUpdater)
        {
            if (box != null)
            {
                box.Text = text;
                colorUpdater(box);
            }
        }

        private void UpdateInputColors(TextBox inputBox)
        {
            if (inputBox == null) return;
            if (inputBox.Text == "ON")
            {
                inputBox.BackColor = Color.LightBlue;
                inputBox.ForeColor = Color.DarkBlue;
            }
            else
            {
                inputBox.BackColor = Color.LightGray;
                inputBox.ForeColor = Color.Black;
            }
        }

        private void UpdateOutputColors(TextBox outputBox)
        {
            if (outputBox == null) return;
            if (outputBox.Text == "ON")
            {
                outputBox.BackColor = Color.LightGreen;
                outputBox.ForeColor = Color.DarkGreen;
            }
            else
            {
                outputBox.BackColor = Color.LightPink;
                outputBox.ForeColor = Color.DarkRed;
            }
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

                    // Bắt đầu timer đọc input khi kết nối thành công
                    inputReadTimer.Start();

                    MessageBox.Show($"Kết nối thành công {port}!");
                }
                else
                {
                    // Dừng timer khi ngắt kết nối
                    inputReadTimer.Stop();

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
            // Dừng timer trước khi đóng form
            if (inputReadTimer != null)
            {
                inputReadTimer.Stop();
                inputReadTimer.Dispose();
            }

            if (rs485.IsOpen) rs485.Close();
        }

        private void ToggleRelay(ref bool relayState, string name, string cmdOn, string cmdOff, Button btn, TextBox txt)
        {
            if (!rs485.IsOpen)
            {
                MessageBox.Show("Cổng RS485 chưa được kết nối!");
                return;
            }

            try
            {
                string cmd = relayState ? cmdOff : cmdOn;
                relayState = !relayState;
                UpdateButtonState(btn, name, relayState);
                if (txt != null) txt.Text = relayState ? "ON" : "OFF";
                if (txt != null) UpdateOutputColors(txt);
                isWaitingForResponse = true;
                lastSentCommand = cmd;
                rs485.SendHex(cmd);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi điều khiển {name}: {ex.Message}");
                isWaitingForResponse = false;
                lastSentCommand = "";
            }
        }

        private void UpdateButtonState(Button button, string name, bool state)
        {
            if (button == null) return;
            button.UseVisualStyleBackColor = false;
            button.BackColor = state ? Color.LimeGreen : Color.LightGray;
            button.Text = $"{name}: {(state ? "ON" : "OFF")}";
        }

        private void button1_Click(object sender, EventArgs e) => ToggleRelay(ref relay1State, "Relay 1", "FE 05 00 00 FF 00 98 35", "FE 05 00 00 00 00 D9 C5", btnOut1, txtOut1);
        private void btnOut2_Click(object sender, EventArgs e) => ToggleRelay(ref relay3State, "Relay 2", "FE 05 00 01 FF 00 C9 F5", "FE 05 00 01 00 00 88 05", btnOut2, txtOut2);
        private void btnOut3_Click(object sender, EventArgs e) => ToggleRelay(ref relay2State, "Relay 3", "FE 05 00 02 FF 00 39 F5", "FE 05 00 02 00 00 78 05", btnOut3, txtOut3);
        private void btnOut4_Click(object sender, EventArgs e) => ToggleRelay(ref relay4State, "Relay 4", "FE 05 00 03 FF 00 68 35", "FE 05 00 03 00 00 29 C5", btnOut4, txtOut4);

        // Empty handlers to avoid designer errors
        private void listBoxLog_SelectedIndexChanged(object sender, EventArgs e) { }
        private void comboBoxBaudRate_SelectedIndexChanged(object sender, EventArgs e) { }
        private void comboBoxComPort_SelectedIndexChanged(object sender, EventArgs e) { }
    }
}