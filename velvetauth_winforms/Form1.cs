namespace velvetauth_winforms
{
    public partial class Form1 : Form
    {public static MyAppSDK vauth = new MyAppSDK(
                appId: "cd1c97ad-f309-4985-9df0-f9ca5552dde6",
                secret: "A6l15Rl1CM4ZkPvE",
                version: "1.1");
        public Form1()
        {
            InitializeComponent();
           // vauth.Initialize();



    }

    private async void button1_Click(object sender, EventArgs e)
        {
            bool registered = await vauth.RegisterLicenseAsync(textBox1.Text, textBox2.Text, textBox3.Text, textBox4.Text);
            if (registered)
            {
                this.Hide();
                Form2 form2 = new Form2();
                form2.ShowDialog();
                this.Close();
            }
            else
            {
                MessageBox.Show("registration failed");
            }
          
        }
    }
}
