using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;
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
        int RP_cost = 0;
        int stepped = 777;
        int multed = 777;
        int RP_bonus_cost = 0;
        bool busy = false;
        bool multiply_busy = false;
        IWebElement logo;
        String logoSRC;
        Uri imageURL;
        List<int> timing_list = new List<int>();
        List<int> multiply_list = new List<int>();
        List<double> multiply3_list = new List<double>();
        ChromeOptions options;
        public IWebDriver driver;
        public IWebDriver driver1;
        



        private String dbFileName = "sample.sqlite";
        private SQLiteConnection m_dbConn;
        private SQLiteCommand m_sqlCmd;
        SQLiteDataReader sqlite_datareader;

        public void screen()
        {
            Graphics graph = null;

            var bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

            graph = Graphics.FromImage(bmp);

            graph.CopyFromScreen(0, 0, 0, 0, bmp.Size);
            try
            {
                bmp.Save(Application.StartupPath + @"\reload\" + Convert.ToString(DateTime.Now).Replace(":", "_") + ".jpg");
            }
            catch (System.Runtime.InteropServices.ExternalException)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(Application.StartupPath + @"\reload");
                if (!dirInfo.Exists)
                {
                    dirInfo.Create();
                }
                bmp.Save(Application.StartupPath + @"\reload\" + Convert.ToString(DateTime.Now).Replace(":", "_") + ".jpg");

            }
        }

        public static void KillChrome(string name)
        {
            Process[] procs = null;

            try
            {
                procs = Process.GetProcessesByName(name);

                foreach (var item in procs)
                {
                    if (!item.HasExited)
                    {
                        item.Kill();
                    }
                }


            }
            finally
            {
                if (procs != null)
                {
                    foreach (Process p in procs)
                    {
                        p.Dispose();
                    }
                }
            }
        }

        public void write_balance(IWebDriver driver, int i)
        {
            int balance;

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].setAttribute('style','display');", driver.FindElement(By.CssSelector(".large-12.fixed")));

            balance = Convert.ToInt32(driver.FindElement(By.Id("balance")).Text.Replace(".", ""));

            m_sqlCmd = m_dbConn.CreateCommand();
            m_sqlCmd.CommandText = "SELECT id FROM Balance WHERE id ='" + i + "'";
            try
            {
                sqlite_datareader = m_sqlCmd.ExecuteReader();
                sqlite_datareader.Read(); //sqlite_datareader.GetInt32(0)
                sqlite_datareader.GetInt32(0);
                sqlite_datareader.Close();
                m_sqlCmd.CommandText = "update Balance set satoshi = " + balance + " where ID = " + i;
                m_sqlCmd.ExecuteNonQuery();
            }
            catch (System.InvalidOperationException)
            {
                sqlite_datareader.Close();
                m_sqlCmd.CommandText = "INSERT INTO Balance ('id', 'satoshi') values ('" + i + "', '" + balance + "' )";

                m_sqlCmd.ExecuteNonQuery();
            }
        }

        public void write_faucet(IWebDriver driver, int i)
        {
            UpdateLog2("(" + i + ")Запись выплаты...");
            string date = Convert.ToString(DateTime.Now).Substring(0, 5);
            int RP = Convert.ToInt32(driver.FindElement(By.Id("fp_reward_points_won")).Text.Replace(".", ""));
            int BTC = Convert.ToInt32(driver.FindElement(By.Id("winnings")).Text.Replace(".", ""));

            

            m_sqlCmd = m_dbConn.CreateCommand();
            m_sqlCmd.CommandText = "SELECT id FROM Log WHERE (Date, Akk) = (" + date + ", " + i + ")";
            try
            {
                sqlite_datareader = m_sqlCmd.ExecuteReader();
                sqlite_datareader.Read(); //sqlite_datareader.GetInt32(0)
                sqlite_datareader.GetInt32(0);
                sqlite_datareader.Close();
                m_sqlCmd.CommandText = "update Log set (Faucet, RP, BTC) = (Faucet+1, RP+" + RP + ", BTC+" + BTC + ") where (Date, Akk) = (" + date + ", " + i +")";
                m_sqlCmd.ExecuteNonQuery();
            }
            catch (InvalidOperationException)
            {
                sqlite_datareader.Close();
                m_sqlCmd.CommandText = "INSERT INTO Log (Date, Akk, Faucet, RP, BTC) values (" + date + ", " + i + ", 1, " + RP + ", " + BTC + ")";

                m_sqlCmd.ExecuteNonQuery();
            }
            UpdateLog2("(" + i + ")Выплата записана");
        }

        public void write_faucet_num(IWebDriver driver, int i)
        {
            UpdateLog2("(" + i + ")Запись выплат...");
            string date = Convert.ToString(DateTime.Now).Substring(0, 5);//.Substring(0, 7)

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].setAttribute('style','display');", driver.FindElement(By.CssSelector(".large-12.fixed")));
            driver.FindElement(By.PartialLinkText("STATS")).Click();
            driver.FindElement(By.Id("personal_stats_button")).Click();
            new WebDriverWait(driver, TimeSpan.FromSeconds(10)).Until(ExpectedConditions.ElementIsVisible(By.Id("user_free_spins_played")));

            int faucet = Convert.ToInt32(driver.FindElement(By.Id("user_free_spins_played")).Text.Replace(",", ""));

            m_sqlCmd = m_dbConn.CreateCommand();
            m_sqlCmd.CommandText = "SELECT id FROM Faucet_num WHERE (Date, Akk) = (" + date + ", " + i + ")";
            try
            {
                sqlite_datareader = m_sqlCmd.ExecuteReader();
                sqlite_datareader.Read(); //sqlite_datareader.GetInt32(0)
                sqlite_datareader.GetInt32(0);
                sqlite_datareader.Close();
                m_sqlCmd.CommandText = "update Faucet_num set (Faucet) = (" + faucet + ") where (Date, Akk) = (" + date + ", " + i + ")";
                m_sqlCmd.ExecuteNonQuery();
            }
            catch (InvalidOperationException)
            {
                sqlite_datareader.Close();
                m_sqlCmd.CommandText = "INSERT INTO Faucet_num (Date, Akk, Faucet) values (" + date + ", " + i + ", " + faucet + ")";

                m_sqlCmd.ExecuteNonQuery();
            }
            UpdateLog2("(" + i + ")Выплаты записана");
        }

        public int multiply(IWebDriver driver)
        {
            string[] words;
            double result;
            int luz_num = 0;
            double wager = 100;
            //double bet = 0.00000001;

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].setAttribute('style','display');", driver.FindElement(By.CssSelector(".large-12.fixed")));

            if (Convert.ToInt32(driver.FindElement(By.Id("balance")).Text.Replace(".", "")) > 30000)
            {
                try
                {
                    
                    FandS(driver, "REQUIREMENTS TO UNLOCK BONUSES").Click();
                    Thread.Sleep(1000);
                    FandS(driver, "//*[@id='unblock_modal_rp_bonuses_container']/div[1]").Click();
                    Thread.Sleep(1000);
                    //UpdateLog2(FandS(driver, "option_container_buy_lottery").Text);

                    words = FandS(driver, "option_container_buy_lottery").Text.Split(new char[] { ' ' });
                    //UpdateLog2(words[1]);
                    wager = Convert.ToDouble(words[1].Replace(".", ","));

                    if (wager >= 0.00002000)
                    {
                        wager = 0.00002000;
                    }

                    driver.Navigate().Refresh();

                    driver.FindElement(By.PartialLinkText("MULTIPLY BTC")).Click();

                    driver.FindElement(By.Id("double_your_btc_stake")).SendKeys("d");
                    Thread.Sleep(200);
                    driver.FindElement(By.Id("double_your_btc_stake")).SendKeys("h");
                    Thread.Sleep(400);
                    result = Convert.ToDouble(driver.FindElement(By.XPath("//*[@id='bet_history_table_rows']/div[3]/div[1]/div[7]/font")).Text.Replace(".", ","));


                    do
                    {
                        if (luz_num >= 5)
                        {
                            result = result * 2;
                            
                            if (words[0] == driver.FindElement(By.XPath("//*[@id='bet_history_table_rows']/div[3]/div[1]/div[1]")).Text)
                            {
                                while (words[0] == driver.FindElement(By.XPath("//*[@id='bet_history_table_rows']/div[3]/div[1]/div[1]")).Text)
                                {
                                    //UpdateLog2(words[0]);
                                    Thread.Sleep(1000);
                                }
                            }
                            else
                            {
                                driver.FindElement(By.Id("double_your_btc_stake")).Clear();
                                driver.FindElement(By.Id("double_your_btc_stake")).SendKeys(result.ToString("F8").Replace("-", "").Replace(",", "."));
                                //Thread.Sleep(1000);
                                words[0] = driver.FindElement(By.XPath("//*[@id='bet_history_table_rows']/div[3]/div[1]/div[1]")).Text;
                                Thread.Sleep(1000);
                                driver.FindElement(By.Id("double_your_btc_stake")).SendKeys("h");
                            }
                        }
                        else
                        {

                            driver.FindElement(By.Id("double_your_btc_stake")).Clear();
                            driver.FindElement(By.Id("double_your_btc_stake")).SendKeys("d");
                            //Thread.Sleep(200);
                            driver.FindElement(By.Id("double_your_btc_stake")).SendKeys("h");
                        }
                        
                        Thread.Sleep(400);
                        result = Convert.ToDouble(driver.FindElement(By.XPath("//*[@id='bet_history_table_rows']/div[3]/div[1]/div[7]/font")).Text.Replace(".", ","));

                        if (result < 0)
                        {
                            luz_num++;

                            wager += result;
                        }

                        else
                        {
                            luz_num = 0;
                            //bet = 0.00000001;
                            wager -= result;
                        }
                        //UpdateLog2(Convert.ToString(result));
                        //UpdateLog2(Convert.ToString(wager));

                    } while (wager >= 0 || luz_num != 0);
                }
                catch (Exception)
                {
                    UpdateLog2("исключение");
                    return 3600;
                }
                return 10;
            }
            return 3600;
        }

        public void check_multiply(IWebDriver driver, int i)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            driver.Navigate().Refresh();

            wait.Until(ExpectedConditions.ElementIsVisible(By.PartialLinkText("REQUIREMENTS TO UNLOCK BONUSES")));
            
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].setAttribute('style','display:none;');", driver.FindElement(By.CssSelector(".large-12.fixed")));
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", driver.FindElement(By.PartialLinkText("REQUIREMENTS TO UNLOCK BONUSES")));


                driver.FindElement(By.PartialLinkText("REQUIREMENTS TO UNLOCK BONUSES")).Click();
            //Thread.Sleep(1000);
            try
            {
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@id='unblock_modal_rp_bonuses_container']/div[1]")));
                driver.FindElement(By.XPath("//*[@id='unblock_modal_rp_bonuses_container']/div[1]")).Click();

                UpdateLog2("(" + i + ")Найдено условие разблокировки бонусов, поставлено в очередь мультика.");
                
                multiply_list.Add(i);
                multiply_list.Distinct();

                return;
            }
            catch (Exception)
            {
                UpdateLog2("(" + i + ")Мультик не требуется.");

            }

        }

        public double check_multiply2(int i)
        {
            string[] result;
            

            m_sqlCmd = m_dbConn.CreateCommand();
            m_sqlCmd.CommandText = "SELECT Cookie FROM Cookie_setting WHERE id = " + i;
            sqlite_datareader = m_sqlCmd.ExecuteReader();
            sqlite_datareader.Read();
            string Cookie = sqlite_datareader.GetString(0);
            sqlite_datareader.Close();

            m_sqlCmd = m_dbConn.CreateCommand();
            m_sqlCmd.CommandText = "SELECT csrf_token FROM Cookie_setting WHERE id = " + i;
            sqlite_datareader = m_sqlCmd.ExecuteReader();
            sqlite_datareader.Read();
            string csrf_token = sqlite_datareader.GetString(0);
            sqlite_datareader.Close();

            m_sqlCmd = m_dbConn.CreateCommand();
            m_sqlCmd.CommandText = "SELECT u FROM Cookie_setting WHERE id = " + i;
            sqlite_datareader = m_sqlCmd.ExecuteReader();
            sqlite_datareader.Read();
            int u = sqlite_datareader.GetInt32(0);
            sqlite_datareader.Close();

            m_sqlCmd = m_dbConn.CreateCommand();
            m_sqlCmd.CommandText = "SELECT p FROM Cookie_setting WHERE id = " + i;
            sqlite_datareader = m_sqlCmd.ExecuteReader();
            sqlite_datareader.Read();
            string p = sqlite_datareader.GetString(0);
            sqlite_datareader.Close();


            var baseAddress = new Uri("https://freebitco.in/stats_new_private/?u=" + u + "&p=" + p + "&f=user_stats&csrf_token=" + csrf_token);//csrf_token=5qSeDTS7kOuM
            using (var handler = new HttpClientHandler { UseCookies = false, AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate })
            using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, baseAddress);
                httpRequestMessage.Headers.Add("Host", "freebitco.in");
                httpRequestMessage.Headers.Add("Connection", "keep-alive");
                httpRequestMessage.Headers.Add("Accept", "*/*");
                httpRequestMessage.Headers.Add("x-csrf-token", "5qSeDTS7kOuM");
                httpRequestMessage.Headers.Add("X-Requested-With", "XMLHttpRequest");
                httpRequestMessage.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.77 Safari/537.36");
                httpRequestMessage.Headers.Add("Referer", "https://freebitco.in/");
                httpRequestMessage.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                httpRequestMessage.Headers.Add("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
                httpRequestMessage.Headers.Add("Cookie", Cookie);//__cfduid=d2b96ce17dae0d3efb78035d3691b39f61529329108; csrf_token=5qSeDTS7kOuM; _ga=GA1.2.1222776224.1529329112; have_account=1; free_play_sound=1; cookieconsent_dismissed=yes; hide_pass_reuse2_msg=1; hide_earn_btc_msg=1; hide_m_btc_comm_inc_msg=1; default_captcha=double_captchas; _gid=GA1.2.340769844.1540792310; btc_address=1JhVKTqeQBXdEwRXhrjao45uLF8dB2RQnb; password=ee6a35b5074bf90c471d5900b3d489edcba04dbc34b8d18dd1d98e1d80762cc9; login_auth=e8992cf572d6575fd03b16f3f20c0e6ac5945b7c17ce83ac69bcdeabc2eafc0e;


                result = client.SendAsync(httpRequestMessage).Result.Content.ReadAsStringAsync().Result.Split(new char[] { '"' });
                Thread.Sleep(100);



                if (Convert.ToDouble(result[result.Length - 2].Replace(".", ",")) <= 0)
                {
                    return 0;
                }
                else
                {
                    return Convert.ToDouble(result[85].Replace(".", ","));
                }
                
            }
        }


        public async void multiply2(int i)
        {
                await Task.Run(() =>
                {
                    try
                    {
                    string[] words;
                    double result = 0.00000001;
                    int luz_num = 0;
                    double wager;
                    string roll = "0";
                    int roll_wait = 0;
                    int old_wager = 777;

                    options = new ChromeOptions();
                    Proxy proxy = new Proxy();
                    proxy.Kind = ProxyKind.Manual;
                    proxy.IsAutoDetect = false;
                    proxy.HttpProxy = data_get_proxy(i);
                    proxy.SslProxy = data_get_proxy(i);
                    options.Proxy = proxy;
                    options.AddArgument("ignore-certificate-errors");
                    //options.AddArgument("--headless");

                    options.AddArguments(@"user-data-dir=" + Application.StartupPath + @"\" + data_get_prof(i));
                    options.AddArguments("--start-maximized");
                    driver1 = new ChromeDriver(options);
                    driver1.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
                    WebDriverWait wait = new WebDriverWait(driver1, TimeSpan.FromSeconds(10));

                    do
                    {
                        try
                        {
                            driver1.Navigate().GoToUrl("https://freebitco.in/");
                            luz_num = 0;
                        }
                        catch (Exception)
                        {
                            luz_num++;
                        }
                    } while (luz_num != 0 || luz_num > 10);

                    if (luz_num > 10)
                    {
                        UpdateLog2("страница мультиплэя не загрузилась с 10 попыток");
                        timing_list[i] = 200;
                        driver1.Quit();
                        multiply_busy = false;
                        multed = 777;
                        return;
                    }

                    ((IJavaScriptExecutor)driver1).ExecuteScript("arguments[0].setAttribute('style','display');", driver1.FindElement(By.CssSelector(".large-12.fixed")));

                    int old_btc = Convert.ToInt32(driver1.FindElement(By.Id("balance")).Text.Replace(".", ""));

                    if (old_btc < 20000)
                    {
                            UpdateLog2("(" + i + ")баланс меньше 20000, отмена мультика.");
                            multiply_list.Remove(i);
                            timing_list[i] = 3600;
                            driver1.Quit();
                            multiply_busy = false;
                            multed = 777;
                            return;
                    }


                    try
                    {
                        wait.Until(ExpectedConditions.ElementIsVisible(By.PartialLinkText("REQUIREMENTS TO UNLOCK BONUSES")));
                        driver1.FindElement(By.PartialLinkText("REQUIREMENTS TO UNLOCK BONUSES")).Click();

                        wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@id='unblock_modal_rp_bonuses_container']/div[1]")));
                        driver1.FindElement(By.XPath("//*[@id='unblock_modal_rp_bonuses_container']/div[1]")).Click();

                        wait.Until(ExpectedConditions.ElementIsVisible(By.Id("option_container_buy_lottery")));
                        words = driver1.FindElement(By.Id("option_container_buy_lottery")).Text.Split(new char[] { ' ' });
                        wager = Convert.ToDouble(words[1].Replace(".", ","));
                        old_wager = Convert.ToInt32(words[1].Replace(".", ""));


                    }
                    catch (Exception)
                    {
                            timing_list[i] = 10;
                            
                            multiply_list.Remove(i);
                            driver1.Quit();
                            multiply_busy = false;
                            multed = 777;
                            return;
                        }

                    //if (wager >= 0.00002000)
                    //{
                    //    wager = 0.00002000;
                    //    old_wager = 2000;
                    //}

                    
                    driver1.FindElement(By.PartialLinkText("MULTIPLY BTC")).Click();
                    do
                    {


                        if (luz_num >= 6)
                        {
                            result = result * 2;

                            driver1.FindElement(By.Id("double_your_btc_stake")).Clear();
                            driver1.FindElement(By.Id("double_your_btc_stake")).SendKeys(result.ToString("F8").Replace("-", "").Replace(",", "."));
                            roll = driver1.FindElement(By.XPath("//*[@id='bet_history_table_rows']/div[3]/div[1]/div[4]")).Text;
                            driver1.FindElement(By.Id("double_your_btc_stake")).SendKeys("h");
                        }
                        else
                        {
                            driver1.FindElement(By.Id("double_your_btc_stake")).Clear();
                            driver1.FindElement(By.Id("double_your_btc_stake")).SendKeys("d");
                            try
                            {
                                roll = driver1.FindElement(By.XPath("//*[@id='bet_history_table_rows']/div[3]/div[1]/div[4]")).Text;
                            }
                            catch (NoSuchElementException)
                            {
                                roll = "0";
                                Thread.Sleep(5000);
                            }
                            driver1.FindElement(By.Id("double_your_btc_stake")).SendKeys("h");
                        }


                        try
                        {
                            while (roll == driver1.FindElement(By.XPath("//*[@id='bet_history_table_rows']/div[3]/div[1]/div[4]")).Text && roll_wait < 20)
                            {
                                Thread.Sleep(200);
                                roll_wait++;

                            }
                        }
                        catch (Exception)
                        {
                            timing_list[i] = 10;

                            driver1.Quit();
                            multiply_busy = false;
                            multed = 777;
                            return;
                        }
                        roll_wait = 0;



                        result = Convert.ToDouble(driver1.FindElement(By.XPath("//*[@id='bet_history_table_rows']/div[3]/div[1]/div[7]/font")).Text.Replace(".", ","));
                        if (driver1.FindElement(By.Id("double_your_btc_bet_lose")).Displayed)
                        {
                            luz_num++;
                            wager += result;
                        }
                        if (driver1.FindElement(By.Id("double_your_btc_bet_win")).Displayed)
                        {
                            luz_num = 0;

                            wager -= result;
                        }

                    } while (wager >= 0 || luz_num != 0);

                    int new_btc = Convert.ToInt32(driver1.FindElement(By.Id("balance")).Text.Replace(".", "")) - old_btc;

                    m_sqlCmd.CommandText = "INSERT INTO Multiply_stat ('id_prof', 'result', 'wager', 'date') values ('" + i + "', '" + new_btc + "', '" + old_wager + "', '" + DateTime.Now + "' )";

                    m_sqlCmd.ExecuteNonQuery();
                    multiply_list.Distinct();
                    multiply_list.Remove(i);
                    timing_list[i] = 10;

                    driver1.Quit();
                    multiply_busy = false;
                    multed = 777;
                    return;
                    }

                    catch (Exception ex)
                    {
                        UpdateLog2("Ошибка мультика" + ex);
                        timing_list[i] = 10;
                        driver1.Quit();
                        multiply_busy = false;
                        multed = 777;
                        return;
                    }
        });

            


        }

        static string Bet(string Cookie, string csrf_token, string bet)
        {
            Random random = new Random();
            string[] result;
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxz";
            string rand = "0123456789";
            chars = new string(Enumerable.Repeat(chars, 16).Select(s => s[random.Next(s.Length)]).ToArray());
            rand = new string(Enumerable.Repeat(rand, 17).Select(s => s[random.Next(s.Length)]).ToArray());

            var baseAddress = new Uri("https://freebitco.in/cgi-bin/bet.pl?m=hi&client_seed=" + chars + "&jackpot=0&stake=" + bet.Replace(",", ".") + "&multiplier=2.00&rand=0." + rand + "&csrf_token=" + csrf_token);//csrf_token=5qSeDTS7kOuM
            using (var handler = new HttpClientHandler { UseCookies = false, AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate })
            using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, baseAddress);
                httpRequestMessage.Headers.Add("Host", "freebitco.in");
                httpRequestMessage.Headers.Add("Connection", "keep-alive");
                httpRequestMessage.Headers.Add("Accept", "*/*");
                httpRequestMessage.Headers.Add("x-csrf-token", "5qSeDTS7kOuM");
                httpRequestMessage.Headers.Add("X-Requested-With", "XMLHttpRequest");
                httpRequestMessage.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.77 Safari/537.36");
                httpRequestMessage.Headers.Add("Referer", "https://freebitco.in/");
                httpRequestMessage.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                httpRequestMessage.Headers.Add("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
                httpRequestMessage.Headers.Add("Cookie", Cookie);//__cfduid=d2b96ce17dae0d3efb78035d3691b39f61529329108; csrf_token=5qSeDTS7kOuM; _ga=GA1.2.1222776224.1529329112; have_account=1; free_play_sound=1; cookieconsent_dismissed=yes; hide_pass_reuse2_msg=1; hide_earn_btc_msg=1; hide_m_btc_comm_inc_msg=1; default_captcha=double_captchas; _gid=GA1.2.340769844.1540792310; btc_address=1JhVKTqeQBXdEwRXhrjao45uLF8dB2RQnb; password=ee6a35b5074bf90c471d5900b3d489edcba04dbc34b8d18dd1d98e1d80762cc9; login_auth=e8992cf572d6575fd03b16f3f20c0e6ac5945b7c17ce83ac69bcdeabc2eafc0e;


                result = client.SendAsync(httpRequestMessage).Result.Content.ReadAsStringAsync().Result.Split(new char[] { ':' });
                Thread.Sleep(100);
                return result[1];
            }
        }

        public async void multiply3(int i)
        {
            await Task.Run(() =>
            {
                if (multiply_list.Contains(i))
                {
                    return;
                }

                multiply_list.Add(i);

                double wager = check_multiply2(i);

                if (wager == 0)
                {
                    timing_list[i] = 10;
                    multiply_list.Remove(i);
                    return;
                }

                int luz_num = 0;
                string sing;
                double bet = 0.00000001;


                m_sqlCmd = m_dbConn.CreateCommand();
                m_sqlCmd.CommandText = "SELECT Cookie FROM Cookie_setting WHERE id = " + i;
                sqlite_datareader = m_sqlCmd.ExecuteReader();
                sqlite_datareader.Read();
                string Cookie = sqlite_datareader.GetString(0);
                sqlite_datareader.Close();

                m_sqlCmd = m_dbConn.CreateCommand();
                m_sqlCmd.CommandText = "SELECT csrf_token FROM Cookie_setting WHERE id = " + i;
                sqlite_datareader = m_sqlCmd.ExecuteReader();
                sqlite_datareader.Read();
                string csrf_token = sqlite_datareader.GetString(0);
                sqlite_datareader.Close();

                

                do
                {
                    sing = Bet(Cookie, csrf_token, bet.ToString("F8"));
                    //UpdateLog3("(" + i + ")" + bet.ToString("F8") + "_" + luz_num + sing + "_" + wager.ToString("F8"));

                    multiply3_list[i] = wager;

                    if (sing == "l")
                    {
                        luz_num++;
                        wager -= bet;

                        if (luz_num >= 6)
                        {
                            bet *= 2;
                        }
                        
                    }
                    else
                    {
                        luz_num = 0;
                        wager -= bet;
                        bet = 0.00000001;
                    }

                } while (wager > 0 || luz_num != 0);

                timing_list[i] = 10;
                multiply_list.Remove(i);
            });
        }

        public bool solve_text2(IWebDriver driver, IWebElement image, IWebElement field)
        {
            Uri imageURL = new Uri(image.GetAttribute("src"));

            String url = imageURL.ToString();

            string localFilename = Application.StartupPath + @"\captcha_images2\12.jpg";
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(url, localFilename);
            }

            String uriString = "http://rucaptcha.com/in.php?key=" + textBox1.Text;
            WebClient myWebClient = new WebClient();

            
            byte[] responseArray = myWebClient.UploadFile(uriString, localFilename);
            string text = Encoding.ASCII.GetString(responseArray).Substring(3);                 //ID капчи

            
            byte[] postArray = Encoding.ASCII.GetBytes(text);
            uriString = "http://rucaptcha.com/res.php?key=" + textBox1.Text + "&action=get&id=" + text;

            string solve;

            do
            {
                Thread.Sleep(5000);
                responseArray = myWebClient.UploadData(uriString, postArray);
                solve = Encoding.ASCII.GetString(responseArray);

            } while (solve == "CAPCHA_NOT_READY");

            solve = solve.Substring(3);


            foreach (var item in solve)
            {
                Convert.ToInt32(item);
                if (item >= 48 && item <= 57)
                {
                    UpdateLog2("эта капча " + solve + " содержит числа");


                    uriString = "http://rucaptcha.com/res.php?key=" + textBox1.Text + "&action=reportbad&id=" + text;
                    responseArray = myWebClient.UploadData(uriString, postArray);
                    UpdateLog2(Encoding.ASCII.GetString(responseArray));

                    return false;
                }
            }

            if (solve.Length != 6)
            {
                UpdateLog2("эта капча " + solve + " не из 6 символов");

                uriString = "http://rucaptcha.com/res.php?key=" + textBox1.Text + "&action=reportbad&id=" + text;
                responseArray = myWebClient.UploadData(uriString, postArray);
                UpdateLog2(Encoding.ASCII.GetString(responseArray));

                return false;
            }

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", field);
            field.SendKeys(solve);

            return true;
        }
        
        public void send_vk(string text, IWebDriver driver)
        {
            ((IJavaScriptExecutor)driver).ExecuteScript("window.open()");
            driver.SwitchTo().Window(driver.WindowHandles.Last());

            driver.Navigate().GoToUrl("https://vk.com/im?sel=8300061");
            driver.FindElement(By.Id("im_editable8300061")).SendKeys(text);
            driver.FindElement(By.XPath("//*[@id='content']/div/div[1]/div[3]/div[3]/div[4]/div[3]/div[3]/div[1]/button")).Click();
            Thread.Sleep(1000);


            driver.Close();
            driver.SwitchTo().Window(driver.WindowHandles.Last());
        }

        public void add_good_proxy(string proxy)
        {
            m_sqlCmd = m_dbConn.CreateCommand();
            m_sqlCmd.CommandText = "SELECT id FROM Proxy_list WHERE proxy ='" + proxy + "'";
            try
            {
                sqlite_datareader = m_sqlCmd.ExecuteReader();
                sqlite_datareader.Read(); //sqlite_datareader.GetInt32(0)
                sqlite_datareader.GetInt32(0);
                sqlite_datareader.Close();
            }
            catch (System.InvalidOperationException)
            {
                sqlite_datareader.Close();
                m_sqlCmd.CommandText = "INSERT INTO Proxy_list ( 'proxy', 'usage') values ( '" + proxy + "' , 0)";

                m_sqlCmd.ExecuteNonQuery();
            }
        }

        public void update_proxy_list()
        {
            //m_sqlCmd = m_dbConn.CreateCommand();
            //m_sqlCmd.CommandText = "DELETE FROM Proxy_list";
            //m_sqlCmd.ExecuteNonQuery();

            HtmlWeb web = new HtmlWeb();

            var htmlDoc = web.Load(@"https://github.com/clarketm/proxy-list/blob/master/proxy-list.txt");

            //int id = 0;
            int id_num = 5;
            string text;
            string[] words;

            Ping ping = new Ping();
            PingReply pingReply = null;

            do
            {
                text = htmlDoc.GetElementbyId("LC" + id_num).InnerText;
                words = text.Split(new char[] { ':' });
                pingReply = ping.Send(words[0]);

                if (text.Length > 3 && pingReply.Status != IPStatus.TimedOut && pingReply.RoundtripTime < 200)
                {
                    words = text.Split(new char[] { ' ' });
                    UpdateLog2(words[0]);

                    add_good_proxy(words[0]);

                    //m_sqlCmd.CommandText = "INSERT INTO Proxy_list ( 'proxy', 'usage') values ( '" + words[0] + "' , 0)";

                    //m_sqlCmd.ExecuteNonQuery();

                    //id++;
                    id_num++;
                }
                else
                {
                    return;
                }


            } while (id_num <= 400);
        }

        public void proxy_change(int id_prof)
        {
            int id_proxy = 1;
            string new_proxy;
            int usage;

            
            try
            {
                m_sqlCmd = m_dbConn.CreateCommand();
                m_sqlCmd.CommandText = "SELECT usage FROM Proxy_list WHERE id = " + id_proxy;
                sqlite_datareader = m_sqlCmd.ExecuteReader();
                sqlite_datareader.Read();
                usage = sqlite_datareader.GetInt32(0);
                
            }
            catch (System.InvalidOperationException)
            {
                MessageBox.Show("Список прокси пуст, обновляю список.", "Error", MessageBoxButtons.OK);

                using (StreamReader sr = new StreamReader(Application.StartupPath + @"\proxy.txt"))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        add_good_proxy(line);
                    }
                }

                m_sqlCmd = m_dbConn.CreateCommand();
                m_sqlCmd.CommandText = "SELECT usage FROM Proxy_list WHERE id = " + id_proxy;
                sqlite_datareader = m_sqlCmd.ExecuteReader();
                sqlite_datareader.Read();
                usage = sqlite_datareader.GetInt32(0);
                
            }

            sqlite_datareader.Close();
            UpdateLog2("id_proxy - " + id_proxy + usage);

            while (usage != 0)
            {
                id_proxy++;

                m_sqlCmd = m_dbConn.CreateCommand();
                m_sqlCmd.CommandText = "SELECT usage FROM Proxy_list WHERE id = " + id_proxy;
                sqlite_datareader = m_sqlCmd.ExecuteReader();
                sqlite_datareader.Read();
                usage = sqlite_datareader.GetInt32(0);

                sqlite_datareader.Close();
                UpdateLog2("while_id_proxy - " + id_proxy + usage);
            }

            if (usage == 0)
            {
                m_sqlCmd = m_dbConn.CreateCommand();
                m_sqlCmd.CommandText = "SELECT proxy FROM Proxy_list WHERE id = " + id_proxy;
                sqlite_datareader = m_sqlCmd.ExecuteReader();
                sqlite_datareader.Read();

                new_proxy = sqlite_datareader.GetString(0);
                
                sqlite_datareader.Close();

                m_sqlCmd.CommandText = "update Setting set proxy = '" + new_proxy + "' where ID=" + id_prof;

                m_sqlCmd.ExecuteNonQuery();

                m_sqlCmd.CommandText = "update Proxy_list set usage = 1 where ID=" + id_proxy;

                m_sqlCmd.ExecuteNonQuery();
                UpdateLog2("if_id_proxy - " + id_proxy + usage);
            }
            else
            {
                UpdateLog2("Все прокси хуевые");
            }

            
        }

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

            foreach (var item in timing_list)
            {
                multiply3_list.Add(0);
            }
        }

        public string data_get_akk(int id_prof)
        {
            m_sqlCmd = m_dbConn.CreateCommand();
            m_sqlCmd.CommandText = "SELECT akk FROM Setting WHERE id = " + id_prof;
            sqlite_datareader = m_sqlCmd.ExecuteReader();
            sqlite_datareader.Read();

            string akk = sqlite_datareader.GetString(0);
            sqlite_datareader.Close();

            
            return akk;
        }

        public string data_get_pass(int id_prof)
        {
            m_sqlCmd = m_dbConn.CreateCommand();
            m_sqlCmd.CommandText = "SELECT pass FROM Setting WHERE id = " + id_prof;
            sqlite_datareader = m_sqlCmd.ExecuteReader();
            sqlite_datareader.Read();

            string pass = sqlite_datareader.GetString(0);
            sqlite_datareader.Close();


            return pass;
        }
        
        public string data_get_proxy(int id_prof)
        {
            if (id_prof == 777)
            {
                return " ";
            }

            m_sqlCmd = m_dbConn.CreateCommand();
            m_sqlCmd.CommandText = "SELECT proxy FROM Setting WHERE id = " + id_prof;
            sqlite_datareader = m_sqlCmd.ExecuteReader();
            sqlite_datareader.Read();

            string proxy = sqlite_datareader.GetString(0);

            if (proxy == " ")
            {
                proxy_change(id_prof);

                m_sqlCmd = m_dbConn.CreateCommand();
                m_sqlCmd.CommandText = "SELECT proxy FROM Setting WHERE id = " + id_prof;
                sqlite_datareader = m_sqlCmd.ExecuteReader();
                sqlite_datareader.Read();

                proxy = sqlite_datareader.GetString(0);
                sqlite_datareader.Close();
                return proxy;
            }
            else
            {
                sqlite_datareader.Close();
                return proxy;
            }
        }

        public string data_get_prof(int id_prof)
        {
            if (id_prof == 777)
            {
                return "7";
            }

            m_sqlCmd = m_dbConn.CreateCommand();
            m_sqlCmd.CommandText = "SELECT prof FROM Setting WHERE id = " + id_prof;
            sqlite_datareader = m_sqlCmd.ExecuteReader();
            sqlite_datareader.Read();

            string prof = sqlite_datareader.GetString(0);
            sqlite_datareader.Close();

            return prof;
        }

        public bool simple_captcha2(IWebDriver driver)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            
            try
            {
                if (!IsElementVisible(driver.FindElement(By.Id("free_play_double_captchas"))) && IsElementVisible(driver.FindElement(By.Id("free_play_recaptcha"))))
                {
                    driver.FindElement(By.Id("switch_captchas_button")).Click();
                }

                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@id='botdetect_free_play_captcha']/div[1]/img")));

                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", driver.FindElement(By.XPath("//*[@id='botdetect_free_play_captcha']/input[2]")));

            }
            catch (Exception)
            {

                return false;
            }
            
            do
            {
                try
                {
                    logo = driver.FindElement(By.XPath("//*[@id='botdetect_free_play_captcha']/div[1]/img"));
                    logoSRC = logo.GetAttribute("src");
                    break;
                }
                catch (System.NullReferenceException)
                {
                    Thread.Sleep(500);
                    continue;
                }
            } while (true);//ожидание загрузки изображения капчи
            
            while (!solve_text2(driver, driver.FindElement(By.XPath("//*[@id='botdetect_free_play_captcha']/div[1]/img")), driver.FindElement(By.XPath("//*[@id='botdetect_free_play_captcha']/input[2]"))))
            {
                
                driver.FindElement(By.XPath("//*[@id='botdetect_free_play_captcha']/div[2]/p[3]")).Click();
                Thread.Sleep(1000);

                do
                {
                    try
                    {
                        logo = driver.FindElement(By.XPath("//*[@id='botdetect_free_play_captcha']/div[1]/img"));
                        logoSRC = logo.GetAttribute("src");
                        break;
                    }
                    catch (System.NullReferenceException)
                    {
                        Thread.Sleep(500);
                        continue;
                    }
                } while (true);//ожидание загрузки изображения капчи
                

            }

            while (!solve_text2(driver, driver.FindElement(By.XPath("//*[@id='botdetect_free_play_captcha2']/div[1]/img")), driver.FindElement(By.XPath("//*[@id='botdetect_free_play_captcha2']/input[2]"))))
            {
                driver.FindElement(By.XPath("//*[@id='botdetect_free_play_captcha2']/div[2]/p[3]/i")).Click();
                Thread.Sleep(1000);
                
                do
                {
                    try
                    {
                        logo = driver.FindElement(By.XPath("//*[@id='botdetect_free_play_captcha2']/div[1]/img"));
                        logoSRC = logo.GetAttribute("src");
                        break;
                    }
                    catch (System.NullReferenceException)
                    {
                        Thread.Sleep(500);
                        continue;
                    }
                } while (true);//ожидание загрузки изображения капчи
                

            }
            
            return true;
        }

        public void Rucaptchav2(IWebDriver driver)
        {
            string id;
            int reqcount = 0;

            ((IJavaScriptExecutor)driver).ExecuteScript("window.open()");
            driver.SwitchTo().Window(driver.WindowHandles.Last());
            driver.Navigate().GoToUrl("http://rucaptcha.com/in.php?key=" + textBox1.Text + "&method=userrecaptcha&googlekey=6LeGfGIUAAAAAEyUovGUehv82L-IdNRusaYFEm5b&pageurl=https://freebitco.in/?op=home&here=now");
            id = Convert.ToString((driver.FindElement(By.XPath("/html/body")).Text).Replace("OK|", ""));
            UpdateLog("id капчи: " + id);

            Thread.Sleep(1000);
            driver.Navigate().GoToUrl("http://rucaptcha.com/res.php?key=" + textBox1.Text + "&action=get&id=" + id);

            while (Convert.ToString(driver.FindElement(By.XPath("/html/body")).Text) == "CAPCHA_NOT_READY" && reqcount < 60)
            {
                Thread.Sleep(3000);
                driver.Navigate().GoToUrl("http://rucaptcha.com/res.php?key=" + textBox1.Text + "&action=get&id=" + id);

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
                m_sqlCmd.CommandText = "INSERT INTO Log ('Время', 'RP', 'BTC', 'miss') values ('" + DateTime.Now + "' , '" + RP + "' , '" + BTC + "', '" + miss + "')";

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
                sqlQuery = "SELECT id, akk, prof, proxy FROM Setting";
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqlQuery, m_dbConn);
                adapter.Fill(dTable);

                if (dTable.Rows.Count > 0)
                {
                    
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

        public void UpdateLog2(string s)
        {


            Action action = () =>
            {
                richTextBox2.AppendText(s + "\n");
                richTextBox2.ScrollToCaret();
            };

            Invoke(action);
        }

        public void UpdateLog3(string s)
        {
            Action action = () =>
            {
                
                richTextBox3.AppendText(s + "\n");
                
            };

            Invoke(action);
        }

        public IWebElement FandS(IWebDriver driver, string selector)
        {
            IWebElement element = null;
            try
            {
                if (selector.IndexOf(".") == 0 && IsElementVisible(driver.FindElement(By.CssSelector(selector))))
                {

                    element = driver.FindElement(By.CssSelector(selector));
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].setAttribute('style','display:none;');", driver.FindElement(By.CssSelector(".large-12.fixed")));
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);

                    return element;
                }

            }
            //catch (OpenQA.Selenium.StaleElementReferenceException)
            //{
            //    while (true)
            //    {
            //        try
            //        {
            //            Thread.Sleep(500);
            //            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
            //            return element;
            //        }
            //        catch
            //        {
            //            continue;
            //        }
            //    }
            //}
            catch (NoSuchElementException)
            {
                //UpdateLog2(selector + " не CssSelector");
            }

            try
            {
                if (selector.IndexOf("/") == 0 && IsElementVisible(driver.FindElement(By.XPath(selector))))
                {
                    element = driver.FindElement(By.XPath(selector));
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].setAttribute('style','display:none;');", driver.FindElement(By.CssSelector(".large-12.fixed")));
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);

                    return element;
                }
            }
            catch (NoSuchElementException)
            {
                //UpdateLog2(selector + " не XPath");
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
                    DataUpdate(spred_RP - RP_cost, spred_BTC, miss);
                    //DataGridUpdate();
                }
            };


            Invoke(action);
            RP_bonus_cost = 0;
        }

        public void bonus(IWebDriver driver)
        {

            try
            {
                if (checkBox1.Checked)
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].setAttribute('style','display');", driver.FindElement(By.CssSelector(".large-12.fixed")));

                    


                    driver.FindElement(By.PartialLinkText("REWARDS")).Click();
                    old_RP = Convert.ToInt32(FandS(driver, ".user_reward_points").Text.Replace(",", ""));
                    FandS(driver, "//*[@id='rewards_tab']/div[4]/div/div[6]/div[1]").Click();
                    Thread.Sleep(1000);

                    if (old_RP >= 12 && old_RP < 120) { FandS(driver, "//*[@id='free_points_rewards']/div[5]/div[2]/div[3]/button").Click(); }
                    if (old_RP >= 120 && old_RP < 300) { FandS(driver, "//*[@id='free_points_rewards']/div[4]/div[2]/div[3]/button").Click(); }
                    if (old_RP >= 300 && old_RP < 600) { FandS(driver, "//*[@id='free_points_rewards']/div[3]/div[2]/div[3]/button").Click(); }
                    if (old_RP >= 600 && old_RP < 1200) { FandS(driver, "//*[@id='free_points_rewards']/div[2]/div[2]/div[3]/button").Click(); }
                    if (old_RP >= 1200) { FandS(driver, "//*[@id='free_points_rewards']/div[1]/div[2]/div[3]/button").Click(); RP_bonus_cost = +1200; }//*[@id="free_points_rewards"]/div[1]/div[2]/div[3]/button
                                                                                                                                                       //driver.Navigate().GoToUrl("https://freebitco.in/");

                    //if (old_RP >= 12 && old_RP < 120) { UpdateLog2("Активируем бонус за 12 на счету " + old_RP + "RP"); }
                    //if (old_RP >= 120 && old_RP < 300) { UpdateLog2("Активируем бонус за 120 на счету " + old_RP + "RP"); }
                    //if (old_RP >= 300 && old_RP < 600) { UpdateLog2("Активируем бонус за 300 на счету " + old_RP + "RP"); }
                    //if (old_RP >= 600 && old_RP < 1200) { UpdateLog2("Активируем бонус за 600 на счету " + old_RP + "RP"); }
                    //if (old_RP >= 1200) { UpdateLog2("Активируем бонус за 1200 на счету " + old_RP + "RP"); }


                }

                if (checkBox2.Checked)
                {
                    //driver.FindElement(By.PartialLinkText("REWARDS")).Click();
                    old_RP = Convert.ToInt32(FandS(driver, ".user_reward_points").Text.Replace(",", ""));
                    FandS(driver, "//*[@id='rewards_tab']/div[4]/div/div[4]/div[1]").Click();
                    Thread.Sleep(1000);

                    if (old_RP >= 1520 && old_RP < 2800) { FandS(driver, "//*[@id='fp_bonus_rewards']/div[3]/div[2]/div[3]/button").Click(); }
                    if (old_RP >= 2800 && old_RP < 4400) { FandS(driver, "//*[@id='fp_bonus_rewards']/div[2]/div[2]/div[3]/button").Click(); }
                    if (old_RP >= 4400) { FandS(driver, "//*[@id='fp_bonus_rewards']/div[1]/div[2]/div[3]/button").Click(); }
                    //driver.Navigate().GoToUrl("https://freebitco.in/");

                    //if (old_RP >= 1520 && old_RP < 2800) { UpdateLog2("Активируем бонус за 320 на счету " + old_RP + "RP"); }
                    //if (old_RP >= 2800 && old_RP < 4400) { UpdateLog2("Активируем бонус за 1600 на счету " + old_RP + "RP"); }
                    //if (old_RP >= 4400) { UpdateLog2("Активируем бонус за 3200 на счету " + old_RP + "RP"); }
                }

                

                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].setAttribute('style','display');", driver.FindElement(By.CssSelector(".large-12.fixed")));
                driver.FindElement(By.PartialLinkText("FREE BTC")).Click();
            }
            catch (Exception)
            {
                UpdateLog2("сбой активации бонусов");
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].setAttribute('style','display');", driver.FindElement(By.CssSelector(".large-12.fixed")));
                driver.FindElement(By.PartialLinkText("FREE BTC")).Click();
            }

        }

        public async void Step3(int i)
        {
            await Task.Run(() =>
            {
                multiply3(i);

                options = new ChromeOptions();
                Proxy proxy = new Proxy();
                proxy.Kind = ProxyKind.Manual;
                proxy.IsAutoDetect = false;
                proxy.HttpProxy = data_get_proxy(i);
                proxy.SslProxy = data_get_proxy(i);
                options.Proxy = proxy;
                options.AddArgument("ignore-certificate-errors");
                //options.AddArgument("--headless");
                options.AddArguments(@"user-data-dir=" + Application.StartupPath + @"\" + data_get_prof(i));
                options.AddArguments("--start-maximized");
                driver = new ChromeDriver(options);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
                //driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(120);
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                bool rush = false;

                try
                {
                    UpdateLog2("(" + i + ")Загрузка страницы...");
                    driver.Navigate().GoToUrl("https://freebitco.in/");
                }
                catch (Exception)
                {
                    UpdateLog2("(" + i + ")Страница не загрузилась.");
                    driver.Quit();
                    busy = false;
                    return;
                }

                try
                {
                    driver.FindElement(By.CssSelector(".error-code"));
                    UpdateLog2("(" + i + ")" + driver.FindElement(By.CssSelector(".error-code")).Text);
                    driver.Quit();
                    busy = false;
                    UpdateLog2("(" + i + ")" + "Смена прокси " + data_get_proxy(i));
                    proxy_change(i);
                    UpdateLog2("(" + i + ")" + "Новый прокси " + data_get_proxy(i));
                    return;
                }
                catch (Exception)
                {
                    
                }

                try
                {
                    UpdateLog2("(" + i + ")Поиск кулдауна...");
                    wait.Until(ExpectedConditions.ElementIsVisible(By.Id("time_remaining")));
                    Thread.Sleep(1000);
                    timing_list[i] = Convert.ToInt32(driver.FindElement(By.CssSelector(".countdown_amount")).Text) * 60 + 10;
                    UpdateLog2(timing_list[i] + "");

                    if (timing_list[i] > 2000)
                    {
                        UpdateLog2("(" + i + ")Кулдаун заменен с " + timing_list[i] + " на 2000.");
                        timing_list[i] = 2000;
                    }

                    
                    driver.Quit();
                    busy = false;
                    return;
                }
                catch (Exception)
                {
                    UpdateLog2("(" + i + ")Кулдаун не обнаружен.");
                    
                }
                
                try
                {
                    if (IsElementVisible(driver.FindElement(By.Id("free_play_form_button"))))
                    {
                        UpdateLog2("(" + i + ")Кнопка сбора найдена.");

                        try
                        {
                            if (IsElementVisible(driver.FindElement(By.Id("play_without_captcha_container"))))
                            {
                                
                            }

                        }
                        catch (Exception)
                        {
                            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", driver.FindElement(By.Id("free_play_form_button")));

                            driver.FindElement(By.Id("free_play_form_button")).Click();
                            UpdateLog2("(" + i + ")Сбор без капчи.");
                        }
                    }
                }
                catch (Exception)
                {
                    UpdateLog2("(" + i + ")Кнопка сбора не найдена.");
                    driver.Quit();
                    busy = false;
                    return;
                }

                
                

                try
                {
                    driver.FindElement(By.Id("switch_captchas_button"));
                    UpdateLog2("(" + i + ")Есть текстовая капча.");
                }
                catch (Exception)
                {
                    
                    
                    UpdateLog2("(" + i + ")Текстовая капча недоступна " + i + " поставлена в очередь мультика.");
                    multiply3(i);
                    timing_list[i] = 1000;
                    driver.Quit();
                    busy = false;
                    return;
                }

                

                do
                {
                    if (simple_captcha2(driver))
                    {
                        
                    }
                    else
                    {
                        driver.Quit();
                        busy = false;
                        break;
                    }

                    if (checkBox1.Checked || checkBox2.Checked)
                        bonus(driver);
                    Thread.Sleep(1000);
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", driver.FindElement(By.Id("free_play_form_button")));

                    driver.FindElement(By.Id("free_play_form_button")).Click();

                    try
                    {
                        wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".countdown_amount")));
                        //Thread.Sleep(1000);
                        timing_list[i] = Convert.ToInt32(driver.FindElement(By.CssSelector(".countdown_amount")).Text) * 60 + 10;

                        if (timing_list[i] > 2000)
                        {
                            UpdateLog2("(" + i + ")Кулдаун заменен с " + timing_list[i] + " на 2000.");
                            timing_list[i] = 2000;
                        }

                        

                        
                        driver.Quit();
                        busy = false;
                        return;
                    }
                    catch (Exception)
                    {
                        driver.Navigate().Refresh();

                        try
                        {
                            wait.Until(ExpectedConditions.ElementIsVisible(By.Id("free_play_form_button")));
                        }
                        catch (Exception)
                        {
                        }
                    }
                    


                } while (IsElementVisible(driver.FindElement(By.Id("free_play_form_button"))));


            });

            UpdateLog2("(" + i + ")Stepped пуст.");
            stepped = 777;
            
            try
            {
                driver.Quit();
                busy = false;
                UpdateLog2("(" + i + ")Дно шага.");
            }
            catch (Exception)
            {
            }
        }
        
        public Form1()
        {
            InitializeComponent();
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
            if (textBox5.Text.Length == 0)
            {
                MessageBox.Show("Не выбран аккаунт", "Error", MessageBoxButtons.OK);
                return;
            }


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
            driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(120);
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
            richTextBox3.Clear();
            for (int i = 0; i < timing_list.Count; i++)
                {
                timing_list[i]--;

                    if (timing_list[i] <= 0 && busy == false && multed != i)
                    {
                    busy = true;
                    UpdateLog2("(" + i + ")Stepped = " + i);
                    stepped = i;
                    Step3(i);
                    }

                if (timing_list[i] <= -1000)
                {
                    KillChrome("chrome");
                    KillChrome("chromedriver");
                    screen();
                    this.Close();
                }
            }
            try
            {
                foreach (var item in timing_list)
                {
                    UpdateLog(Convert.ToString(item) + "\t");
                }

                foreach (var item in multiply3_list)
                {
                    UpdateLog3(item.ToString("F8") + "\t");
                }
            }
            catch (Exception)
            {
                
            }

            //if (multiply_list.Count != 0 && multiply_busy == false && stepped != multiply_list[0])
            //{
            //    UpdateLog2("(" + multiply_list[0] + ")Multed = " + multiply_list[0]);
            //    multed = multiply_list[0];
            //    multiply_busy = true;
            //    multiply2(multiply_list[0]);
            //}

            Time -= 1;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            m_dbConn = new SQLiteConnection();
            m_sqlCmd = new SQLiteCommand();

            if (!File.Exists(dbFileName))
                SQLiteConnection.CreateFile(dbFileName);

            try
            {
                m_dbConn = new SQLiteConnection("Data Source=" + dbFileName + ";Version=3;");
                m_dbConn.Open();
                m_sqlCmd.Connection = m_dbConn;

                m_sqlCmd.CommandText = "CREATE TABLE IF NOT EXISTS Log (id INTEGER PRIMARY KEY , Date TEXT, Akk INTEGER, faucet INTEGER, RP INTEGER, BTC INTEGER)"; //AUTOINCREMENT
                m_sqlCmd.ExecuteNonQuery();

                m_sqlCmd.CommandText = "CREATE TABLE IF NOT EXISTS Setting (id INTEGER PRIMARY KEY , akk TEXT, prof TEXT, pass TEXT, proxy TEXT)";
                m_sqlCmd.ExecuteNonQuery();

                m_sqlCmd.CommandText = "CREATE TABLE IF NOT EXISTS Proxy_list (id INTEGER PRIMARY KEY, proxy TEXT, usage BOOL)";
                m_sqlCmd.ExecuteNonQuery();

                m_sqlCmd.CommandText = "CREATE TABLE IF NOT EXISTS Balance (id INTEGER PRIMARY KEY, satoshi INTEGER)";
                m_sqlCmd.ExecuteNonQuery();

                m_sqlCmd.CommandText = "CREATE TABLE IF NOT EXISTS Faucet_num (id INTEGER PRIMARY KEY, Date TEXT, Akk INTEGER, faucet INTEGER)";
                m_sqlCmd.ExecuteNonQuery();

                m_sqlCmd.CommandText = "CREATE TABLE IF NOT EXISTS Cookie_setting (id INTEGER PRIMARY KEY, Cookie TEXT, csrf_token TEXT, u INTEGER, p TEXT)";
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

            
            get_timing_list();

            if (Properties.Settings.Default.autostart)
            {
                checkBox4.Checked = true;
                timer1.Start();
                Go.Text = "Стапэ!";
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int i = 0;
            foreach (var item in timing_list)
            {
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
                driver = new ChromeDriver(options);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(120);
                driver.Navigate().GoToUrl("https://freebitco.in/");

                write_faucet_num(driver, i);
                write_balance(driver, i);
                i++;
                driver.Quit();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            
            //DataGridUpdate();
        }

        public async void button6_Click(object sender, EventArgs e)//тестовая кнопка
        {
            timing_list[0] = -1200;
            
        }


        private void button7_Click(object sender, EventArgs e)//обновление
        {
            DataGridUpdate1();
        }
        
        private void форматСпискаПрофилейToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Setting_format Setting_format = new Setting_format();

            Setting_format.Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //update_proxy_list();

            using (StreamReader sr = new StreamReader(Application.StartupPath + @"\proxy.txt"))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    add_good_proxy(line);
                }
            }
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
            {
                Properties.Settings.Default.autostart = true;
            }
            else
            {
                Properties.Settings.Default.autostart = false;
            }

            Properties.Settings.Default.Save();
        }
    }
}
