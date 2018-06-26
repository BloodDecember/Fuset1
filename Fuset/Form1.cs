﻿using System;
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

        public bool IsElementVisible(IWebElement element)
        {
            return element.Displayed && element.Enabled;
        }

        public void Step()
        {
            ChromeOptions options = new ChromeOptions();
            //options.AddArgument("--headless");
            //options.AddArgument("--disable-gpu");
            //options.AddArgument("--no-sandbox");
            //options.AddArgument("--ignore-certificate-errors");
            options.AddArguments(@"user-data-dir=" + Application.StartupPath + @"\TestProf");
            options.AddArguments("--start-maximized");
            IWebDriver driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(10);
            switch (listBox1.SelectedIndex)
            {
                case 0:
                    driver.Navigate().GoToUrl("https://freebitco.in/");


                    //Ищем на форме таймер кулдауна.
                    try
                    {
                        Time = Convert.ToInt32(driver.FindElement(By.CssSelector(".countdown_amount")).Text) * 60;
                        label2.Text = Convert.ToString(Time);
                        driver.Quit();
                        break;
                    }

                    //Если не находим пытаемся залогинится
                    catch (Exception)
                    {
                        try
                        {
                            driver.FindElement(By.CssSelector(".login_menu_button")).Click();
                            richTextBox1.AppendText("Вход...");
                            driver.FindElement(By.Id("login_form_btc_address")).SendKeys("blooddecember@gmail.com");
                            driver.FindElement(By.Id("login_form_password")).SendKeys("Problem.net87");
                            driver.FindElement(By.Id("login_button")).Click();
                            richTextBox1.AppendText("Залогинились!");
                        }
                        catch (OpenQA.Selenium.NoSuchElementException)
                        {
                            richTextBox1.AppendText("Вход не требуется.");
                        }


                        //Переключаемся на текстовые капчи, ищем и сохраняем первую капчу
                        try
                        {
                            driver.FindElement(By.Id("switch_captchas_button")).Click();
                            logo = driver.FindElement(By.XPath("//*[@id='captchasnet_free_play_captcha']/div[1]/img"));
                            logoSRC = logo.GetAttribute("src");
                            imageURL = new Uri(logoSRC);
                            PuthToPicture = Rucaptcha.Download_Captcha(imageURL.ToString());
                        }
                        catch (NoSuchElementException)
                        {
                            driver.FindElement(By.Id("free_play_form_button")).Click();
                            driver.Quit();
                            break;
                        }
                        //Пытаемся найти поле для ввода первой капчи, разгадываем и заполняем поле первой капчи
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='captchasnet_free_play_captcha']/input[2]")).SendKeys(Rucaptcha.Recognize(PuthToPicture));
                        }

                        //Если не находит поле для ввода капчи нажимаем на переключатель капч, ищем и заполняем еще раз
                        catch (OpenQA.Selenium.ElementNotVisibleException)
                        {
                            driver.FindElement(By.Id("switch_captchas_button")).Click();
                            driver.FindElement(By.XPath("//*[@id='captchasnet_free_play_captcha']/input[2]")).SendKeys(Rucaptcha.Recognize(PuthToPicture));
                        }

                        //Ищем и согхроняем вторую капчу
                        logo = driver.FindElement(By.XPath("//*[@id='botdetect_free_play_captcha']/div[1]/img"));
                        logoSRC = logo.GetAttribute("src");
                        imageURL = new Uri(logoSRC);
                        PuthToPicture = Rucaptcha.Download_Captcha(imageURL.ToString());

                        //Разгадываем и заполняем вторую капчу
                        try
                        {
                            driver.FindElement(By.XPath("//*[@id='botdetect_free_play_captcha']/input[2]")).SendKeys(Rucaptcha.Recognize(PuthToPicture));
                        }
                        catch (OpenQA.Selenium.ElementNotVisibleException)
                        {
                            driver.FindElement(By.Id("switch_captchas_button")).Click();
                            driver.FindElement(By.XPath("//*[@id='botdetect_free_play_captcha']/input[2]")).SendKeys(Rucaptcha.Recognize(PuthToPicture));
                        }

                        //Нажимаем кнопку сбора
                        driver.FindElement(By.Id("free_play_form_button")).Click();
                        
                        //Ищем ошибку сбора
                        try
                        {
                            Console.WriteLine(Convert.ToString(driver.FindElement(By.Id("free_play_error")).Text));
                        }

                        //Если не находим
                        catch(System.FormatException)
                        {
                            //driver.FindElement(By.CssSelector(".close-reveal-modal")).Click();
                            //IWebElement Sat = driver.FindElement(By.Id("winnings"));
                            //richTextBox1.AppendText(Convert.ToString(Sat.Text) + "\n");
                            driver.Quit();
                            break;
                        }
                        driver.Quit();
                        break;
                    }
            }
        }

        public Form1()
        {
            InitializeComponent();
            Rucaptcha.Key = "50e9fba39de714daa84c59e34ad638b2";
            
        }

        private void listBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            richTextBox1.AppendText(listBox1.SelectedItem.ToString() + "\n");
            richTextBox1.AppendText(listBox1.SelectedIndex.ToString() + "\n");
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
            ChromeOptions options = new ChromeOptions();
            //options.AddArgument("--headless");
            //options.AddArgument("--disable-gpu");
            //options.AddArgument("--no-sandbox");
            //options.AddArgument("--ignore-certificate-errors");
            options.AddArguments(@"user-data-dir=" + Application.StartupPath + @"\TestProf");
            options.AddArguments("--start-maximized");
            IWebDriver driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(10);

            driver.Navigate().GoToUrl("https://freebitco.in/");

            try
            {
                if (driver.FindElement(By.CssSelector(".login_menu_button")) != null)
                {
                    driver.FindElement(By.CssSelector(".login_menu_button")).Click();
                    richTextBox1.AppendText("Вход...");
                    driver.FindElement(By.Id("login_form_btc_address")).SendKeys("blooddecember@gmail.com");
                    driver.FindElement(By.Id("login_form_password")).SendKeys("Problem.net87");
                    driver.FindElement(By.Id("login_button")).Click();
                    richTextBox1.AppendText("Залогинились!");
                }


                if (driver.FindElement(By.CssSelector(".countdown_amount")) != null)
                {
                    Time = Convert.ToInt32(driver.FindElement(By.CssSelector(".countdown_amount")).Text) * 60;
                    label2.Text = Convert.ToString(Time);
                    driver.Quit();
                }

                if (driver.FindElement(By.Id("free_play_form_button")) != null)
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
            catch (Exception)
            {

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
            options.AddArguments("--start-maximized");
            IWebDriver driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(10);

            driver.Navigate().GoToUrl("https://freebitco.in/");



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
            else
            {
                driver.Quit();
            }
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
