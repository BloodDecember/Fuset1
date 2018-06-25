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

namespace Fuset
{
    public partial class Form1 : Form
    {
        string PuthToPicture; //Переменная для хранения пути к картинке капчи на компьютере
        double winnings = 0;

        public Form1()
        {
            InitializeComponent();
            Rucaptcha.Key = "";
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
                        driver.FindElement(By.Id("login_form_password")).SendKeys("");
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
            driver.FindElement(By.Id("switch_captchas_button")).Click();
            IWebElement logo = driver.FindElement(By.XPath("//*[@id='captchasnet_free_play_captcha']/div[1]/img"));
            String logoSRC = logo.GetAttribute("src");
            Uri imageURL = new Uri(logoSRC);
            PuthToPicture = Rucaptcha.Download_Captcha(imageURL.ToString());
            //Thread.Sleep(1000);
            pictureBox1.ImageLocation = PuthToPicture;
            try
            {
                driver.FindElement(By.XPath("//*[@id='captchasnet_free_play_captcha']/input[2]")).SendKeys(Rucaptcha.Recognize(PuthToPicture));
            }
            catch (OpenQA.Selenium.ElementNotVisibleException)
            {
                driver.FindElement(By.Id("switch_captchas_button")).Click();
                driver.FindElement(By.XPath("//*[@id='captchasnet_free_play_captcha']/input[2]")).SendKeys(Rucaptcha.Recognize(PuthToPicture));
            }
            logo = driver.FindElement(By.XPath("//*[@id='botdetect_free_play_captcha']/div[1]/img"));
            logoSRC = logo.GetAttribute("src");
            imageURL = new Uri(logoSRC);
            PuthToPicture = Rucaptcha.Download_Captcha(imageURL.ToString());
            //Thread.Sleep(1000);
            pictureBox2.ImageLocation = PuthToPicture;
            try
            {
                driver.FindElement(By.XPath("//*[@id='botdetect_free_play_captcha']/input[2]")).SendKeys(Rucaptcha.Recognize(PuthToPicture));
            }
            catch (OpenQA.Selenium.ElementNotVisibleException)
            {
                driver.FindElement(By.Id("switch_captchas_button")).Click();
                driver.FindElement(By.XPath("//*[@id='botdetect_free_play_captcha']/input[2]")).SendKeys(Rucaptcha.Recognize(PuthToPicture));
            }

            driver.FindElement(By.CssSelector(".close-reveal-modal")).Click();
            winnings += Convert.ToDouble(driver.FindElement(By.Id("winnings")).Text);
            label1.Text = Convert.ToString(winnings);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            //Записываем в настройки персональный Rucaptcha key
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            /*
             * Скачиваем картинку по ссылке
             * и сохраняем ее в папку \captcha_images\
             * которая появится возле исполняемого файла (.exe)
             * и записываем в переменную PuthToPicture 
             * путь к картинке (капче), которую скачали
             */
            PuthToPicture = Rucaptcha.Download_Captcha("https://captchas.freebitco.in/cgi-bin/captcha_generator?client=freebitcoin&random=OsGPzk6irqVe4YX3xMxw8ivvtrYVsGC8");
            /*
             * Если переменная PuthToPicture пустая
             * значит скачивание не произошло из-за
             * отсутствия интернет-соединения
             * либо из-за битой ссылки (URL)
             * 
             * Если же переменная не пустая - отображаем 
             * в pictureBox1 скачавшуюся капчу
             */
            if (PuthToPicture != "")
                pictureBox1.ImageLocation = PuthToPicture;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            /*
             * Отправляем скачавшуюся ранее картинку на разгадывание
             * Если ответ будет содержать слово ERROR, значит возникла 
             * определенная ошибка, которая будет описана в ответе после слова ERROR            
             * 
             * В ином же случае, если слова ERROR в ответе не будет, значит сервис 
             * вернет текст отправленной капчи 
             */
            MessageBox.Show(Rucaptcha.Recognize(PuthToPicture));
            /* Ответ от функци, вероятно, придётся ждать определенное время
             * Во-первых, мы ждем наличия свободного работника 
             * бывает что все заняты, бывает что все свободны — всегда по разному
             * И во-вторых, ждем, пока работник разгадает капчу 
             * 
             * И да, при использовании этой функции в определённых программах  рекомендую ее вешать 
             * на отдельный поток, чтобы при этом не зависала основная форма программы
             */
        }

        private void button4_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            PuthToPicture = openFileDialog1.FileName;
            if (PuthToPicture != "")
                pictureBox1.ImageLocation = PuthToPicture;
        }
    }
}
