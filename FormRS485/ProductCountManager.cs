using FormRS485;
using System;
using System.Collections.Generic;
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
        private DataGridView dataGridViewProducts;

        // Quản lý Slave device
        private string currentSlaveID = "";
        private bool isSlaveConnected = false;

        // Timer cho việc đọc input để đếm sản phẩm
        private System.Windows.Forms.Timer productCountTimer;
        private int currentInputIndex = 0;

        // Dữ liệu sản phẩm
        private DataTable productTable;
        private List<ProductInfo> products;

        // Trạng thái input trước đó để phát hiện thay đổi
        private bool[] previousInputStates = new bool[4];
        private bool[] currentInputStates = new bool[4];

        // Event để thông báo khi có yêu cầu gửi lệnh
        public event Action<string, string> OnSendCommand;
        public event Action<bool, string> OnSlaveConnectionChanged;

        public ProductCountManager(RS485Port rs485Port, TextBox txtSlaveID, Button btnConnectSlaveID, DataGridView dataGridViewProducts)
        {
            this.rs485 = rs485Port;
            this.txtSlaveID = txtSlaveID;
            this.btnConnectSlaveID = btnConnectSlaveID;
            this.dataGridViewProducts = dataGridViewProducts;

            InitializeProductData();
            InitializeTimer();
            SetupEvents();
            InitializeDataGridView();
        }

        private void InitializeProductData()
        {
            products = new List<ProductInfo>
            {
                new ProductInfo { Name = "Sản phẩm 1", Count = 10, InputChannel = 1 },
               // new ProductInfo { Name = "Sản phẩm 2", Count = 0, InputChannel = 2 },
              //  new ProductInfo { Name = "Sản phẩm 3", Count = 0, InputChannel = 3 },
               // new ProductInfo { Name = "Sản phẩm 4", Count = 0, InputChannel = 4 }
            };

            // Khởi tạo DataTable
            productTable = new DataTable();
            productTable.Columns.Add("Sản phẩm", typeof(string));
            productTable.Columns.Add("Số lượng", typeof(int));

            UpdateProductDataGrid();
        }

        private void InitializeDataGridView()
        {
            if (dataGridViewProducts == null) return;

            dataGridViewProducts.DataSource = productTable;
            dataGridViewProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewProducts.ReadOnly = false; // Cho phép edit tên sản phẩm
            dataGridViewProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // Chỉ cho phép edit cột "Sản phẩm", không cho edit cột "Số lượng"
            if (dataGridViewProducts.Columns.Count > 1)
            {
                dataGridViewProducts.Columns["Số lượng"].ReadOnly = true;
            }

            // Định dạng header
            dataGridViewProducts.ColumnHeadersDefaultCellStyle.BackColor = Color.Navy;
            dataGridViewProducts.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridViewProducts.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 10, FontStyle.Bold);

            // Định dạng alternating rows
            dataGridViewProducts.AlternatingRowsDefaultCellStyle.BackColor = Color.LightGray;
            dataGridViewProducts.DefaultCellStyle.SelectionBackColor = Color.DarkSlateBlue;

            // Thêm context menu để xóa sản phẩm
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripMenuItem deleteItem = new ToolStripMenuItem("Xóa sản phẩm");
            deleteItem.Click += (s, e) => {
                if (dataGridViewProducts.SelectedRows.Count > 0)
                {
                    int selectedIndex = dataGridViewProducts.SelectedRows[0].Index;
                    if (MessageBox.Show("Bạn có chắc muốn xóa sản phẩm này?", "Xác nhận",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        RemoveProduct(selectedIndex);
                    }
                }
            };
            contextMenu.Items.Add(deleteItem);
            dataGridViewProducts.ContextMenuStrip = contextMenu;

            // Event khi edit cell
            dataGridViewProducts.CellEndEdit += DataGridViewProducts_CellEndEdit;
        }

        private void DataGridViewProducts_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.RowIndex >= 0 && e.RowIndex < products.Count) // Cột "Sản phẩm"
            {
                string newName = dataGridViewProducts.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString();
                if (!string.IsNullOrWhiteSpace(newName))
                {
                    EditProduct(e.RowIndex, newName);
                }
                else
                {
                    // Khôi phục tên cũ nếu để trống
                    dataGridViewProducts.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = products[e.RowIndex].Name;
                }
            }
        }

        private void InitializeTimer()
        {
            productCountTimer = new System.Windows.Forms.Timer();
            productCountTimer.Interval = 200; // 200ms để quét nhanh các input
            productCountTimer.Tick += ProductCountTimer_Tick;
        }

        private void SetupEvents()
        {
            if (txtSlaveID != null)
            {
                txtSlaveID.Text = currentSlaveID;
            }

            if (btnConnectSlaveID != null)
            {
                btnConnectSlaveID.Text = "Kết nối slave";
                btnConnectSlaveID.Click += BtnConnectSlaveID_Click;
            }
        }

        private void BtnConnectSlaveID_Click(object sender, EventArgs e)
        {
            if (!rs485.IsOpen)
            {
                MessageBox.Show("Vui lòng kết nối RS485 trước!");
                return;
            }

            if (!isSlaveConnected)
            {
                ConnectToSlave();
            }
            else
            {
                DisconnectFromSlave();
            }
        }

        private void ConnectToSlave()
        {
            try
            {
                string slaveID = txtSlaveID?.Text?.Trim() ?? "01";
                if (string.IsNullOrEmpty(slaveID))
                {
                    MessageBox.Show("Vui lòng nhập Slave ID!");
                    return;
                }

                currentSlaveID = slaveID.PadLeft(2, '0');

                // Gửi lệnh test kết nối đến slave
                string testCommand = GenerateTestSlaveCommand(currentSlaveID);
                OnSendCommand?.Invoke(testCommand, $"TEST_SLAVE_{currentSlaveID}");

                Console.WriteLine($"Đang kết nối đến Slave {currentSlaveID}...");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối slave: {ex.Message}");
            }
        }

        private void DisconnectFromSlave()
        {
            isSlaveConnected = false;
            productCountTimer.Stop();

            if (btnConnectSlaveID != null)
            {
                btnConnectSlaveID.Text = "Kết nối slave";
                btnConnectSlaveID.BackColor = SystemColors.Control;
            }

            OnSlaveConnectionChanged?.Invoke(false, currentSlaveID);
            Console.WriteLine($"Đã ngắt kết nối Slave {currentSlaveID}");
        }

        private string GenerateTestSlaveCommand(string slaveID)
        {
            // Sử dụng lệnh đọc trạng thái 4 input để test kết nối
            // Format: [SlaveID] 02 00 00 00 04 [CRC]
            string baseCommand = $"{slaveID} 02 00 00 00 04";
            return CalculateCRCAndFormat(baseCommand);
        }

        private void ProductCountTimer_Tick(object sender, EventArgs e)
        {
            if (!isSlaveConnected || !rs485.IsOpen) return;

            try
            {
                // Đọc từng input để đếm sản phẩm
                string readCommand = GenerateInputReadCommand(currentSlaveID, currentInputIndex + 1);
                if (!string.IsNullOrEmpty(readCommand))
                {
                    string commandId = $"READ_COUNT_{currentInputIndex + 1}";
                    OnSendCommand?.Invoke(readCommand, commandId);
                }

                // Chuyển sang input tiếp theo
                currentInputIndex = (currentInputIndex + 1) % 4;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi đọc input cho đếm sản phẩm: {ex.Message}");
            }
        }

        private string GenerateInputReadCommand(string slaveID, int inputNumber)
        {
            // Tạo lệnh đọc input theo Modbus protocol từ tài liệu
            string baseCommand = "";
            switch (inputNumber)
            {
                case 1: baseCommand = $"{slaveID} 02 00 00 00 01"; break;
                case 2: baseCommand = $"{slaveID} 02 00 01 00 01"; break;
                case 3: baseCommand = $"{slaveID} 02 00 02 00 01"; break;
                case 4: baseCommand = $"{slaveID} 02 00 03 00 01"; break;
                default: return "";
            }

            return CalculateCRCAndFormat(baseCommand);
        }

        public void ProcessSlaveTestResponse(string hexData, string lastCommand)
        {
            if (string.IsNullOrEmpty(hexData)) return;

            try
            {
                string cleanHex = hexData.Replace(" ", "").ToUpper();

                // Kiểm tra phản hồi có hợp lệ không
                if (cleanHex.StartsWith(currentSlaveID.ToUpper()) && cleanHex.Length >= 8)
                {
                    isSlaveConnected = true;
                    productCountTimer.Start();

                    if (btnConnectSlaveID != null)
                    {
                        btnConnectSlaveID.Text = "Ngắt kết nối";
                        btnConnectSlaveID.BackColor = Color.LightGreen;
                    }

                    OnSlaveConnectionChanged?.Invoke(true, currentSlaveID);
                    MessageBox.Show($"Kết nối thành công với Slave {currentSlaveID}!");

                    Console.WriteLine($"Slave {currentSlaveID} connected successfully");
                }
                else
                {
                    MessageBox.Show($"Không thể kết nối với Slave {currentSlaveID}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xử lý phản hồi test slave: {ex.Message}");
                MessageBox.Show($"Lỗi kết nối Slave: {ex.Message}");
            }
        }

        public void ProcessCountResponse(string hexData, string lastCommand)
        {
            if (string.IsNullOrEmpty(hexData) || !lastCommand.StartsWith("READ_COUNT_")) return;

            try
            {
                string cleanHex = hexData.Replace(" ", "").ToUpper();

                // Phản hồi đọc input: [SlaveID] 02 01 [Status] [CRC]
                if (cleanHex.StartsWith(currentSlaveID.ToUpper()) && cleanHex.Length >= 10)
                {
                    string statusByte = cleanHex.Substring(6, 2);
                    int inputNumber = int.Parse(lastCommand.Replace("READ_COUNT_", ""));
                    int inputIndex = inputNumber - 1;

                    int status = Convert.ToInt32(statusByte, 16);
                    bool inputState = (status & 0x01) != 0;

                    // Lưu trạng thái hiện tại
                    previousInputStates[inputIndex] = currentInputStates[inputIndex];
                    currentInputStates[inputIndex] = inputState;

                    // Phát hiện edge từ OFF sang ON (sản phẩm đi qua)
                    if (!previousInputStates[inputIndex] && currentInputStates[inputIndex])
                    {
                        IncrementProductCount(inputNumber);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xử lý dữ liệu đếm sản phẩm: {ex.Message}");
            }
        }

        private void IncrementProductCount(int inputChannel)
        {
            try
            {
                var product = products.Find(p => p.InputChannel == inputChannel);
                if (product != null)
                {
                    product.Count++;
                    UpdateProductDataGrid();

                    Console.WriteLine($"Sản phẩm {product.Name}: {product.Count}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tăng số lượng sản phẩm: {ex.Message}");
            }
        }

        private void UpdateProductDataGrid()
        {
            try
            {
                if (productTable == null) return;

                productTable.Rows.Clear();
                foreach (var product in products)
                {
                    productTable.Rows.Add(product.Name, product.Count);
                }

                if (dataGridViewProducts != null)
                {
                    dataGridViewProducts.Refresh();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi cập nhật DataGrid: {ex.Message}");
            }
        }

        private string CalculateCRCAndFormat(string baseCommand)
        {
            // Đây là implementation đơn giản, trong thực tế cần tính CRC16 chính xác
            // Tạm thời sử dụng các lệnh có sẵn từ tài liệu
            string cmd = baseCommand.Replace(" ", "");

            // Mapping với các lệnh có sẵn từ tài liệu PDF
            if (cmd.StartsWith("FE"))
            {
                switch (cmd)
                {
                    case "FE020000000001": return "FE 02 00 00 00 01 AD C5";
                    case "FE020001000001": return "FE 02 00 01 00 01 FC 05";
                    case "FE020002000001": return "FE 02 00 02 00 01 0C 05";
                    case "FE020003000001": return "FE 02 00 03 00 01 5D C5";
                    case "FE020000000004": return "FE 02 00 00 00 04 6D C6";
                }
            }

            // Trả về command gốc nếu không tìm thấy mapping
            return baseCommand;
        }

        // Public methods
        public void ResetAllCounts()
        {
            foreach (var product in products)
            {
                product.Count = 0;
            }
            UpdateProductDataGrid();
            MessageBox.Show("Đã reset tất cả số lượng sản phẩm!");
        }

        public void AddProduct(string productName)
        {
            if (products.Count >= 4)
            {
                MessageBox.Show("Chỉ hỗ trợ tối đa 4 loại sản phẩm (4 kênh input)!");
                return;
            }

            if (string.IsNullOrWhiteSpace(productName))
            {
                MessageBox.Show("Tên sản phẩm không được để trống!");
                return;
            }

            // Kiểm tra trùng tên
            if (products.Exists(p => p.Name.Equals(productName.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Sản phẩm đã tồn tại!");
                return;
            }

            var newProduct = new ProductInfo
            {
                Name = productName.Trim(),
                Count = 0,
                InputChannel = products.Count + 1
            };

            products.Add(newProduct);
            UpdateProductDataGrid();

            MessageBox.Show($"Đã thêm sản phẩm: {productName.Trim()} (Kênh {newProduct.InputChannel})");
            Console.WriteLine($"Added product: {productName} on channel {newProduct.InputChannel}");
        }

        public void RemoveProduct(int productIndex)
        {
            if (productIndex >= 0 && productIndex < products.Count)
            {
                string productName = products[productIndex].Name;
                products.RemoveAt(productIndex);

                // Cập nhật lại input channel cho các sản phẩm còn lại
                for (int i = 0; i < products.Count; i++)
                {
                    products[i].InputChannel = i + 1;
                }

                UpdateProductDataGrid();
                MessageBox.Show($"Đã xóa sản phẩm: {productName}");
            }
        }

        public void EditProduct(int productIndex, string newName)
        {
            if (productIndex >= 0 && productIndex < products.Count)
            {
                if (string.IsNullOrWhiteSpace(newName))
                {
                    MessageBox.Show("Tên sản phẩm không được để trống!");
                    return;
                }

                // Kiểm tra trùng tên (trừ sản phẩm hiện tại)
                if (products.Exists(p => p.Name.Equals(newName.Trim(), StringComparison.OrdinalIgnoreCase)
                    && products.IndexOf(p) != productIndex))
                {
                    MessageBox.Show("Tên sản phẩm đã tồn tại!");
                    return;
                }

                string oldName = products[productIndex].Name;
                products[productIndex].Name = newName.Trim();
                UpdateProductDataGrid();

                MessageBox.Show($"Đã đổi tên từ '{oldName}' thành '{newName.Trim()}'");
            }
        }

        public int GetProductCount(int productIndex)
        {
            if (productIndex >= 0 && productIndex < products.Count)
            {
                return products[productIndex].Count;
            }
            return 0;
        }

        public void Dispose()
        {
            productCountTimer?.Stop();
            productCountTimer?.Dispose();
        }

        // Properties
        public bool IsSlaveConnected => isSlaveConnected;
        public string CurrentSlaveID => currentSlaveID;
    }

    // Helper class
    public class ProductInfo
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public int InputChannel { get; set; }
    }
}