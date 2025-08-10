using FormRS485;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace projectRS485
{
    public class ProductCountManager
    {
        private RS485Port rs485;
        private TextBox txtSlaveID;
        private Button btnConnectSlaveID;
        private DataGridView dgvProducts;
        private InputManager inputManager;

        // Slave ID hiện tại
        private byte currentSlaveID = 0xFE; // Mặc định broadcast address
        private bool isSlaveConnected = false;

        // Trạng thái đếm sản phẩm
        private bool[] previousInputStates = new bool[4];
        private int[] productCounts = new int[4]; // Đếm cho từng input

        // Event để thông báo khi có yêu cầu gửi lệnh
        public event Action<string, string> OnSendCommand;

        public ProductCountManager(RS485Port rs485Port, TextBox txtSlaveID, Button btnConnectSlaveID,
                                   DataGridView dgvProducts, InputManager inputManager)
        {
            this.rs485 = rs485Port;
            this.txtSlaveID = txtSlaveID;
            this.btnConnectSlaveID = btnConnectSlaveID;
            this.dgvProducts = dgvProducts;
            this.inputManager = inputManager;

            InitializeControls();
            InitializeDataGridView();

            // Đăng ký timer kiểm tra thay đổi trạng thái input để đếm sản phẩm
            if (inputManager != null)
            {
                var checkTimer = new System.Windows.Forms.Timer();
                checkTimer.Interval = 100; // Kiểm tra mỗi 100ms
                checkTimer.Tick += CheckInputChanges;
                checkTimer.Start();
            }
        }

        private void InitializeControls()
        {
            if (txtSlaveID != null)
            {
                txtSlaveID.Text = "254"; // Mặc định broadcast address
                txtSlaveID.MaxLength = 3;
                txtSlaveID.KeyPress += TxtSlaveID_KeyPress;
            }

            if (btnConnectSlaveID != null)
            {
                btnConnectSlaveID.Text = "Kết nối Slave ID";
                btnConnectSlaveID.UseVisualStyleBackColor = false;
                btnConnectSlaveID.BackColor = Color.LightGray;
                btnConnectSlaveID.Click += BtnConnectSlaveID_Click;
            }
        }

        private void InitializeDataGridView()
        {
            if (dgvProducts == null) return;

            try
            {
                dgvProducts.Columns.Clear();

                // Nền & chữ mặc định
                dgvProducts.BackgroundColor = Color.White;
                dgvProducts.DefaultCellStyle.BackColor = Color.White;
                dgvProducts.DefaultCellStyle.ForeColor = Color.Black;

                // Tắt highlight khi chọn (Selection màu trùng nền)
                dgvProducts.DefaultCellStyle.SelectionBackColor = dgvProducts.DefaultCellStyle.BackColor;
                dgvProducts.DefaultCellStyle.SelectionForeColor = dgvProducts.DefaultCellStyle.ForeColor;
                dgvProducts.RowsDefaultCellStyle.SelectionBackColor = dgvProducts.DefaultCellStyle.BackColor;
                dgvProducts.RowsDefaultCellStyle.SelectionForeColor = dgvProducts.DefaultCellStyle.ForeColor;
                dgvProducts.AlternatingRowsDefaultCellStyle.SelectionBackColor = dgvProducts.DefaultCellStyle.BackColor;
                dgvProducts.AlternatingRowsDefaultCellStyle.SelectionForeColor = dgvProducts.DefaultCellStyle.ForeColor;

                // Header (giữ nguyên style header, không ảnh hưởng)
                dgvProducts.EnableHeadersVisualStyles = false;
                dgvProducts.ColumnHeadersDefaultCellStyle.BackColor = Color.Navy;
                dgvProducts.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                dgvProducts.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Bold);

                // Tạo cột
                dgvProducts.Columns.Add("InputChannel", "Kênh Input");
                dgvProducts.Columns.Add("ProductName", "Tên Sản Phẩm");
                dgvProducts.Columns.Add("Count", "Số Lượng");
                //dgvProducts.Columns.Add("Status", "Trạng Thái");

                // Thuộc tính cột
                dgvProducts.Columns["InputChannel"].Width = 80;
                dgvProducts.Columns["InputChannel"].ReadOnly = true;
                dgvProducts.Columns["ProductName"].Width = 200;
                dgvProducts.Columns["Count"].Width = 100;
                dgvProducts.Columns["Count"].ReadOnly = true;
                //dgvProducts.Columns["Status"].Width = 100;
               // dgvProducts.Columns["Status"].ReadOnly = true;

                // Thêm 4 hàng
                for (int i = 1; i <= 4; i++)
                {
                    dgvProducts.Rows.Add($"Input {i}", $"Sản phẩm {i}", "0", "OFF");
                }

                // Sự kiện chỉnh sửa
                dgvProducts.CellEndEdit += DgvProducts_CellEndEdit;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khởi tạo DataGridView: {ex.Message}");
            }
        }

        private void TxtSlaveID_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Chỉ cho phép nhập số và phím điều khiển
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void BtnConnectSlaveID_Click(object sender, EventArgs e)
        {
            try
            {
                if (!rs485.IsOpen)
                {
                    MessageBox.Show("Vui lòng kết nối cổng RS485 trước!", "Lỗi",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtSlaveID.Text))
                {
                    MessageBox.Show("Vui lòng nhập Slave ID!", "Lỗi",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int slaveID;
                if (!int.TryParse(txtSlaveID.Text, out slaveID) || slaveID < 1 || slaveID > 254)
                {
                    MessageBox.Show("Slave ID phải từ 1-254!", "Lỗi",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                currentSlaveID = (byte)slaveID;
                isSlaveConnected = true;

                // Cập nhật UI
                btnConnectSlaveID.BackColor = Color.LightGreen;
                btnConnectSlaveID.Text = $"Đã kết nối (ID: {slaveID})";
                txtSlaveID.ReadOnly = true;

                // Test kết nối
                TestSlaveConnection();

                MessageBox.Show($"Đã kết nối với Slave ID: {slaveID}", "Thành công",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối Slave ID: {ex.Message}", "Lỗi",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TestSlaveConnection()
        {
            try
            {
                string command = GenerateModbusCommand(currentSlaveID, 0x01, 0x0000, 0x0004);
                OnSendCommand?.Invoke(command, "TEST_SLAVE_CONNECTION");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi test kết nối slave: {ex.Message}");
            }
        }

        private void DgvProducts_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (dgvProducts.Columns[e.ColumnIndex].Name == "ProductName")
                {
                    string productName = dgvProducts.Rows[e.RowIndex].Cells["ProductName"].Value == null
                        ? null
                        : dgvProducts.Rows[e.RowIndex].Cells["ProductName"].Value.ToString();

                    if (string.IsNullOrWhiteSpace(productName))
                    {
                        dgvProducts.Rows[e.RowIndex].Cells["ProductName"].Value = $"Sản phẩm {e.RowIndex + 1}";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi chỉnh sửa cell: {ex.Message}");
            }
        }

        private void CheckInputChanges(object sender, EventArgs e)
        {
            if (!isSlaveConnected || inputManager == null) return;

            try
            {
                bool[] currentStates = {
                    inputManager.Input1State,
                    inputManager.Input2State,
                    inputManager.Input3State,
                    inputManager.Input4State
                };

                for (int i = 0; i < 4; i++)
                {
                    // Đếm cạnh lên (rising edge)
                    if (!previousInputStates[i] && currentStates[i])
                    {
                        productCounts[i]++;
                        UpdateProductCount(i, productCounts[i]);
                    }

                    // Cập nhật trạng thái (không đổi màu)
                    UpdateInputStatus(i, currentStates[i]);

                    previousInputStates[i] = currentStates[i];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi kiểm tra thay đổi input: {ex.Message}");
            }
        }

        private void UpdateProductCount(int inputIndex, int count)
        {
            try
            {
                if (dgvProducts != null && inputIndex >= 0 && inputIndex < dgvProducts.Rows.Count)
                {
                    dgvProducts.Rows[inputIndex].Cells["Count"].Value = count.ToString();

                    // KHÔNG highlight: bỏ đổi màu & bỏ timer xóa highlight
                    // (cố ý để trống)
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi cập nhật số đếm sản phẩm: {ex.Message}");
            }
        }

        private void UpdateInputStatus(int inputIndex, bool state)
        {
            try
            {
                if (dgvProducts != null && inputIndex >= 0 && inputIndex < dgvProducts.Rows.Count)
                {
                    // Chỉ cập nhật text, KHÔNG đổi màu
                    string status = state ? "ON" : "OFF";
                    dgvProducts.Rows[inputIndex].Cells["Status"].Value = status;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi cập nhật trạng thái input: {ex.Message}");
            }
        }

        // Tạo lệnh Modbus với Slave ID cụ thể
        private string GenerateModbusCommand(byte slaveID, byte functionCode, ushort startAddress, ushort quantity)
        {
            try
            {
                byte[] command = new byte[8];
                command[0] = slaveID;
                command[1] = functionCode;
                command[2] = (byte)(startAddress >> 8);
                command[3] = (byte)(startAddress & 0xFF);
                command[4] = (byte)(quantity >> 8);
                command[5] = (byte)(quantity & 0xFF);

                // Tính CRC16
                ushort crc = CalculateCRC16(command, 6);
                command[6] = (byte)(crc & 0xFF);
                command[7] = (byte)(crc >> 8);

                return RS485Port.ByteArrayToHexString(command);
            }
            catch
            {
                return "";
            }
        }

        // Tính CRC16 cho Modbus
        private ushort CalculateCRC16(byte[] data, int length)
        {
            ushort crc = 0xFFFF;
            for (int i = 0; i < length; i++)
            {
                crc ^= data[i];
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x0001) == 1)
                        crc = (ushort)((crc >> 1) ^ 0xA001);
                    else
                        crc = (ushort)(crc >> 1);
                }
            }
            return crc;
        }

        // Reset số đếm sản phẩm
        public void ResetProductCounts()
        {
            try
            {
                for (int i = 0; i < 4; i++)
                {
                    productCounts[i] = 0;
                    if (dgvProducts != null && i < dgvProducts.Rows.Count)
                    {
                        dgvProducts.Rows[i].Cells["Count"].Value = "0";
                    }
                }
                MessageBox.Show("Đã reset tất cả số đếm sản phẩm!", "Thông báo",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi reset số đếm: {ex.Message}");
            }
        }

        // Reset số đếm cho input cụ thể
        public void ResetProductCount(int inputIndex)
        {
            try
            {
                if (inputIndex >= 0 && inputIndex < 4)
                {
                    productCounts[inputIndex] = 0;
                    if (dgvProducts != null && inputIndex < dgvProducts.Rows.Count)
                    {
                        dgvProducts.Rows[inputIndex].Cells["Count"].Value = "0";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi reset số đếm input {inputIndex}: {ex.Message}");
            }
        }

        // Ngắt kết nối Slave
        public void DisconnectSlave()
        {
            try
            {
                isSlaveConnected = false;
                currentSlaveID = 0xFE;

                if (btnConnectSlaveID != null)
                {
                    btnConnectSlaveID.BackColor = Color.LightGray;
                    btnConnectSlaveID.Text = "Kết nối Slave ID";
                }

                if (txtSlaveID != null)
                {
                    txtSlaveID.ReadOnly = false;
                }

                // Reset số đếm
                ResetProductCounts();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi ngắt kết nối slave: {ex.Message}");
            }
        }

        // Properties
        public bool IsSlaveConnected { get { return isSlaveConnected; } }
        public byte CurrentSlaveID { get { return currentSlaveID; } }
        public int[] ProductCounts { get { return (int[])productCounts.Clone(); } }

        // Lấy tên sản phẩm từ DataGridView
        public string GetProductName(int inputIndex)
        {
            try
            {
                if (dgvProducts != null && inputIndex >= 0 && inputIndex < dgvProducts.Rows.Count)
                {
                    object val = dgvProducts.Rows[inputIndex].Cells["ProductName"].Value;
                    return val == null ? ("Sản phẩm " + (inputIndex + 1)) : val.ToString();
                }
                return "Sản phẩm " + (inputIndex + 1);
            }
            catch
            {
                return "Sản phẩm " + (inputIndex + 1);
            }
        }
    }
}
