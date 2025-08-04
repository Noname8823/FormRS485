using FormRS485;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace projectRS485
{
    public class InputManager
    {
        private RS485Port rs485;
        private TextBox txtIn1, txtIn2, txtIn3, txtIn4;
        
        // Trạng thái input
        private bool input1State = false, input2State = false, input3State = false, input4State = false;
        
        // Timer và biến điều khiển
        private System.Windows.Forms.Timer inputReadTimer;
        private int currentInputToRead = 1;
        
        // Event để thông báo khi có yêu cầu gửi lệnh
        public event Action<string, string> OnSendCommand;
        
        public InputManager(RS485Port rs485Port, TextBox txtIn1, TextBox txtIn2, TextBox txtIn3, TextBox txtIn4)
        {
            this.rs485 = rs485Port;
            this.txtIn1 = txtIn1;
            this.txtIn2 = txtIn2;
            this.txtIn3 = txtIn3;
            this.txtIn4 = txtIn4;
            
            InitializeInputTimer();
            InitializeInputStates();
        }
        
        private void InitializeInputTimer()
        {
            inputReadTimer = new System.Windows.Forms.Timer();
            inputReadTimer.Interval = 500; // 500ms để đọc nhanh qua 4 input
            inputReadTimer.Tick += InputReadTimer_Tick;
        }
        
        private void InitializeInputStates()
        {
            InitializeTextBox(txtIn1, "OFF");
            InitializeTextBox(txtIn2, "OFF");
            InitializeTextBox(txtIn3, "OFF");
            InitializeTextBox(txtIn4, "OFF");
        }
        
        private void InitializeTextBox(TextBox box, string text)
        {
            if (box != null)
            {
                box.Text = text;
                UpdateInputColors(box);
            }
        }
        
        private void InputReadTimer_Tick(object sender, EventArgs e)
        {
            if (!rs485.IsOpen) return;

            try
            {
                // Đọc từng input theo chu kỳ
                string readCommand = GetInputReadCommand(currentInputToRead);
                if (!string.IsNullOrEmpty(readCommand))
                {
                    string commandId = $"READ_INPUT_{currentInputToRead}";
                    OnSendCommand?.Invoke(readCommand, commandId);
                }

                // Chuyển sang input tiếp theo
                currentInputToRead = (currentInputToRead % 4) + 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi đọc input: {ex.Message}");
            }
        }
        
        private string GetInputReadCommand(int inputNumber)
        {
            // Lệnh Modbus đọc trạng thái từng input (function code 02 - Read Discrete Inputs)
            switch (inputNumber)
            {
                case 1: return "FE 02 00 00 00 01 AD C5"; // Đọc input 1
                case 2: return "FE 02 00 01 00 01 FC 05"; // Đọc input 2  
                case 3: return "FE 02 00 02 00 01 0C 05"; // Đọc input 3
                case 4: return "FE 02 00 03 00 01 5D C5"; // Đọc input 4
                default: return "";
            }
        }
        
        public void ProcessInputResponse(string hexData, string lastCommand)
        {
            if (string.IsNullOrEmpty(hexData) || !lastCommand.StartsWith("READ_INPUT_")) return;

            try
            {
                // Loại bỏ khoảng trắng và chuyển về chữ hoa
                string cleanHex = hexData.Replace(" ", "").ToUpper();

                // Phản hồi đọc input có format: FE 02 01 XX CRC_LO CRC_HI
                if (cleanHex.StartsWith("FE02") && cleanHex.Length >= 10)
                {
                    string statusByte = cleanHex.Substring(6, 2); // Lấy byte trạng thái
                    int inputNumber = int.Parse(lastCommand.Replace("READ_INPUT_", ""));

                    // Chuyển đổi hex thành int để kiểm tra bit
                    int status = Convert.ToInt32(statusByte, 16);
                    bool inputState = (status & 0x01) != 0; // Kiểm tra bit 0

                    UpdateInputState(inputNumber, inputState);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi phân tích dữ liệu input hex: {ex.Message}");
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
        
        public void StartReading()
        {
            inputReadTimer.Start();
        }
        
        public void StopReading()
        {
            inputReadTimer.Stop();
        }
        
        public void Dispose()
        {
            if (inputReadTimer != null)
            {
                inputReadTimer.Stop();
                inputReadTimer.Dispose();
            }
        }
        
        // Properties để truy cập trạng thái input từ bên ngoài
        public bool Input1State => input1State;
        public bool Input2State => input2State;
        public bool Input3State => input3State;
        public bool Input4State => input4State;
    }
}