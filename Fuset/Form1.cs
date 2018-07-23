using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;

namespace Fuset
{
    
    public partial class Form1 : Form
    {
        string PuthToPicture;
        int old_RP = 0;
        int spred_BTC = 0;
        int spred_RP = 0;
        int Time = 0;
        int miss = 0;
        bool bad_ip = false;
        bool busy = false;
        IWebElement logo;
        String logoSRC;
        Uri imageURL;
        List<int> timing_list = new List<int>();
        ChromeOptions options;

        private String dbFileName = "sample.sqlite";
        private SQLiteConnection m_dbConn;
        private SQLiteCommand m_sqlCmd;
        SQLiteDataReader sqlite_datareader;

        public void get_timing_list()
        {
            m_sqlCmd = m_dbConn.CreateCommand();
            m_sqlCmd.CommandText = "SELECT Count(*) FROM Setting";
            sqlite_datareader = m_sqlCmd.ExecuteReader();
            sqlite_datareader.Read();

            

            timing_list = new List<int>(sqlite_datareader.GetInt32(0));
            sqlite_datareader.Close();


            m_sqlCmd.CommandText = @"SELECT id FROM Setting";
            m_sqlCmd.CommandType = CommandType.Text;
            SQLiteDataReader reader = m_sqlCmd.ExecuteReader();

            while (reader.Read()) // построчно считываем данные
            {
                //object id = reader.GetValue(0);
                timing_list.Insert(Convert.ToInt32(reader.GetValue(0)), 0);

            }

            reader.Close();

            //UpdateLog(Convert.ToString(timing_list[0]));
            //UpdateLog(Convert.ToString(timing_list[1]));
            //UpdateLog(Convert.ToString(timing_list.Count));
        }

        public string data_get_proxy(int id_prof)
        {
            m_sqlCmd = m_dbConn.CreateCommand();
            m_sqlCmd.CommandText = "SELECT proxy FROM Setting WHERE id = " + id_prof;
            sqlite_datareader = m_sqlCmd.ExecuteReader();
            sqlite_datareader.Read();

            string proxy = sqlite_datareader.GetString(0);
            sqlite_datareader.Close();

            return proxy;
        }

        public string data_get_prof(int id_prof)
        {
            m_sqlCmd = m_dbConn.CreateCommand();
            m_sqlCmd.CommandText = "SELECT prof FROM Setting WHERE id = " + id_prof;
            sqlite_datareader = m_sqlCmd.ExecuteReader();
            sqlite_datareader.Read();

            string prof = sqlite_datareader.GetString(0);
            sqlite_datareader.Close();

            return prof;
        }

        public bool simple_captcha(IWebDriver driver)
        {
            if (IsElementVisible(FandS(driver, "botdetect_free_play_captcha")) && IsElementVisible(FandS(driver, "switch_captchas_button")))
            {
                logo = FandS(driver, "//*[@id='botdetect_free_play_captcha']/div[1]/img");
                logoSRC = logo.GetAttribute("src");
                imageURL = new Uri(logoSRC);
                PuthToPicture = Rucaptcha.Download_Captcha(imageURL.ToString());
                logoSRC = Rucaptcha.Recognize(PuthToPicture);

                if (logoSRC == "ERROR|TIMEOUT")
                {
                    return false;
                }
                else
                {
                    FandS(driver, "//*[@id='botdetect_free_play_captcha']/input[2]").SendKeys(logoSRC);

                    logo = FandS(driver, "//*[@id='botdetect_free_play_captcha2']/div[1]/img");
                    logoSRC = logo.GetAttribute("src");
                    imageURL = new Uri(logoSRC);
                    PuthToPicture = Rucaptcha.Download_Captcha(imageURL.ToString());
                    logoSRC = Rucaptcha.Recognize(PuthToPicture);
                    FandS(driver, "//*[@id='botdetect_free_play_captcha2']/input[2]").SendKeys(logoSRC);
                    
                    miss += 4;
                    return true;
                }
                


            }
            if (!IsElementVisible(FandS(driver, "botdetect_free_play_captcha")) && IsElementVisible(FandS(driver, "switch_captchas_button")))
            {
                FandS(driver, "switch_captchas_button").Click();

                logo = FandS(driver, "//*[@id='botdetect_free_play_captcha']/div[1]/img");
                logoSRC = logo.GetAttribute("src");
                imageURL = new Uri(logoSRC);
                PuthToPicture = Rucaptcha.Download_Captcha(imageURL.ToString());
                logoSRC = Rucaptcha.Recognize(PuthToPicture);

                if (logoSRC == "ERROR | TIMEOUT")
                {
                    return false;
                }
                else
                {
                    FandS(driver, "//*[@id='botdetect_free_play_captcha']/input[2]").SendKeys(logoSRC);

                    logo = FandS(driver, "//*[@id='botdetect_free_play_captcha2']/div[1]/img");
                    logoSRC = logo.GetAttribute("src");
                    imageURL = new Uri(logoSRC);
                    PuthToPicture = Rucaptcha.Download_Captcha(imageURL.ToString());
                    logoSRC = Rucaptcha.Recognize(PuthToPicture);
                    FandS(driver, "//*[@id='botdetect_free_play_captcha2']/input[2]").SendKeys(logoSRC);
                    
                    miss += 4;
                    return true;
                }
            }
            
            return false;



        }

