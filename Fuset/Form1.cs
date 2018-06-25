using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;

namespace Fuset
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void listBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            richTextBox1.AppendText(listBox1.SelectedItem.ToString() + "\n");
            richTextBox1.AppendText(listBox1.SelectedIndex.ToString() + "\n");
        }

        private void button1_Click(object sender, EventArgs e)
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
            switch (listBox1.SelectedIndex)
            {
                case 0:
                    driver.Navigate().GoToUrl("https://freebitco.in/");
                    try
                    {
                        driver.FindElement(By.CssSelector(".login_menu_button")).Click();
                        richTextBox1.AppendText("Вход...");
                        driver.FindElement(By.Id("login_form_btc_address")).SendKeys("blooddecember@gmail.com");
                        driver.FindElement(By.Id("login_form_password")).SendKeys("Problem.net87");
                        driver.FindElement(By.Id("login_button")).Click();
                    }
                    catch (OpenQA.Selenium.NoSuchElementException)
                    {
                        richTextBox1.AppendText("Вход не требуется.");
                        //logIn = true;
                    }
                    //richTextBox1.AppendText(Convert.ToString((driver.FindElement(By.Id("btc_usd_price")).Text + "\n")));
                    break;
            }
        }
    }
}
