using System;
using System.Data;
using System.Windows.Forms;
using Npgsql;

namespace YourAppName
{
    public partial class Form1 : Form
    {
        private string id = "";
        private int intRow = 0;

        public Form1()
        {
            InitializeComponent();
            resetMe();
        }

        private void resetMe()
        {
            this.id = string.Empty;

            firstNameTextBox.Text = "";
            lastNameTextBox.Text = "";
            birthDateDateTimePicker.Value = DateTime.Now; // Assuming birthDate is a date column

            updateButton.Text = "Update ()";
            deleteButton.Text = "Delete ()";

            keywordTextBox.Clear();

            if (keywordTextBox.CanSelect)
            {
                keywordTextBox.Select();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            loadData("");
        }

        private void loadData(string keyword)
        {
            CRUD.sql = "SELECT personID, firstName, lastName, birthDate FROM person.person " +
                       "WHERE CONCAT(CAST(personID as varchar), ' ', firstName, ' ', lastName) LIKE @keyword::varchar " +
                       "ORDER BY personID ASC";

            string strKeyword = string.Format("%{0}%", keyword);

            CRUD.cmd = new NpgsqlCommand(CRUD.sql, CRUD.con);
            CRUD.cmd.Parameters.Clear();
            CRUD.cmd.Parameters.AddWithValue("keyword", strKeyword);

            DataTable dt = CRUD.PerformCRUD(CRUD.cmd);

            if (dt.Rows.Count > 0)
            {
                intRow = Convert.ToInt32(dt.Rows.Count.ToString());
            }
            else
            {
                intRow = 0;
            }

            toolStripStatusLabel1.Text = "Number of row(s): " + intRow.ToString();

            dataGridView1.MultiSelect = false;
            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            dataGridView1.DataSource = dt;

            dataGridView1.Columns[0].HeaderText = "ID";
            dataGridView1.Columns[1].HeaderText = "First Name";
            dataGridView1.Columns[2].HeaderText = "Last Name";
            dataGridView1.Columns[3].HeaderText = "Birth Date";

            dataGridView1.Columns[0].Width = 85;
            dataGridView1.Columns[1].Width = 170;
            dataGridView1.Columns[2].Width = 170;
            dataGridView1.Columns[3].Width = 220;
        }

        private void execute(string mySQL, string param)
        {
            CRUD.cmd = new NpgsqlCommand(mySQL, CRUD.con);
            addParameters(param);
            CRUD.PerformCRUD(CRUD.cmd);
        }

        private void addParameters(string str)
        {
            CRUD.cmd.Parameters.Clear();
            CRUD.cmd.Parameters.AddWithValue("firstName", firstNameTextBox.Text.Trim());
            CRUD.cmd.Parameters.AddWithValue("lastName", lastNameTextBox.Text.Trim());
            CRUD.cmd.Parameters.AddWithValue("birthDate", birthDateDateTimePicker.Value);

            if (str == "Update" || str == "Delete" && !string.IsNullOrEmpty(this.id))
            {
                CRUD.cmd.Parameters.AddWithValue("personID", this.id);
            }
        }

        private void insertButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(firstNameTextBox.Text.Trim()) || string.IsNullOrEmpty(lastNameTextBox.Text.Trim()))
            {
                MessageBox.Show("Please input first name and last name.", "Insert Data", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            CRUD.sql = "INSERT INTO person.person(firstName, lastName, birthDate) VALUES(@firstName, @lastName, @birthDate)";

            execute(CRUD.sql, "Insert");

            MessageBox.Show("The record has been saved.", "Insert Data", MessageBoxButtons.OK, MessageBoxIcon.Information);

            loadData("");

            resetMe();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex != -1)
            {
                DataGridView dgv1 = dataGridView1;

                this.id = Convert.ToString(dgv1.CurrentRow.Cells[0].Value);
                updateButton.Text = "Update (" + this.id + ")";
                deleteButton.Text = "Delete (" + this.id + ")";

                firstNameTextBox.Text = Convert.ToString(dgv1.CurrentRow.Cells[1].Value);
                lastNameTextBox.Text = Convert.ToString(dgv1.CurrentRow.Cells[2].Value);
                birthDateDateTimePicker.Value = Convert.ToDateTime(dgv1.CurrentRow.Cells[3].Value);
            }
        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count == 0)
            {
                return;
            }

            if (string.IsNullOrEmpty(this.id))
            {
                MessageBox.Show("Please select an item from the list.", "Update Data", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (string.IsNullOrEmpty(firstNameTextBox.Text.Trim()) || string.IsNullOrEmpty(lastNameTextBox.Text.Trim()))
            {
                MessageBox.Show("Please input first name and last name.", "Update Data", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            CRUD.sql = "UPDATE person.person SET firstName = @firstName, lastName = @lastName, birthDate = @birthDate WHERE personID = @personID::integer";

            execute(CRUD.sql, "Update");

            MessageBox.Show("The record has been updated.", "Update Data", MessageBoxButtons.OK, MessageBoxIcon.Information);

            loadData("");

            resetMe();
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count == 0)
            {
                return;
            }

            if (string.IsNullOrEmpty(this.id))
            {
                MessageBox.Show("Please select an item from the list.", "Delete Data", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (MessageBox.Show("Do you want to permanently delete the selected record?", "Delete Data", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                CRUD.sql = "DELETE FROM person.person WHERE personID = @personID::integer";

                execute(CRUD.sql, "Delete");

                MessageBox.Show("The record has been deleted.", "Delete Data", MessageBoxButtons.OK, MessageBoxIcon.Information);

                loadData("");

                resetMe();
            }
        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(keywordTextBox.Text.Trim()))
            {
                loadData("");
            }
            else
            {
                loadData(keywordTextBox.Text.Trim());
            }

            resetMe();
        }
    }

    class CRUD
    {
        private static string getConnectionString()
        {
            string host = "Host=localhost;";
            string port = "Port=5432;";
            string db = "Database=your_database_name;"; // Change 'your_database_name' to your actual database name
            string user = "Username=postgres;";
            string pass = "Password=1234;";

            string conString = string.Format("{0}{1}{2}{3}{4}", host, port, db, user, pass);
            return conString;
        }

        public static NpgsqlConnection con = new NpgsqlConnection(getConnectionString());
        public static NpgsqlCommand cmd = default(NpgsqlCommand);
        public static string sql = string.Empty;

        public static DataTable PerformCRUD(NpgsqlCommand com)
        {
            NpgsqlDataAdapter da = default(NpgsqlDataAdapter);
            DataTable dt = new DataTable();

            try
            {
                da = new NpgsqlDataAdapter();
                da.SelectCommand = com;
                da.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Perform CRUD Operations Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                dt = null;
            }

            return dt;
        }
    }
}