        public void Rucaptchav2(IWebDriver driver)
        {
            string id;
            int reqcount = 0;

            ((IJavaScriptExecutor)driver).ExecuteScript("window.open()");
            driver.SwitchTo().Window(driver.WindowHandles.Last());
            driver.Navigate().GoToUrl("http://rucaptcha.com/in.php?key=50e9fba39de714daa84c59e34ad638b2&method=userrecaptcha&googlekey=6LeGfGIUAAAAAEyUovGUehv82L-IdNRusaYFEm5b&pageurl=https://freebitco.in/?op=home&here=now");
            id = Convert.ToString((driver.FindElement(By.XPath("/html/body")).Text).Replace("OK|", ""));
            UpdateLog("id капчи: " + id);

            Thread.Sleep(1000);
            driver.Navigate().GoToUrl("http://rucaptcha.com/res.php?key=50e9fba39de714daa84c59e34ad638b2&action=get&id=" + id);

            while (Convert.ToString(driver.FindElement(By.XPath("/html/body")).Text) == "CAPCHA_NOT_READY" && reqcount < 60)
            {
                Thread.Sleep(3000);
                driver.Navigate().GoToUrl("http://rucaptcha.com/res.php?key=50e9fba39de714daa84c59e34ad638b2&action=get&id=" + id);

            }

            if (reqcount >= 60)
            {
                return;
            }

            id = Convert.ToString((driver.FindElement(By.XPath("/html/body")).Text).Replace("OK|", ""));
            Thread.Sleep(1000);

            driver.Close();
            driver.SwitchTo().Window(driver.WindowHandles.Last());
            Thread.Sleep(1000);

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].setAttribute('style','visibility:visible;');", driver.FindElement(By.Id("g-recaptcha-response")));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", driver.FindElement(By.Id("g-recaptcha-response")));


