using FormRS485;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace projectRS485
{
    public class OutputManager
    {
        private RS485Port rs485;
        private Button btnOut1, btnOut2, btnOut3, btnOut4;
        private TextBox txtOut1, txtOut2, txtOut3, txtOut4;

        // Trạng thái relay
        private bool relay1State = false, relay2State = false, relay3State = false, relay4State = false;

        // Event để thông báo khi có yêu cầu gửi lệnh
        public event Action<string, string> OnSendCommand;

        public OutputManager(RS485Port rs485Port,
            Button btnOut1, Button btnOut2, Button btnOut3, Button btnOut4,
            TextBox txtOut1, TextBox txtOut2, TextBox txtOut3, TextBox txtOut4)
        {
            this.rs485 = rs485Port;
            this.btnOut1 = btnOut1;
            this.btnOut2 = btnOut2;
            this.btnOut3 = btnOut3;
            this.btnOut4 = btnOut4;
            this.txtOut1 = txtOut1;
            this.txtOut2 = txtOut2;
            this.txtOut3 = txtOut3;
            this.txtOut4 = txtOut4;

            InitializeOutputStates();
            SetupButtonEvents();
        }

        private void InitializeOutputStates()
        {
            InitializeTextBox(txtOut1, "OFF");
            InitializeTextBox(txtOut2, "OFF");
            InitializeTextBox(txtOut3, "OFF");
            InitializeTextBox(txtOut4, "OFF");

            UpdateButtonState(btnOut1, "Relay 1", relay1State);
            UpdateButtonState(btnOut2, "Relay 2", relay2State);
            UpdateButtonState(btnOut3, "Relay 3", relay3State);
            UpdateButtonState(btnOut4, "Relay 4", relay4State);
        }

        private void InitializeTextBox(TextBox box, string text)
        {
            if (box != null)
            {
                box.Text = text;
                UpdateOutputColors(box);
            }
        }

        private void SetupButtonEvents()
        {
            if (btnOut1 != null) btnOut1.Click += (s, e) => ToggleRelay1();
            if (btnOut2 != null) btnOut2.Click += (s, e) => ToggleRelay2();
            if (btnOut3 != null) btnOut3.Click += (s, e) => ToggleRelay3();
            if (btnOut4 != null) btnOut4.Click += (s, e) => ToggleRelay4();
        }

        private void ToggleRelay1()
        {
            ToggleRelay(ref relay1State, "Relay 1",
                "FE 05 00 00 FF 00 98 35", // ON command
                "FE 05 00 00 00 00 D9 C5", // OFF command
                btnOut1, txtOut1);
        }

        private void ToggleRelay2()
        {
            ToggleRelay(ref relay2State, "Relay 2",
                "FE 05 00 01 FF 00 C9 F5", // ON command
                "FE 05 00 01 00 00 88 05", // OFF command
                btnOut2, txtOut2);
        }

        private void ToggleRelay3()
        {
            ToggleRelay(ref relay3State, "Relay 3",
                "FE 05 00 02 FF 00 39 F5", // ON command
                "FE 05 00 02 00 00 78 05", // OFF command
                btnOut3, txtOut3);
        }

        private void ToggleRelay4()
        {
            ToggleRelay(ref relay4State, "Relay 4",
                "FE 05 00 03 FF 00 68 35", // ON command
                "FE 05 00 03 00 00 29 C5", // OFF command
                btnOut4, txtOut4);
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

                OnSendCommand?.Invoke(cmd, $"RELAY_{name.Replace(" ", "_")}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi điều khiển {name}: {ex.Message}");
            }
        }

        private void UpdateButtonState(Button button, string name, bool state)
        {
            if (button == null) return;
            button.UseVisualStyleBackColor = false;
            button.BackColor = state ? Color.LimeGreen : Color.LightGray;
            button.Text = $"{name}: {(state ? "ON" : "OFF")}";
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

        public void ProcessOutputResponse(string hexData, string lastCommand)
        {
            // Xử lý phản hồi từ relay nếu cần
            // Thường thì relay sẽ echo lại lệnh đã gửi để xác nhận
            try
            {
                if (!string.IsNullOrEmpty(hexData) && lastCommand.StartsWith("RELAY_"))
                {
                    Console.WriteLine($"Relay response: {hexData} for command: {lastCommand}");
                    // Có thể thêm logic xử lý phản hồi relay ở đây
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi phân tích dữ liệu output hex: {ex.Message}");
            }
        }

        // Properties để truy cập trạng thái relay từ bên ngoài
        public bool Relay1State => relay1State;
        public bool Relay2State => relay2State;
        public bool Relay3State => relay3State;
        public bool Relay4State => relay4State;

        // Method để set trạng thái relay từ bên ngoài (ví dụ khi khôi phục trạng thái)
        public void SetRelayState(int relayNumber, bool state)
        {
            switch (relayNumber)
            {
                case 1:
                    relay1State = state;
                    UpdateButtonState(btnOut1, "Relay 1", state);
                    if (txtOut1 != null) { txtOut1.Text = state ? "ON" : "OFF"; UpdateOutputColors(txtOut1); }
                    break;
                case 2:
                    relay2State = state;
                    UpdateButtonState(btnOut2, "Relay 2", state);
                    if (txtOut2 != null) { txtOut2.Text = state ? "ON" : "OFF"; UpdateOutputColors(txtOut2); }
                    break;
                case 3:
                    relay3State = state;
                    UpdateButtonState(btnOut3, "Relay 3", state);
                    if (txtOut3 != null) { txtOut3.Text = state ? "ON" : "OFF"; UpdateOutputColors(txtOut3); }
                    break;
                case 4:
                    relay4State = state;
                    UpdateButtonState(btnOut4, "Relay 4", state);
                    if (txtOut4 != null) { txtOut4.Text = state ? "ON" : "OFF"; UpdateOutputColors(txtOut4); }
                    break;
            }
        }
    }
}