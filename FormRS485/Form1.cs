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
        private ProductCountManager productCountManager;

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

            // Khởi tạo ProductCountManager - sửa tên tham số từ rs485Port thành rs485
            productCountManager = new ProductCountManager(rs485,
                txtSlaveID,
                btnConnectSlaveID,
                dgvProducts,
                inputManager);
            productCountManager.OnSendCommand += HandleSendCommand;
        }

        private void HandleSendCommand(string command, string commandId)
        {
            if (isWaitingForResponse) return;

            try
            {
                isWaitingForResponse = true;
                lastSentCommand = commandId;
                rs485.SendHex(command);

                Console.WriteLine($"Sent: {command} | Command ID: {commandId}");

                // Tự động reset trạng thái sau 2 giây để tránh bị kẹt
                var resetTimer = new System.Windows.Forms.Timer();
                resetTimer.Interval = 2000;
                resetTimer.Tick += (s, e) => {
                    isWaitingForResponse = false;
                    lastSentCommand = "";
                    ((System.Windows.Forms.Timer)s).Stop();
                    ((System.Windows.Forms.Timer)s).Dispose();
                };
                resetTimer.Start();
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
            try
            {
                comboBoxComPort.Items.Clear();
                string[] ports = rs485.GetPortNames();
                if (ports.Length > 0)
                {
                    comboBoxComPort.Items.AddRange(ports);
                    comboBoxComPort.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("Không tìm thấy cổng COM nào!", "Cảnh báo",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // Đảm bảo comboBoxBaudRate có items trước khi set SelectedIndex
                if (comboBoxBaudRate.Items.Count > 3)
                {
                    comboBoxBaudRate.SelectedIndex = 3;
                }

                lblStatus.Text = "Chưa kết nối";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khởi tạo form: {ex.Message}");
            }
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
                Console.WriteLine("String Data Received: " + data);
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
                    inputManager?.ProcessInputResponse(hexData, lastSentCommand);
                }
                else if (lastSentCommand.StartsWith("RELAY_"))
                {
                    outputManager?.ProcessOutputResponse(hexData, lastSentCommand);
                }
                else if (lastSentCommand == "TEST_SLAVE_CONNECTION")
                {
                    Console.WriteLine($"Slave connection test response: {hexData}");
                    // Xử lý phản hồi test kết nối slave
                }

                isWaitingForResponse = false;
                lastSentCommand = "";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi phân tích dữ liệu hex: {ex.Message}");
                isWaitingForResponse = false;
                lastSentCommand = "";
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
            try
            {
                comboBoxComPort.Items.Clear();
                string[] ports = rs485.GetPortNames();
                if (ports.Length > 0)
                {
                    comboBoxComPort.Items.AddRange(ports);
                    comboBoxComPort.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("Không tìm thấy cổng COM nào!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi refresh cổng COM: {ex.Message}");
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (!rs485.IsOpen)
                {
                    if (comboBoxComPort.SelectedItem == null)
                    {
                        MessageBox.Show("Vui lòng chọn cổng COM!");
                        return;
                    }

                    if (comboBoxBaudRate.SelectedItem == null)
                    {
                        MessageBox.Show("Vui lòng chọn Baud Rate!");
                        return;
                    }

                    string port = comboBoxComPort.SelectedItem.ToString();
                    int baud = int.Parse(comboBoxBaudRate.SelectedItem.ToString());
                    rs485.Open(port, baud);
                    UpdateConnectionUI(true);

                    // Bắt đầu đọc input khi kết nối thành công
                    inputManager?.StartReading();

                    MessageBox.Show($"Kết nối thành công {port}!");
                }
                else
                {
                    // Dừng đọc input khi ngắt kết nối
                    inputManager?.StopReading();

                    // Ngắt kết nối slave nếu đang kết nối
                    productCountManager?.DisconnectSlave();

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
            try
            {
                // Dọn dẹp resources
                inputManager?.StopReading();
                inputManager?.Dispose();
                productCountManager?.DisconnectSlave();

                if (rs485.IsOpen) rs485.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi đóng form: {ex.Message}");
            }
        }

        // Empty handlers to avoid designer errors
        private void listBoxLog_SelectedIndexChanged(object sender, EventArgs e) { }
        private void comboBoxBaudRate_SelectedIndexChanged(object sender, EventArgs e) { }
        private void comboBoxComPort_SelectedIndexChanged(object sender, EventArgs e) { }

        // Các method này có thể được gọi từ bên ngoài để truy cập trạng thái
        public bool GetInputState(int inputNumber)
        {
            if (inputManager == null) return false;

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
            if (outputManager == null) return false;

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
            outputManager?.SetRelayState(relayNumber, state);
        }

        // Methods cho Product Count Manager
        public int GetProductCount(int inputIndex)
        {
            if (productCountManager == null || inputIndex < 0 || inputIndex >= 4) return 0;
            return productCountManager.ProductCounts[inputIndex];
        }

        public void ResetAllProductCounts()
        {
            productCountManager?.ResetProductCounts();
        }

        public string GetProductName(int inputIndex)
        {
            return productCountManager?.GetProductName(inputIndex) ?? $"Sản phẩm {inputIndex + 1}";
        }

        public bool IsSlaveConnected()
        {
            return productCountManager?.IsSlaveConnected ?? false;
        }

        public byte GetCurrentSlaveID()
        {
            return productCountManager?.CurrentSlaveID ?? 0xFE;
        }

        // Event handlers cho các control mới
        private void btnResetCounts_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Bạn có chắc muốn reset tất cả số đếm?", "Xác nhận",
                                  MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    productCountManager?.ResetProductCounts();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi reset số đếm: {ex.Message}");
            }
        }

        private void btnConnectSlaveID_Click(object sender, EventArgs e)
        {
            // Event này được xử lý bởi ProductCountManager
            // Không cần code ở đây vì button event đã được setup trong ProductCountManager
        }

        // Method để hiển thị thông tin debug
        public void ShowDebugInfo()
        {
            try
            {
                string info = $"RS485 Connected: {rs485.IsOpen}\n";
                info += $"Slave Connected: {IsSlaveConnected()}\n";
                info += $"Slave ID: {GetCurrentSlaveID()}\n";
                info += $"Input States: {GetInputState(1)}, {GetInputState(2)}, {GetInputState(3)}, {GetInputState(4)}\n";
                info += $"Relay States: {GetRelayState(1)}, {GetRelayState(2)}, {GetRelayState(3)}, {GetRelayState(4)}\n";
                info += $"Product Counts: {GetProductCount(0)}, {GetProductCount(1)}, {GetProductCount(2)}, {GetProductCount(3)}";

                MessageBox.Show(info, "Debug Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi hiển thị debug info: {ex.Message}");
            }
        }
    }
}