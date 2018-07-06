using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
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
        string PuthToPicture; //Переменная для хранения пути к картинке капчи на компьютере
        int old_RP = 0; 
        int old_BTC = 0;
        int spred_BTC = 0;
        int spred_RP = 0;
        int Time = 0;
        IWebElement logo;
        String logoSRC;
        Uri imageURL;
        IWebDriver driver;
        ChromeOptions options;

        private String dbFileName = "sample.sqlite";
        private SQLiteConnection m_dbConn;
        private SQLiteCommand m_sqlCmd;

        public void DataUpdate(int RP, int BTC)
        {
            try
            {
                m_sqlCmd.CommandText = "INSERT INTO Catalog ('Время', 'RP', 'BTC') values ('" + DateTime.Now + "' , '" + RP + "' , '" + BTC + "')";

                m_sqlCmd.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        public void DataGridUpdate()
        {
            DataTable dTable = new DataTable();
            String sqlQuery;


            try
            {
                sqlQuery = "SELECT * FROM Catalog";
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqlQuery, m_dbConn);
                adapter.Fill(dTable);

                if (dTable.Rows.Count > 0)
                {
                    dataGridView1.Rows.Clear();

                    for (int i = dTable.Rows.Count - 1; i >= 0; i--)
                        dataGridView1.Rows.Add(dTable.Rows[i].ItemArray);
                }
                else
                    MessageBox.Show("Database is empty");
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Error: " + ex.Message);
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

        public void calculete(string BTC, string RP)
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
                    DataUpdate(spred_RP, spred_BTC);
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
                driver.FindElement(By.CssSelector(".rewards_link")).Click();

                Thread.Sleep(1000);
                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                js.ExecuteScript("window.scrollBy(0,950);");
                driver.FindElement(By.XPath("//*[@id='rewards_tab']/div[4]/div/div[6]/div[1]")).Click();

                Thread.Sleep(1000);
                js.ExecuteScript("window.scrollBy(0,950);");
                driver.FindElement(By.XPath("//*[@id='free_points_rewards']/div[1]/div[2]/div[3]/button")).Click();
                Thread.Sleep(1000);
                driver.FindElement(By.XPath("//*[@id='free_points_rewards']/div[2]/div[2]/div[3]/button")).Click();
                Thread.Sleep(1000);
                driver.FindElement(By.XPath("//*[@id='free_points_rewards']/div[3]/div[2]/div[3]/button")).Click();
                Thread.Sleep(1000);
                driver.FindElement(By.XPath("//*[@id='free_points_rewards']/div[4]/div[2]/div[3]/button")).Click();
                Thread.Sleep(1000);
                driver.FindElement(By.XPath("//*[@id='free_points_rewards']/div[5]/div[2]/div[3]/button")).Click();

                driver.Navigate().Refresh();
            }

            if (old_RP > 2800 && checkBox2.Checked)
            {
                driver.FindElement(By.CssSelector(".rewards_link")).Click();

                Thread.Sleep(1000);
                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                js.ExecuteScript("window.scrollBy(0,950);");
                driver.FindElement(By.XPath("//*[@id='rewards_tab']/div[4]/div/div[4]/div[1]")).Click();

                Thread.Sleep(1000);
                js.ExecuteScript("window.scrollBy(0,700);");
                driver.FindElement(By.XPath("//*[@id='fp_bonus_rewards']/div[1]/div[2]/div[3]/button")).Click();
                Thread.Sleep(1000);
                driver.FindElement(By.XPath("//*[@id='fp_bonus_rewards']/div[2]/div[2]/div[3]/button")).Click();
                Thread.Sleep(1000);

                driver.Navigate().Refresh();
             }
        }

        public async void Step()
        {
            richTextBox1.Clear();
            timer1.Stop();
            DataGridUpdate();
            await Task.Run(() =>
            {
                options = new ChromeOptions();
                //options.AddArgument("--headless");
                //options.AddArgument("--disable-gpu");
                //options.AddArgument("--no-sandbox");
                //options.AddArgument("--ignore-certificate-errors");
                options.AddArguments(@"user-data-dir=" + Application.StartupPath + @"\TestProf");
                options.AddArguments("--start-maximized");
                IWebDriver driver = new ChromeDriver(options);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(20);

                driver.Navigate().GoToUrl("https://freebitco.in/");


//Записываем баланс до попытки сбора
                old_BTC = Convert.ToInt32(driver.FindElement(By.Id("balance")).Text.Replace(".", ""));

                driver.FindElement(By.CssSelector(".rewards_link")).Click();
                old_RP = Convert.ToInt32(driver.FindElement(By.XPath("//*[@id='rewards_tab']/div[2]/div/div[2]")).Text.Replace(",", ""));

                UpdateLog("old_RP = " + Convert.ToString(old_RP));

                driver.Navigate().Refresh();


//Определение кулдауна сбора
                if (IsElementVisible(driver.FindElement(By.CssSelector(".countdown_amount"))))             //
                {
                    Time = Convert.ToInt32(driver.FindElement(By.CssSelector(".countdown_amount")).Text) * 60 + 60;
                    
                    UpdateLog("Кулдаун сбора " + Time + " секунд");
                }


//Активация бонусов
                bonus(driver);



//Переключение на текстовые капчи, поиск, разгадывание первой текстовой капчи
                try
                {
                    driver.FindElement(By.Id("switch_captchas_button")).Click();
                    logo = driver.FindElement(By.XPath("//*[@id='captchasnet_free_play_captcha']/div[1]/img"));
                    logoSRC = logo.GetAttribute("src");
                    imageURL = new Uri(logoSRC);
                    PuthToPicture = Rucaptcha.Download_Captcha(imageURL.ToString());
                    if (IsElementVisible(driver.FindElement(By.XPath("//*[@id='captchasnet_free_play_captcha']/input[2]"))))
                    {
                        driver.FindElement(By.XPath("//*[@id='captchasnet_free_play_captcha']/input[2]")).SendKeys(Rucaptcha.Recognize(PuthToPicture));
                    }
                    else
                    {
                        driver.FindElement(By.Id("switch_captchas_button")).Click();
                        driver.FindElement(By.XPath("//*[@id='captchasnet_free_play_captcha']/input[2]")).SendKeys(Rucaptcha.Recognize(PuthToPicture));
                    }
                }
                catch (Exception)
                {
                    UpdateLog("Первая капча не найдена");
                }


//Вторая капча
                try
                {
                    logo = driver.FindElement(By.XPath("//*[@id='botdetect_free_play_captcha']/div[1]/img"));
                    logoSRC = logo.GetAttribute("src");
                    imageURL = new Uri(logoSRC);
                    PuthToPicture = Rucaptcha.Download_Captcha(imageURL.ToString());
                    if (IsElementVisible(driver.FindElement(By.XPath("//*[@id='captchasnet_free_play_captcha']/input[2]"))))
                    {
                        driver.FindElement(By.XPath("//*[@id='botdetect_free_play_captcha']/input[2]")).SendKeys(Rucaptcha.Recognize(PuthToPicture));
                    }
                    else
                    {
                        driver.FindElement(By.Id("switch_captchas_button")).Click();
                        driver.FindElement(By.XPath("//*[@id='botdetect_free_play_captcha']/input[2]")).SendKeys(Rucaptcha.Recognize(PuthToPicture));
                    }
                }
                catch (Exception)
                {
                    UpdateLog("Вторая капча не найдена");
                    try
                    {
                        IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                        js.ExecuteScript("window.scrollBy(0,950);");

                        driver.FindElement(By.Id("free_play_form_button")).Click();

                        Time = 3660;

                    }
                    catch (Exception)
                    {
                        UpdateLog("Кнопка сбора не найдена");

                    }
                }


//Ищем ошибку
                if (IsElementVisible(driver.FindElement(By.Id("free_play_error"))))
                {
                    string error = driver.FindElement(By.Id("free_play_error")).Text;
                    UpdateLog(error);
                }

//Запись статистики
                Thread.Sleep(7000);

                //driver.FindElement(By.CssSelector(".close-reveal-modal")).Click();
                driver.FindElement(By.CssSelector(".rewards_link")).Click();
                driver.FindElement(By.CssSelector(".free_play_link")).Click();

                Thread.Sleep(1000);
                IJavaScriptExecutor s = driver as IJavaScriptExecutor;
                s.ExecuteScript("window.scrollBy(0,950);");

                try
                {
                    calculete(driver.FindElement(By.Id("winnings")).Text, driver.FindElement(By.Id("fp_reward_points_won")).Text);
                }
                catch (Exception)
                {

                }
                //driver.Navigate().Refresh();
                //driver.FindElement(By.CssSelector(".rewards_link")).Click();
                //calculete(driver.FindElement(By.Id("balance")).Text , driver.FindElement(By.XPath("//*[@id='rewards_tab']/div[2]/div/div[2]")).Text);
                //UpdateLog("new_RP = " + Convert.ToString(driver.FindElement(By.XPath("//*[@id='rewards_tab']/div[2]/div/div[2]")).Text));
                driver.Quit();


            });
            
            label2.Text = Convert.ToString(Time);

            timer1.Start();
            Go.Text = "Стапэ!";
        }

        public Form1()
        {
            InitializeComponent();
            Rucaptcha.Key = "50e9fba39de714daa84c59e34ad638b2";
            
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
            //options.AddArgument("--headless");
            //options.AddArgument("--disable-gpu");
            //options.AddArgument("--no-sandbox");
            //options.AddArgument("--ignore-certificate-errors");
            options.AddArguments(@"user-data-dir=" + Application.StartupPath + @"\TestProf");
            //options.AddArguments("--start-maximized");
            IWebDriver driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
            //driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(20);

            driver.Navigate().GoToUrl("https://freebitco.in/");

            if (Time <= 0 && checkBox1.Checked)
            {
                driver.FindElement(By.CssSelector(".rewards_link")).Click();

                Thread.Sleep(1000);
                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                js.ExecuteScript("window.scrollBy(0,950);");
                driver.FindElement(By.XPath("//*[@id='rewards_tab']/div[4]/div/div[6]/div[1]")).Click();

                Thread.Sleep(1000);
                js.ExecuteScript("window.scrollBy(0,950);");
                driver.FindElement(By.XPath("//*[@id='free_points_rewards']/div[1]/div[2]/div[3]/button")).Click();
                Thread.Sleep(1000);
                driver.FindElement(By.XPath("//*[@id='free_points_rewards']/div[2]/div[2]/div[3]/button")).Click();
                Thread.Sleep(1000);
                driver.FindElement(By.XPath("//*[@id='free_points_rewards']/div[3]/div[2]/div[3]/button")).Click();
                Thread.Sleep(1000);
                driver.FindElement(By.XPath("//*[@id='free_points_rewards']/div[4]/div[2]/div[3]/button")).Click();
                Thread.Sleep(1000);
                driver.FindElement(By.XPath("//*[@id='free_points_rewards']/div[5]/div[2]/div[3]/button")).Click();

                driver.Navigate().Refresh();
            }

            if (Time <= 0 && checkBox2.Checked)
            {
                driver.FindElement(By.CssSelector(".rewards_link")).Click();

                Thread.Sleep(1000);
                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                js.ExecuteScript("window.scrollBy(0,950);");
                driver.FindElement(By.XPath("//*[@id='rewards_tab']/div[4]/div/div[4]/div[1]")).Click();

                Thread.Sleep(1000);
                js.ExecuteScript("window.scrollBy(0,700);");
                driver.FindElement(By.XPath("//*[@id='fp_bonus_rewards']/div[1]/div[2]/div[3]/button")).Click();
                Thread.Sleep(1000);
                driver.FindElement(By.XPath("//*[@id='fp_bonus_rewards']/div[2]/div[2]/div[3]/button")).Click();
                Thread.Sleep(1000);
                driver.FindElement(By.XPath("//*[@id='fp_bonus_rewards']/div[3]/div[2]/div[3]/button")).Click();
                Thread.Sleep(1000);
                driver.FindElement(By.XPath("//*[@id='fp_bonus_rewards']/div[4]/div[2]/div[3]/button")).Click();
                Thread.Sleep(1000);
                driver.FindElement(By.XPath("//*[@id='fp_bonus_rewards']/div[5]/div[2]/div[3]/button")).Click();

                driver.Navigate().Refresh();
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            UpdateLog("текс");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Time >= 0)
            {
                Time -= 1;
                label2.Text = Convert.ToString(Time);
            }
            else
            {
                Step();
            }
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

                m_sqlCmd.CommandText = "CREATE TABLE IF NOT EXISTS Catalog (id INTEGER PRIMARY KEY AUTOINCREMENT, Время TEXT, RP INTEGER, BTC INTEGER)";
                m_sqlCmd.ExecuteNonQuery();

                
            }
            catch (SQLiteException ex)
            {
                
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            options = new ChromeOptions();
            //options.AddArgument("--headless");
            //options.AddArgument("--disable-gpu");
            //options.AddArgument("--no-sandbox");
            //options.AddArgument("--ignore-certificate-errors");
            options.AddArguments(@"user-data-dir=" + Application.StartupPath + @"\TestProf");
            options.AddArguments("--start-maximized");
            driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(10);
            driver.Navigate().GoToUrl("https://freebitco.in/");

            //driver.FindElement(By.CssSelector(".login_menu_button")).Click();
        }
    }
}
