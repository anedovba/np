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
using System.Threading;//
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Client;

namespace Server
{
    public partial class Form1 : Form
    {
        //переменные которыми будем управлять
        int port;//условный числовой код нашей программы
        IPAddress ip;//адрес пк
        IPEndPoint ep;
        Socket serverListener;
        Thread t;
        string[] result = null;
        string fromUser = null;
        string toUser = null;
        string text = null;
        Database1Entities2 db = new Database1Entities2();
        public Form1()
        {
            InitializeComponent();
        }
        //включить сервер
        private void button1_Click(object sender, EventArgs e)
        {
            
            //инициализируем адрес ip
            ip = IPAddress.Parse(textBox1.Text);
            port = Convert.ToInt32(textBox2.Text);
            ep = new IPEndPoint(ip, port);

            serverListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            //активация сервера
            serverListener.Bind(ep);//связываем с конечной точкой
            serverListener.Listen(10);//максимальное кол-во в очереди на подключение
            //создание выделенного потока для рабочего метода что б не блокировать основной поток
            t = new Thread(new ThreadStart(Work));
            t.IsBackground = true;//сделать его фоновым, для его управления призавершении основного он тоже завершится
            t.Start();
            label1.Text = "Сервер включен";
            //запись в журнал
            textBox3.Text += DateTime.Now.ToString() + "->" + "Сервер включен\r\n";
            button1.Enabled = false;
            button2.Enabled = true;
        }
        //выключить сервер
        private void button2_Click(object sender, EventArgs e)
        {
            label1.Text = "Сервер выключен";
            textBox3.Text += DateTime.Now.ToString() + "->" + "Сервер выключен\r\n";
            t.Suspend();
            serverListener.Close();
            button1.Enabled = true;
            button2.Enabled = false;
        }
        //закрыть форму
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (serverListener != null)
            {
                t.Suspend();
                serverListener.Close();
            }

        }

        //рабочий метод
        void Work()
        {
            //буфферы для приема и передачи
            byte[] buffIn = new byte[1024];
            byte[] buffOut = new byte[1024];
            string messIn = "";
            string messOut = "";
            int count = 0;//счетчик байтов для правильного перекодирования

            //рабочий цикл сервера синхнонный режим
            while (true)
            {
                //сервер ожидает клиента ждет пока он постучится
                Socket xClient = serverListener.Accept();//нужен для обслуживания одного клиента
                //соединяем в потое байтов
                count = xClient.Receive(buffIn);

                messIn = Encoding.UTF8.GetString(buffIn, 0, count);//от куда раскодируем, с какого символа и количество байтов
               
                string[] stringSeparators = new string[] { " to ", ": " };
                result = messIn.Split(stringSeparators, StringSplitOptions.None);
                if (result.LongLength > 1)
                {
                    fromUser = result[0];
                    toUser = result[1];
                    text = result[2];
                }
                
                    //разрешить клиенту получать данные из базы если для него есть сообщения
                if (text == "REQUEST")
                {
                    messOut = null;
                    int toId = 0;
                      
                    foreach (var item in db.Users)
                    {
                        if (item.name.Equals(fromUser))
                        {
                            toId = item.Id;
                        }
                        
                    }
                    if (toId != 0)
                    {
                        foreach (var item in db.Messages)
                        {
                            if (toId == item.ToUserId)
                            {
                                foreach (var i in db.Users)
                                {
                                    if (i.Id == item.FromUserId)
                                    {
                                        messOut += i.name + " send you " + item.Date.ToString() + " " + item.Message + "\r\n";
                                    }
                                }
                            }
                            else
                            {
                                if (toId == item.FromUserId)
                                    foreach (var i in db.Users)
                                    {
                                        if (i.Id == item.ToUserId)
                                        {
                                            messOut += "You" + " send to " + i.name + " " + item.Date.ToString() + " " + item.Message + "\r\n";
                                        }
                                    }                                
                            } 
                        }
                        buffOut = Encoding.UTF8.GetBytes(messOut);
                        xClient.Send(buffOut);//отправляем клиенту по его запросу
                    }
                   
                }
                //занести сообшение в базу сообщений
                else
                {
                   //textBox3.Text += DateTime.Now.ToString() + "->" + messIn + "\r\n";//выводим текущую дату и сообщение
                    //проверяем есть ли уже такие пользователи если нет - добавляем в базу
                   addNewUser();
                   addMessage();
                   
                }
                //отключаем соединение
                xClient.Shutdown(SocketShutdown.Both);//с двух сторон
                //удаляем принимающий сокет
                xClient.Close();
            }
        }

        private void addMessage()
        {
            if (result.LongLength > 1)
            {
                int fromId = 0;
                int toId = 0;
                foreach (var item in db.Users)
                {
                    if (item.name.Equals(fromUser))
                    {
                        fromId = item.Id;
                    }
                    if (item.name.Equals(toUser))
                    {
                        toId = item.Id;
                    }
                }
                Messages mess = new Messages
                {
                    FromUserId = fromId,
                    Date = DateTime.Now,
                    Message = text,
                    ToUserId = toId
                };
                db.Messages.Add(mess);
                db.SaveChanges();
            }
        }

        private void addNewUser()
        {
            bool flag1 = false;
            bool flag2 = false;
            if (result.LongLength > 1)
            {
                
                foreach (var item in db.Users)
                {
                    if (item.name.Equals(fromUser))
                    {
                        flag1 = true;
                    }
                    if (item.name.Equals(toUser))
                    {
                        flag2 = true;
                    }
                    if (flag1 == true && flag2 == true)
                    {
                        break;
                    }
                }
                if (flag1 == false)
                {
                    
                    Users user = new Users
                    {
                        name = fromUser
                    };
                    db.Users.Add(user);
                    db.SaveChanges();
                }
                if (flag2 == false)
                {
                    
                    Users user = new Users
                    {
                        name = toUser
                    };
                    db.Users.Add(user);
                    db.SaveChanges();
                }
            }
        }
    }
}