            driver.FindElement(By.Id("g-recaptcha-response")).SendKeys(id);
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].setAttribute('style','display:none;');", driver.FindElement(By.Id("g-recaptcha-response")));
            miss += 16;

        }

        public void DataUpdate(int RP, int BTC, int miss)
        {
            try
            {
                m_sqlCmd.CommandText = "INSERT INTO Catalog ('Время', 'RP', 'BTC', 'miss') values ('" + DateTime.Now + "' , '" + RP + "' , '" + BTC + "', '" + miss + "')";

                m_sqlCmd.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                UpdateLog("Error: " + ex.Message);
            }
        }

        public void DataGridUpdate1()
        {
            DataTable dTable = new DataTable();
            String sqlQuery;


            try
            {
                sqlQuery = "SELECT * FROM Setting";
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqlQuery, m_dbConn);
                adapter.Fill(dTable);

                if (dTable.Rows.Count > 0)
                {
                    dataGridView2.Rows.Clear();

                    for (int i = dTable.Rows.Count - 1; i >= 0; i--)
                        dataGridView2.Rows.Add(dTable.Rows[i].ItemArray);
                }
                else
                    UpdateLog("Database is empty");
            }
            catch (SQLiteException ex)
            {
                UpdateLog("Error: " + ex.Message);
            }
        }


        public void DataGridUpdate()
        {
            DataTable dTable = new DataTable();
            String sqlQuery;


            try
            {
                sqlQuery = "SELECT * FROM Log";
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqlQuery, m_dbConn);
                adapter.Fill(dTable);

                if (dTable.Rows.Count > 0)
                {
                    dataGridView1.Rows.Clear();

                    for (int i = dTable.Rows.Count - 1; i >= 0; i--)
                        dataGridView1.Rows.Add(dTable.Rows[i].ItemArray);
                }
                else
                    UpdateLog("Database is empty");
            }
            catch (SQLiteException ex)
            {
                UpdateLog("Error: " + ex.Message);
            }
        }

        public void UpdateLog(string s)
        {
            

            Action action = () =>
            {
                richTextBox1.AppendText(s + "\n");
            };

            Invoke(action);
        }

        public IWebElement FandS(IWebDriver driver, string selector)
        {
            IWebElement element = null;
            try
            {
                if (IsElementVisible(driver.FindElement(By.CssSelector(selector))))
                {

                    element = driver.FindElement(By.CssSelector(selector));
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].setAttribute('style','display:none;');", driver.FindElement(By.CssSelector(".large-12.fixed")));
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);

                    return element;
                }

            }
            catch (NoSuchElementException)
            {
                
            }

            try
            {
                if (IsElementVisible(driver.FindElement(By.Id(selector))))
                {
                    element = driver.FindElement(By.Id(selector));
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].setAttribute('style','display:none;');", driver.FindElement(By.CssSelector(".large-12.fixed")));
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);

                    return element;
                }
            }
            catch (NoSuchElementException)
            {

            }

            try
            {
                if (IsElementVisible(driver.FindElement(By.XPath(selector))))
                {
                    element = driver.FindElement(By.XPath(selector));
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].setAttribute('style','display:none;');", driver.FindElement(By.CssSelector(".large-12.fixed")));
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);

                    return element;
                }
            }
            catch (NoSuchElementException)
            {

            }

            try
            {
                if (IsElementVisible(driver.FindElement(By.PartialLinkText(selector))))
                {
                    element = driver.FindElement(By.PartialLinkText(selector));
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].setAttribute('style','display:none;');", driver.FindElement(By.CssSelector(".large-12.fixed")));
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);

                    return element;
                }
            }
            catch (NoSuchElementException)
            {

            }

            return null;

        }

        public bool IsElementVisible(IWebElement element)
        {
            try
            {
                return element.Displayed && element.Enabled;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void calculete(string BTC, string RP, int miss)
        {

            Action action = () =>
            {
                try
                {
                    spred_BTC = Convert.ToInt32(BTC.Replace(".", ""));
                    spred_RP = Convert.ToInt32(RP);
                }
                catch (System.FormatException)
                {
                    spred_BTC = 0;
                }

                if (spred_BTC != 0)
                {
                    DataUpdate(spred_RP, spred_BTC, miss);
                    DataGridUpdate();
                }
            };


            Invoke(action);
        }

        public void stat()
        {
            //dataGridView2[0, 0].Value = spred_BTC;
            //dataGridView2[1, 0].Value = old_RP;
            //dataGridView2[2, 0].Value = old_RP;
            //dataGridView2[3, 0].Value = old_RP;
        }

        public void bonus(IWebDriver driver)
        {
            

            if (checkBox1.Checked)
            {
                driver.FindElement(By.PartialLinkText("REWARDS")).Click();
                old_RP = Convert.ToInt32(FandS(driver, ".user_reward_points").Text.Replace(",", ""));
                FandS(driver, "//*[@id='rewards_tab']/div[4]/div/div[6]/div[1]").Click();

                if (old_RP >= 12 && old_RP < 120) { FandS(driver, "//*[@id='free_points_rewards']/div[5]/div[2]/div[3]/button").Click(); }
                if (old_RP >= 120 && old_RP < 300) { FandS(driver, "//*[@id='free_points_rewards']/div[4]/div[2]/div[3]/button").Click(); }
                if (old_RP >= 300 && old_RP < 600) { FandS(driver, "//*[@id='free_points_rewards']/div[3]/div[2]/div[3]/button").Click(); }
                if (old_RP >= 600 && old_RP < 1200) { FandS(driver, "//*[@id='free_points_rewards']/div[2]/div[2]/div[3]/button").Click(); }
                if (old_RP >= 1200) { FandS(driver, "//*[@id='free_points_rewards']/div[1]/div[2]/div[3]/button").Click(); }
                driver.Navigate().GoToUrl("https://freebitco.in/");

            }

            if (checkBox2.Checked)
            {
                driver.FindElement(By.PartialLinkText("REWARDS")).Click();
                old_RP = Convert.ToInt32(FandS(driver, ".user_reward_points").Text.Replace(",", ""));
                FandS(driver, "//*[@id='rewards_tab']/div[4]/div/div[4]/div[1]").Click();

                if (old_RP >= 1520 && old_RP < 2800) { FandS(driver, "//*[@id='fp_bonus_rewards']/div[3]/div[2]/div[3]/button").Click(); }
                if (old_RP >= 2800 && old_RP < 4400) { FandS(driver, "//*[@id='fp_bonus_rewards']/div[2]/div[2]/div[3]/button").Click(); }
                if (old_RP >= 4400) { FandS(driver, "//*[@id='fp_bonus_rewards']/div[1]/div[2]/div[3]/button").Click(); }
                driver.Navigate().GoToUrl("https://freebitco.in/");


            }
        }

        public async void Step(int i)
        {
            richTextBox1.Clear();
            miss = 0;
            
            DataGridUpdate();
            await Task.Run(() =>
            {
                //options.AddArgument("--headless");
                options = new ChromeOptions();
                Proxy proxy = new Proxy();
                proxy.Kind = ProxyKind.Manual;
                proxy.IsAutoDetect = false;
                proxy.HttpProxy = data_get_proxy(i);
                proxy.SslProxy = data_get_proxy(i);
                options.Proxy = proxy;
                options.AddArgument("ignore-certificate-errors");

                options.AddArguments(@"user-data-dir=" + Application.StartupPath + @"\" + data_get_prof(i));
                options.AddArguments("--start-maximized");
                IWebDriver driver = new ChromeDriver(options);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);

                driver.Navigate().GoToUrl("https://freebitco.in/");





                //Определение кулдауна сбора
                if (IsElementVisible(FandS(driver, "multi_acct_same_ip")))
                {
                    bad_ip = true;


                    timing_list[i] = 10000;
                    driver.Quit();
                    busy = false;
                    return;
                }

                if (IsElementVisible(FandS(driver, ".countdown_amount")))
                {
                    timing_list[i] = Convert.ToInt32(FandS(driver, ".countdown_amount").Text) * 60 + 10;
                    
                    UpdateLog("Кулдаун сбора " + timing_list[i] + " секунд");

                    driver.Quit();
                    busy = false;
                    return ;
                }
                driver.Navigate().Refresh();


//Активация бонусов
                bonus(driver);





                do
                {
                    driver.Navigate().Refresh();
                    if (IsElementVisible(FandS(driver, "switch_captchas_button")))
                        simple_captcha(driver);

                    
                    if (IsElementVisible(FandS(driver, ".g-recaptcha")) && !IsElementVisible(FandS(driver, "switch_captchas_button")))
                        Rucaptchav2(driver);

                    FandS(driver, "free_play_form_button").Click();
                    Thread.Sleep(3000);
                }
                while (IsElementVisible(FandS(driver, "free_play_form_button")));


                try
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", driver.FindElement(By.Id("winnings")));

                    calculete(driver.FindElement(By.Id("winnings")).Text, driver.FindElement(By.Id("fp_reward_points_won")).Text, miss);
                    timing_list[i] = Convert.ToInt32(FandS(driver, ".countdown_amount").Text) * 60 + 10;
                }

                catch (Exception)
                {

                }

                
                driver.Quit();
                busy = false;

            });
        }

        public Form1()
        {
            InitializeComponent();
            
            
        }

        private void listBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            //richTextBox1.AppendText(listBox1.SelectedItem.ToString() + "\n");
            //richTextBox1.AppendText(listBox1.SelectedIndex.ToString() + "\n");
        }

        private void Go_Click(object sender, EventArgs e)
        {
            if (Go.Text == "Паихали!")
            {
                timer1.Start();
                Go.Text = "Стапэ!";
            }
            else
            {
                timer1.Stop();
                Go.Text = "Паихали!";
                Time = 0;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {



            options = new ChromeOptions();
            Proxy proxy = new Proxy();
            proxy.Kind = ProxyKind.Manual;
            proxy.IsAutoDetect = false;
            proxy.HttpProxy = data_get_proxy(Convert.ToInt32(textBox5.Text));
            proxy.SslProxy = data_get_proxy(Convert.ToInt32(textBox5.Text));
            options.Proxy = proxy;
            options.AddArgument("ignore-certificate-errors");

            options.AddArguments(@"user-data-dir=" + Application.StartupPath + @"\" + data_get_prof(Convert.ToInt32(textBox5.Text)));
            options.AddArguments("--start-maximized");
            IWebDriver driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);

            driver.Navigate().GoToUrl("https://freebitco.in/");

            


        }

        private void button3_Click(object sender, EventArgs e)
        {
            StreamWriter sw = new StreamWriter(Application.StartupPath + @"\Setting.txt");
            sw.WriteLine(textBox1.Text);
            //Write a second line of text
            //sw.WriteLine("From the StreamWriter class");
            sw.Close();

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
                richTextBox1.Clear();

                for (int i = 0; i < timing_list.Count; i++)
                {
                    timing_list[i]--;

                    if (timing_list[i] <= 0 && busy == false)
                    {
                    busy = true;
                    Step(i);
                    }

                }
                foreach (var item in timing_list)
                {
                    UpdateLog(Convert.ToString(item) + "\t");
                }
                Time -= 1;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            listBox1.SetSelected(0, true);

            

            m_dbConn = new SQLiteConnection();
            m_sqlCmd = new SQLiteCommand();

            if (!File.Exists(dbFileName))
                SQLiteConnection.CreateFile(dbFileName);

            try
            {
                m_dbConn = new SQLiteConnection("Data Source=" + dbFileName + ";Version=3;");
                m_dbConn.Open();
                m_sqlCmd.Connection = m_dbConn;

                m_sqlCmd.CommandText = "CREATE TABLE IF NOT EXISTS Log (id INTEGER PRIMARY KEY AUTOINCREMENT, Время TEXT, RP INTEGER, BTC INTEGER, miss INTEGER)";
                m_sqlCmd.ExecuteNonQuery();

                m_sqlCmd.CommandText = "CREATE TABLE IF NOT EXISTS Setting (id INTEGER PRIMARY KEY AUTOINCREMENT, akk TEXT, prof TEXT, proxy TEXT)";
                m_sqlCmd.ExecuteNonQuery();



            }
            catch (SQLiteException ex)
            {
                
                MessageBox.Show("Error: " + ex.Message);
            }

            
            if (File.Exists("Setting.txt"))
            {
                StreamReader sr = new StreamReader(Application.StartupPath + @"\Setting.txt");
                //Read the first line of text
                textBox1.Text = sr.ReadLine();
                sr.Close();
            }

            Rucaptcha.Key = textBox1.Text;
            get_timing_list();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            options = new ChromeOptions();
            Proxy proxy = new Proxy();
            proxy.Kind = ProxyKind.Manual;
            proxy.IsAutoDetect = false;
            proxy.HttpProxy = " ";
            proxy.SslProxy = " ";
            options.Proxy = proxy;
            options.AddArgument("ignore-certificate-errors");

            options.AddArguments(@"user-data-dir=" + Application.StartupPath + @"\" + data_get_prof(Convert.ToInt32(textBox5.Text)));
            options.AddArguments("--start-maximized");
            IWebDriver driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);

            driver.Navigate().GoToUrl("https://freebitco.in/");
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            
            DataGridUpdate();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                m_sqlCmd.CommandText = "INSERT INTO Setting (id, 'akk', 'prof', 'proxy') values ('" + Convert.ToInt32(textBox6.Text) + "' , '" + textBox2.Text + "' , '" + textBox3.Text + "' , '" + textBox4.Text + "')";


                m_sqlCmd.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                UpdateLog("Error: " + ex.Message);
            }

            DataGridUpdate1();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            textBox6.Clear();

            get_timing_list();
        }

        private void button6_Click(object sender, EventArgs e)//тестовая кнопка
        {
            options = new ChromeOptions();
            Proxy proxy = new Proxy();
            proxy.Kind = ProxyKind.Manual;
            proxy.IsAutoDetect = false;
            proxy.HttpProxy = " ";
            proxy.SslProxy = " ";
            options.Proxy = proxy;
            options.AddArgument("ignore-certificate-errors");

            options.AddArguments(@"user-data-dir=" + Application.StartupPath + @"\" + data_get_prof(Convert.ToInt32(textBox5.Text)));
            options.AddArguments("--start-maximized");
            IWebDriver driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);

            driver.Navigate().GoToUrl("https://freebitco.in/");
            Thread.Sleep(3000);

            if (IsElementVisible(FandS(driver, ".countdown_amount")))
            {
                int i = Convert.ToInt32(FandS(driver, ".countdown_amount").Text) * 60 + 10;

                UpdateLog("Кулдаун сбора " + i + " секунд");
            }
            //driver.Quit();

        }

        private void button7_Click(object sender, EventArgs e)//обновление
        {
            DataGridUpdate1();
        }

        private void button8_Click(object sender, EventArgs e)//уделение строки из сеттингов по ид
        {

            m_sqlCmd.CommandText = "DELETE FROM Setting WHERE id=" + Convert.ToInt32(textBox6.Text) + "";
            m_sqlCmd.ExecuteNonQuery();
            DataGridUpdate1();
            get_timing_list();
        }
    }
}
