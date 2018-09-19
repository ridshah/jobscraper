using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using HtmlAgilityPack;
using System.Net.Http;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support;
using System.IO;

namespace web_scraping
{
    class Program
    {
        protected static string google_jobs_xpath = "//*[@id='immersive_desktop_root']/div/div[3]/div[1]";
        protected static string indeed_xpath = "//td[@id='resultsCol']/div/*/a[@data-tn-element='jobTitle']";
        protected static string glassdoor_xpath = "//div[@id='JobResults']/section/article/div/ul/li/div/div[@class='flexbox']/*/a[@class='jobLink']";
        protected static string monster_xpath = "//div[@id='SearchResults']/section/*/*/*/h2[@class='title']/a";
        protected static string keyword = "";
        protected static List<string> linklist = new List<string>();

        static void Main(string[] args)
        {
            Console.Write("Enter job portal to search from below option:\n1. Google\n2. Indeed\n3. Glassdoor\n4. Monster\n5. All\n");
            string jobportal = Console.ReadLine();
            Console.Write("Enter your keyword:\n");
            keyword = Console.ReadLine();
            IWebDriver driver = new ChromeDriver(System.IO.Directory.GetCurrentDirectory());
            if (jobportal.Trim().ToLower() == "google" || jobportal.Trim().ToLower() == "1")
            {
                Console.WriteLine("You have chosen Google. Please wait while we get you the links...");
                GooleJobs(driver);
                driver.Close();
                driver.Quit();
            }
            else if (jobportal.Trim().ToLower() == "indeed" || jobportal.Trim().ToLower() == "2")
            {
                Console.WriteLine("You have chosen Indeed. Please wait while we get you the links...");
                IndeedJobs(driver);
                driver.Close();
                driver.Quit();
            }
            else if (jobportal.Trim().ToLower() == "glassdoor" || jobportal.Trim().ToLower() == "3")
            {
                Console.WriteLine("You have chosen Glassdoor. Please wait while we get you the links...");
                GlassdoorJobs(driver);
                driver.Close();
                driver.Quit();
            }
            else if (jobportal.Trim().ToLower() == "monster" || jobportal.Trim().ToLower() == "4")
            {
                Console.WriteLine("You have chosen Monster. Please wait while we get you the links...");
                MonsterJobs(driver);
                driver.Close();
                driver.Quit();
            }
            else if (jobportal.Trim().ToLower() == "all" || jobportal.Trim().ToLower() == "5")
            {
                Console.WriteLine("You have chosen all. Please wait while we get you the links...");
                GooleJobs(driver);
                IndeedJobs(driver);
                GlassdoorJobs(driver);
                MonsterJobs(driver);
                driver.Close();
                driver.Quit();
            }
            else
            {
                Console.WriteLine("Sorry this portal is yet not added.");
                driver.Close();
                driver.Quit();
                Thread.Sleep(1000);
                Environment.Exit(0);
            }
        }

        private static void MonsterJobs(IWebDriver driver)
        {
            try
            {
                string url = "https://www.monster.com/jobs/search/?q=" + keyword;
                driver.Url = url;
                string str = driver.PageSource;
                HtmlAgilityPack.HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(str);
                foreach (HtmlNode row in doc.DocumentNode.SelectNodes(monster_xpath))
                {
                    string linkurl = WebUtility.UrlDecode(row.Attributes["href"].Value);
                    linklist.Add(linkurl);
                }
                File.WriteAllLines(keyword + "monsterlinks.csv", linklist.Select(x => string.Join("\n", x))); 
            }
            catch (Exception ex)
            {
                Console.WriteLine("Oops, Something went wrong!\nDetails:" + ex.Message);
                driver.Quit();
                Thread.Sleep(1000);
                Environment.Exit(0);
            }
        }

        private static void GlassdoorJobs(IWebDriver driver)
        {
            try
            {
                string url = "https://www.glassdoor.com/Job/jobs.htm?typedKeyword=non profit " + keyword + "&sc.keyword=non profit " + keyword;
                driver.Url = url;
                string str = driver.PageSource;
                HtmlAgilityPack.HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(str);
                foreach (HtmlNode row in doc.DocumentNode.SelectNodes(glassdoor_xpath))
                {
                    string linkurl = WebUtility.UrlDecode(row.Attributes["href"].Value);
                    string pat = @"jobListingId.*";
                    Regex r = new Regex(pat, RegexOptions.IgnoreCase);
                    Match m = r.Match(linkurl);
                    linklist.Add("https://www.glassdoor.com/partner/jobListing.htm?" + m.Value);
                }
                File.WriteAllLines(keyword + "glassdoorlinks.csv", linklist.Select(x => string.Join("\n", x))); 
            }
            catch (Exception ex)
            {
                Console.WriteLine("Oops, Something went wrong!\nDetails:" + ex.Message);
                driver.Quit();
                Thread.Sleep(1000);
                Environment.Exit(0);
            }
        }

        private static void IndeedJobs(IWebDriver driver)
        {
            try
            {
                string url = "https://www.indeed.com/jobs?q=" + keyword + "&l=United+States";
                driver.Url = url;
                string str = driver.PageSource;
                HtmlAgilityPack.HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(str);
                foreach (HtmlNode row in doc.DocumentNode.SelectNodes(indeed_xpath))
                {
                    string linkurl = "https://www.indeed.com" + WebUtility.UrlDecode(row.Attributes["href"].Value);
                    linklist.Add(linkurl);
                }
                File.WriteAllLines(keyword + "indeedlinks.csv", linklist.Select(x => string.Join("\n", x))); 
            }
            catch (Exception ex)
            {
                Console.WriteLine("Oops, Something went wrong!\nDetails:" + ex.Message);
                driver.Quit();
                Thread.Sleep(1000);
                Environment.Exit(0);
            }
        }

        private static void GooleJobs(IWebDriver driver)
        {
            try 
            {
                string url = "https://www.google.com/search?q=jobs:" + keyword + "&oq=jobs:" + keyword + "&ibp=htl;jobs&sa=X";
                driver.Url = url;
                IWebElement ele = driver.FindElement(By.XPath(google_jobs_xpath));
                ele.Click();
                OpenQA.Selenium.Interactions.Actions action = new OpenQA.Selenium.Interactions.Actions(driver);
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                String s = String.Format("document.evaluate(\"" + google_jobs_xpath + "\", document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue.scrollTop+=6000;");
                js.ExecuteScript(s);
                Thread.Sleep(4000);
                s = String.Format("document.evaluate(\"" + google_jobs_xpath + "\", document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue.scrollTop+=6000;");
                js.ExecuteScript(s);
                Thread.Sleep(4000);
                string str = driver.PageSource;
                HtmlAgilityPack.HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(str);

                foreach (HtmlNode row in doc.DocumentNode.SelectNodes("//*[@id]/div[2]/div[2]/div/div/a"))
                {
                    string linkurl = WebUtility.UrlDecode(row.Attributes["href"].Value);
                    string pat = @"https.*";
                    Regex r = new Regex(pat, RegexOptions.IgnoreCase);
                    Match m = r.Match(linkurl);
                    linklist.Add(m.Value);
                }
                File.WriteAllLines(keyword + "googlelinks.csv", linklist.Select(x => string.Join("\n", x))); 
            }
            catch (Exception ex)
            {
                Console.WriteLine("Oops, Something went wrong!\nDetails:" + ex.Message);
                driver.Quit();
                Thread.Sleep(1000);
                Environment.Exit(0);
            } 
        }
    }
}
