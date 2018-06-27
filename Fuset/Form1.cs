using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        double winnings = 0;
        int Time = 0;
        IWebElement logo;
        String logoSRC;
        Uri imageURL;
        IWebDriver driver;
        ChromeOptions options;
        

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

        public void Step()
        {
            ChromeOptions options = new ChromeOptions();
            //options.AddArgument("--headless");
            //options.AddArgument("--disable-gpu");
            //options.AddArgument("--no-sandbox");
            //options.AddArgument("--ignore-certificate-errors");
            options.AddArguments(@"user-data-dir=" + Application.StartupPath + @"\TestProf");
            //options.AddArguments("--start-maximized");
            IWebDriver driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(10);

            driver.Navigate().GoToUrl("https://freebitco.in/");


            //Блок авторизации
            try
            {
                driver.FindElement(By.CssSelector(".login_menu_button")).Click();
                richTextBox1.AppendText("Вход...");
                driver.FindElement(By.Id("login_form_btc_address")).SendKeys("blooddecember@gmail.com");
                driver.FindElement(By.Id("login_form_password")).SendKeys("Problem.net87");
                driver.FindElement(By.Id("login_button")).Click();
                driver.FindElement(By.CssSelector(".reward_point_redeem_result_error"));
                if (IsElementVisible(driver.FindElement(By.CssSelector(".reward_point_redeem_result_error"))))
                {
                    richTextBox1.AppendText("Много попыток входа, кулдаун 5 минут\n");
                    Time = 300;
                    driver.Quit();
                    label2.Text = Convert.ToString(Time);
                }
                else
                {
                    richTextBox1.AppendText("Залогинились!\n");
                }
            }
            catch (OpenQA.Selenium.NoSuchElementException)
            {
                richTextBox1.AppendText("Вход не требуется.\n");
            }


            //Определение кулдауна сбора
            if (IsElementVisible(driver.FindElement(By.CssSelector(".countdown_amount"))))             //
            {
                Time = Convert.ToInt32(driver.FindElement(By.CssSelector(".countdown_amount")).Text) * 60 + 60;
                label2.Text = Convert.ToString(Time);
                richTextBox1.AppendText("Кулдаун сбора " + Time + " секунд\n");
                //driver.Quit();
            }


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
                richTextBox1.AppendText("Первая капча не найдена\n");
                //driver.FindElement(By.Id("free_play_form_button")).Click();
                //driver.Quit();
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
                richTextBox1.AppendText("Вторая капча не найдена\n");
                try
                {
                    Actions actions = new Actions(driver);

                    actions.MoveToElement(driver.FindElement(By.Id("free_play_form_button"))).Click().Perform();

                    driver.FindElement(By.Id("free_play_form_button")).Click();

                    Time = 3660;

                }
                catch (Exception)
                {
                    richTextBox1.AppendText("Кнопка сбора не найдена\n");

                }
            }


            //Ищем ошибку
            if (IsElementVisible(driver.FindElement(By.Id("free_play_error"))))
            {
                string error = driver.FindElement(By.Id("free_play_error")).Text;
                richTextBox1.AppendText(error + "\n");
            }
            driver.Quit();

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

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
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

            //Логинимся
            try
            {
                driver.FindElement(By.CssSelector(".login_menu_button")).Click();
                richTextBox1.AppendText("Вход...");
                driver.FindElement(By.Id("login_form_btc_address")).SendKeys("blooddecember@gmail.com");
                driver.FindElement(By.Id("login_form_password")).SendKeys("Problem.net87");
                driver.FindElement(By.Id("login_button")).Click();
                richTextBox1.AppendText("Залогинились!\n");
            }
            catch (OpenQA.Selenium.NoSuchElementException)
            {
                richTextBox1.AppendText("Вход не требуется.\n");
            }


            if (IsElementVisible(driver.FindElement(By.CssSelector(".countdown_amount"))))             //
            {
                Time = Convert.ToInt32(driver.FindElement(By.CssSelector(".countdown_amount")).Text) * 60;
                label2.Text = Convert.ToString(Time);
                driver.Quit();
            }

            if (IsElementVisible(driver.FindElement(By.Id("free_play_form_button"))))
            {
                driver.FindElement(By.Id("switch_captchas_button")).Click();
                logo = driver.FindElement(By.XPath("//*[@id='captchasnet_free_play_captcha']/div[1]/img"));
                logoSRC = logo.GetAttribute("src");
                imageURL = new Uri(logoSRC);
                PuthToPicture = Rucaptcha.Download_Captcha(imageURL.ToString());

                if (driver.FindElement(By.XPath("//*[@id='captchasnet_free_play_captcha']/input[2]")) != null)
                {
                    driver.FindElement(By.XPath("//*[@id='captchasnet_free_play_captcha']/input[2]")).SendKeys(Rucaptcha.Recognize(PuthToPicture));

                }

            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            ChromeOptions options = new ChromeOptions();
            //options.AddArgument("--headless");
            //options.AddArgument("--disable-gpu");
            //options.AddArgument("--no-sandbox");
            //options.AddArgument("--ignore-certificate-errors");
            options.AddArguments(@"user-data-dir=" + Application.StartupPath + @"\TestProf");
            //options.AddArguments("--start-maximized");
            IWebDriver driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(10);

            driver.Navigate().GoToUrl("https://freebitco.in/");


            //Блок авторизации
            try
            {
                driver.FindElement(By.CssSelector(".login_menu_button")).Click();
                richTextBox1.AppendText("Вход...");
                driver.FindElement(By.Id("login_form_btc_address")).SendKeys("blooddecember@gmail.com");
                driver.FindElement(By.Id("login_form_password")).SendKeys("Problem.net87");
                driver.FindElement(By.Id("login_button")).Click();
                driver.FindElement(By.CssSelector(".reward_point_redeem_result_error"));
                if (IsElementVisible(driver.FindElement(By.CssSelector(".reward_point_redeem_result_error"))))
                {
                    richTextBox1.AppendText("Много попыток входа, кулдаун 5 минут\n");
                    Time = 300;
                    driver.Quit();
                    label2.Text = Convert.ToString(Time);
                }
                else
                {
                    richTextBox1.AppendText("Залогинились!\n");
                }
            }
            catch (OpenQA.Selenium.NoSuchElementException)
            {
                richTextBox1.AppendText("Вход не требуется.\n");
            }


            //Определение кулдауна сбора
            if (IsElementVisible(driver.FindElement(By.CssSelector(".countdown_amount"))))             //
            {
                Time = Convert.ToInt32(driver.FindElement(By.CssSelector(".countdown_amount")).Text) * 60;
                label2.Text = Convert.ToString(Time);
                richTextBox1.AppendText("Кулдаун сбора " + Time + " секунд\n");
                //driver.Quit();
            }


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
                richTextBox1.AppendText("Первая капча не найдена\n");
                //driver.FindElement(By.Id("free_play_form_button")).Click();
                //driver.Quit();
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
                richTextBox1.AppendText("Вторая капча не найдена\n");
                try
                {
                    driver.FindElement(By.Id("free_play_form_button")).Click();
                    Time = 3600;
                    
                }
                catch (Exception)
                {
                    richTextBox1.AppendText("Кнопка сбора не найдена\n");
                    
                }
            }


            //Ищем ошибку
            if (IsElementVisible(driver.FindElement(By.Id("free_play_error"))))
            {
                string error = driver.FindElement(By.Id("free_play_error")).Text;
                richTextBox1.AppendText(error + "\n");
            }
            driver.Quit();

            timer1.Start();
            Go.Text = "Стапэ!";
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
        }


    }
}
