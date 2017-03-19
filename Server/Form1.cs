using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;//подключить
using System.Net.Sockets;//


namespace Client
{
    public partial class Form1 : Form
    {
        //переменные которыми будем управлять
        int port;//условный числовой код нашей программы
        IPAddress ip;//адрес пк
        IPEndPoint ep;
        Socket client;

        byte[] buffIn;
        byte[] buffOut;
        string messIn;
        string messOut;
        int count;//счетчик байтов для правильного перекодирования

        public Form1()
        {
            try
            {
                InitializeComponent();
                //буфферы для приема и передачи
                buffIn = new byte[4024];
                buffOut = new byte[1024];
                messIn = "";
                messOut = "";
                count = 0;//счетчик байтов для правильного перекодирования
                button2.Enabled = false;

            }
            catch (Exception ex)
            {
                textBox5.Text = ex.Message;
            }

        }
        //подключиться к серверу
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                ip = IPAddress.Parse(textBox1.Text);
                port = Convert.ToInt32(textBox2.Text);
                ep = new IPEndPoint(ip, port);

                button1.Enabled = false;
                button2.Enabled = true;
                button3.Enabled = true;


                timer1.Start();
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                client.Connect(ep);
                textBox5.Text = "Вы подключились к Messanger";
                messOut = textBox3.Text + " присоединился к чату";
                buffOut = Encoding.UTF8.GetBytes(messOut);
                client.Send(buffOut);
                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
            catch (Exception ex)
            {
                textBox5.Text = ex.Message;
                button1.Enabled = true;
                button2.Enabled = false;
                button3.Enabled = false;

            }
        }
        //отключиться от сервера
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                button1.Enabled = true;
                button2.Enabled = false;
                button3.Enabled = false;

                timer1.Stop();
                textBox5.Clear();
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                client.Connect(ep);
                textBox5.Text = "Вы отключились от Messanger";
                messOut = textBox3.Text + " отсоединился от чата";
                buffOut = Encoding.UTF8.GetBytes(messOut);
                client.Send(buffOut);
                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
            catch (Exception ex)
            {
                textBox5.Text = ex.Message;
            }
        }
        //отправить сообщение
        private void button3_Click(object sender, EventArgs e)
        {

            try
            {
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                client.Connect(ep);
                messOut = textBox3.Text + " to " + textBox6.Text + ": " + textBox4.Text;
                textBox4.Clear();
                buffOut = Encoding.UTF8.GetBytes(messOut);
                client.Send(buffOut);
                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
            catch (Exception ex)
            {
                textBox5.Text = ex.Message;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                textBox5.Clear();
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                client.Connect(ep);
                messOut = textBox3.Text + " to " + textBox6.Text + ": " + "REQUEST";
                buffOut = Encoding.UTF8.GetBytes(messOut);
                client.Send(buffOut);

                count = client.Receive(buffIn);
                messIn = Encoding.UTF8.GetString(buffIn, 0, count);
                textBox5.Text = messIn;

                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
            catch (Exception ex)
            {
                textBox5.Text = ex.Message;
            }
        }

        private void textBox4_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button3.PerformClick();
            }
        }
    }
}

