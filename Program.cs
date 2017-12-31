using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Specialized;


using MySql.Data.MySqlClient;
using System.Data;
using CoreLib;

/// <summary>
/// 樂透抓取器
/// </summary>
namespace LottoSpider
{
    class Program
    {

        public static  string LOTTO649_URL,DAILYCASH_URL,SUPERLOTTO638_URL;

        static void Main(string[] args)
        {

            try {

                

                //初始化參數
                initPara();

                //抓取資料
                getData();
            }
            catch (Exception ex) {

                logError(ex.Message);
                sendMail("抓取失敗", "錯誤原因：" + ex.ToString());

            }
         

        }



        private static void logError(string errMsg) {

                      
            string sql = "INSERT INTO D_LOG(CREATE_DATE,INFO_TYPE,MEMO) " +
                    " VALUE(@CREATE_DATE,@INFO_TYPE,@MEMO)";
            MySqlCommand cmd = new MySql.Data.MySqlClient.MySqlCommand(sql);
            cmd.Parameters.Add("@CREATE_DATE", MySqlDbType.VarChar).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            cmd.Parameters.Add("@INFO_TYPE", MySqlDbType.VarChar).Value = "ERR";
            cmd.Parameters.Add("@MEMO", MySqlDbType.VarChar).Value = errMsg;

            Mariadb md = new Mariadb(getConnectionString());
            md.ExecuteNonQuery(cmd);


        }


        private static string getConnectionString() {
            return Config.getConnectionString("CN"); 
        }

        /// <summary>
        /// 初始各字串
        /// </summary>
        private static void  initPara() {

            //威力彩
            SUPERLOTTO638_URL = "http://www.taiwanlottery.com.tw/lotto/superlotto638/history.aspx";
            //今彩539
            DAILYCASH_URL = "http://www.taiwanlottery.com.tw/lotto/DailyCash/history.aspx";
            //大樂透
            LOTTO649_URL = "http://www.taiwanlottery.com.tw/lotto/Lotto649/history.aspx";
            
        }



        #region 取得獎號

        private static void getData()
        {

            getLotto649Data();
            getDailyCashData();
            getSuperLotto638();

        }

        /// <summary>
        /// 大樂透
        /// </summary>
        private static void getLotto649Data()
        {

            string content = "";
            if (downloadContent(LOTTO649_URL, out content) == true)
            {
                //過濾網頁資料
                //Regex regex = new Regex("<span[^>]*>(.*?)</span>");
                //string value = "D539Control_history1_dlQuery_SNo2_2";

                string PERIOD_TAG, PERIOD, LOTTERYDATE_TAG, LOTTERYDATE, NUM1_TAG, NUM1, NUM2_TAG, NUM2, NUM3_TAG, NUM3, NUM4_TAG, NUM4, NUM5_TAG, NUM5,NUM6_TAG,NUM6,ESP_NUM_TAG,ESP_NUM;

                for (var i = 0; i <= 9; i++)
                {
                    PERIOD_TAG = "Lotto649Control_history_dlQuery_L649_DrawTerm_" + i.ToString();
                    LOTTERYDATE_TAG = "Lotto649Control_history_dlQuery_L649_DDate_" + i.ToString();
                    NUM1_TAG = "Lotto649Control_history_dlQuery_SNo1_" + i.ToString();
                    NUM2_TAG = "Lotto649Control_history_dlQuery_SNo2_" + i.ToString();
                    NUM3_TAG = "Lotto649Control_history_dlQuery_SNo3_" + i.ToString();
                    NUM4_TAG = "Lotto649Control_history_dlQuery_SNo4_" + i.ToString();
                    NUM5_TAG = "Lotto649Control_history_dlQuery_SNo5_" + i.ToString();
                    NUM6_TAG = "Lotto649Control_history_dlQuery_SNo6_" + i.ToString();
                    ESP_NUM_TAG = "Lotto649Control_history_dlQuery_No7_" + i.ToString();

                    PERIOD = getValue(PERIOD_TAG, content);
                    LOTTERYDATE = getValue(LOTTERYDATE_TAG, content);
                    NUM1 = getValue(NUM1_TAG, content);
                    NUM2 = getValue(NUM2_TAG, content);
                    NUM3 = getValue(NUM3_TAG, content);
                    NUM4 = getValue(NUM4_TAG, content);
                    NUM5 = getValue(NUM5_TAG, content);
                    NUM6 = getValue(NUM6_TAG, content);

                    ESP_NUM= getValue(ESP_NUM_TAG, content);

                    //將資料倒入DB
                    import649Data(PERIOD, LOTTERYDATE, NUM1, NUM2, NUM3, NUM4, NUM5,NUM6,ESP_NUM);


                }





            }
        }

        /// <summary>
        /// 今彩539
        /// </summary>
        private static void getDailyCashData()
        {

            string content = "";
            if (downloadContent(DAILYCASH_URL, out content) == true) {
                //過濾網頁資料


                //Regex regex = new Regex("<span[^>]*>(.*?)</span>");
                //string value = "D539Control_history1_dlQuery_SNo2_2";

                string PERIOD_TAG, PERIOD, LOTTERYDATE_TAG, LOTTERYDATE, NUM1_TAG, NUM1, NUM2_TAG, NUM2, NUM3_TAG, NUM3, NUM4_TAG, NUM4, NUM5_TAG, NUM5;

                for (var i = 0; i <= 9; i++) {
                    PERIOD_TAG = "D539Control_history1_dlQuery_D539_DrawTerm_" + i.ToString();
                    LOTTERYDATE_TAG = "D539Control_history1_dlQuery_D539_DDate_" + i.ToString();
                    NUM1_TAG = "D539Control_history1_dlQuery_No1_" + i.ToString();
                    NUM2_TAG = "D539Control_history1_dlQuery_No2_" + i.ToString();
                    NUM3_TAG = "D539Control_history1_dlQuery_No3_" + i.ToString();
                    NUM4_TAG = "D539Control_history1_dlQuery_No4_" + i.ToString();
                    NUM5_TAG = "D539Control_history1_dlQuery_No5_" + i.ToString();

                    PERIOD = getValue(PERIOD_TAG, content);
                    LOTTERYDATE= getValue(LOTTERYDATE_TAG, content);
                    NUM1 = getValue(NUM1_TAG, content);
                    NUM2 = getValue(NUM2_TAG, content);
                    NUM3 = getValue(NUM3_TAG, content);
                    NUM4 = getValue(NUM4_TAG, content);
                    NUM5 = getValue(NUM5_TAG, content);


                    //將資料倒入DB
                    import539Data(PERIOD, LOTTERYDATE, NUM1, NUM2, NUM3, NUM4, NUM5);


                }


                

              
            }

        }

        /// <summary>
        /// 取得正規化後的資料
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        private static string getValue(string tag,string content) {

            string result="";
            Regex regexSpan = new Regex("<span id=\"" + tag  + "\">(.*?)</span>", RegexOptions.Compiled | RegexOptions.IgnoreCase);


            if (regexSpan.IsMatch(content))
            {
                MatchCollection collection = regexSpan.Matches(content);
                foreach (Match m in collection)
                {
                    result = m.Groups[1].Value;
                    return result;
                }
            }


            return result;
        }

        /// <summary>
        /// 威力彩
        /// </summary>
        private static void getSuperLotto638()
        {
            string content = "";
            if (downloadContent(SUPERLOTTO638_URL, out content) == true)
            {
                //過濾網頁資料
                //Regex regex = new Regex("<span[^>]*>(.*?)</span>");
                //string value = "D539Control_history1_dlQuery_SNo2_2";

                string PERIOD_TAG, PERIOD, LOTTERYDATE_TAG, LOTTERYDATE, NUM1_TAG, NUM1, NUM2_TAG, NUM2, NUM3_TAG, NUM3, NUM4_TAG, NUM4, NUM5_TAG, NUM5, NUM6_TAG, NUM6, ESP_NUM_TAG, ESP_NUM;

                for (var i = 0; i <= 9; i++)
                {
                    PERIOD_TAG = "SuperLotto638Control_history1_dlQuery_DrawTerm_" + i.ToString();
                    LOTTERYDATE_TAG = "SuperLotto638Control_history1_dlQuery_Date_" + i.ToString();
                    NUM1_TAG = "SuperLotto638Control_history1_dlQuery_SNo1_" + i.ToString();
                    NUM2_TAG = "SuperLotto638Control_history1_dlQuery_SNo2_" + i.ToString();
                    NUM3_TAG = "SuperLotto638Control_history1_dlQuery_SNo3_" + i.ToString();
                    NUM4_TAG = "SuperLotto638Control_history1_dlQuery_SNo4_" + i.ToString();
                    NUM5_TAG = "SuperLotto638Control_history1_dlQuery_SNo5_" + i.ToString();
                    NUM6_TAG = "SuperLotto638Control_history1_dlQuery_SNo6_" + i.ToString();
                    ESP_NUM_TAG = "SuperLotto638Control_history1_dlQuery_SNo7_" + i.ToString();

                    PERIOD = getValue(PERIOD_TAG, content);
                    LOTTERYDATE = getValue(LOTTERYDATE_TAG, content);
                    NUM1 = getValue(NUM1_TAG, content);
                    NUM2 = getValue(NUM2_TAG, content);
                    NUM3 = getValue(NUM3_TAG, content);
                    NUM4 = getValue(NUM4_TAG, content);
                    NUM5 = getValue(NUM5_TAG, content);
                    NUM6 = getValue(NUM6_TAG, content);

                    ESP_NUM = getValue(ESP_NUM_TAG, content);

                    //將資料倒入DB
                    importSuperLottoData(PERIOD, LOTTERYDATE, NUM1, NUM2, NUM3, NUM4, NUM5, NUM6, ESP_NUM);


                }





            }
        }

        #endregion


   
        /// <summary>
        /// 匯入DB
        /// </summary>
        private static void import539Data(string period , string date , string num1,string num2,string num3,string num4,string num5) {

            //先檢查資料是否存在
            Mariadb md = new Mariadb(getConnectionString());
            string sql = "SELECT PERIOD FROM PRIZE539 WHERE PERIOD=@PERIOD";
            MySqlCommand cmd = new MySqlCommand(sql);
            cmd.Parameters.Add("@PERIOD", MySqlDbType.VarChar).Value = period;
            DataSet ds = md.GetDataset(cmd);

            if (ds.Tables[0].Rows.Count == 0) {

                //開始新增
                sql = "INSERT INTO PRIZE539(PRIZE_UID,CREATE_DATE,PERIOD,LOTTERY_DAY,NUM1,NUM2,NUM3,NUM4,NUM5)" +
                    " VALUES(@PRIZE_UID,@CREATE_DATE,@PERIOD,@LOTTERY_DAY,@NUM1,@NUM2,@NUM3,@NUM4,@NUM5)";
                cmd = new MySqlCommand(sql);
                cmd.Parameters.Add("@PRIZE_UID", MySqlDbType.VarChar).Value =Guid.NewGuid().ToString();
                cmd.Parameters.Add("@CREATE_DATE", MySqlDbType.VarChar).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                cmd.Parameters.Add("@PERIOD", MySqlDbType.VarChar).Value = period;
                cmd.Parameters.Add("@LOTTERY_DAY", MySqlDbType.VarChar).Value = date;
                cmd.Parameters.Add("@NUM1", MySqlDbType.VarChar).Value = num1;
                cmd.Parameters.Add("@NUM2", MySqlDbType.VarChar).Value = num2;
                cmd.Parameters.Add("@NUM3", MySqlDbType.VarChar).Value = num3;
                cmd.Parameters.Add("@NUM4", MySqlDbType.VarChar).Value = num4;
                cmd.Parameters.Add("@NUM5", MySqlDbType.VarChar).Value = num5;
               

                md.ExecuteNonQuery(cmd);

                //寄送處理的信件
                string subject = "今彩539第" + period + "期號碼抓取完成";
                string body = "本次新增期數" + period + "\r\n" + "號碼：" + num1  + "," + num2 + "," + num3 + "," + num4 + "," + num5;
                sendMail(subject,body);
            }

        }

        private static void import649Data(string period, string date, string num1, string num2, string num3, string num4, string num5,string num6,string espnum)
        {

            //先檢查資料是否存在
            Mariadb md = new Mariadb(getConnectionString());
            string sql = "SELECT PERIOD FROM LOTTO649 WHERE PERIOD=@PERIOD";
            MySqlCommand cmd = new MySqlCommand(sql);
            cmd.Parameters.Add("@PERIOD", MySqlDbType.VarChar).Value = period;
            DataSet ds = md.GetDataset(cmd);

            if (ds.Tables[0].Rows.Count == 0)
            {

                //開始新增
                sql = "INSERT INTO LOTTO649(LOTTO_UID,CREATE_DATE,PERIOD,LOTTERY_DATE,NUM1,NUM2,NUM3,NUM4,NUM5,NUM6,ESP_NUM)" +
                    " VALUES(@LOTTO_UID,@CREATE_DATE,@PERIOD,@LOTTERY_DATE,@NUM1,@NUM2,@NUM3,@NUM4,@NUM5,@NUM6,@ESP_NUM)";
                cmd = new MySqlCommand(sql);
                cmd.Parameters.Add("@LOTTO_UID", MySqlDbType.VarChar).Value = Guid.NewGuid().ToString();
                cmd.Parameters.Add("@CREATE_DATE", MySqlDbType.VarChar).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                cmd.Parameters.Add("@PERIOD", MySqlDbType.VarChar).Value = period;
                cmd.Parameters.Add("@LOTTERY_DATE", MySqlDbType.VarChar).Value = date;
                cmd.Parameters.Add("@NUM1", MySqlDbType.VarChar).Value = num1;
                cmd.Parameters.Add("@NUM2", MySqlDbType.VarChar).Value = num2;
                cmd.Parameters.Add("@NUM3", MySqlDbType.VarChar).Value = num3;
                cmd.Parameters.Add("@NUM4", MySqlDbType.VarChar).Value = num4;
                cmd.Parameters.Add("@NUM5", MySqlDbType.VarChar).Value = num5;
                cmd.Parameters.Add("@NUM6", MySqlDbType.VarChar).Value = num6;
                cmd.Parameters.Add("@ESP_NUM", MySqlDbType.VarChar).Value = espnum ;

                md.ExecuteNonQuery(cmd);

                //寄送處理的信件
                string subject = "大樂透號碼抓取完成";
                string body = "本次新增期數" + period + "\r\n" + "號碼：" + num1 + "," + num2 + "," + num3 + "," + num4 + "," + num5+"," + num6 + ",特別號"+ espnum;
                sendMail(subject, body);
            }

        }


        private static void importSuperLottoData(string period, string date, string num1, string num2, string num3, string num4, string num5, string num6, string espnum)
        {

            //先檢查資料是否存在
            Mariadb md = new Mariadb(getConnectionString());
            string sql = "SELECT PERIOD FROM SUPER_LOTTO WHERE PERIOD=@PERIOD";
            MySqlCommand cmd = new MySqlCommand(sql);
            cmd.Parameters.Add("@PERIOD", MySqlDbType.VarChar).Value = period;
            DataSet ds = md.GetDataset(cmd);

            if (ds.Tables[0].Rows.Count == 0)
            {

                //開始新增
                sql = "INSERT INTO SUPER_LOTTO(LOTTO_UID,CREATE_DATE,PERIOD,LOTTERY_DATE,NUM1,NUM2,NUM3,NUM4,NUM5,NUM6,ESP_NUM)" +
                    " VALUES(@LOTTO_UID,@CREATE_DATE,@PERIOD,@LOTTERY_DATE,@NUM1,@NUM2,@NUM3,@NUM4,@NUM5,@NUM6,@ESP_NUM)";
                cmd = new MySqlCommand(sql);
                cmd.Parameters.Add("@LOTTO_UID", MySqlDbType.VarChar).Value = Guid.NewGuid().ToString();
                cmd.Parameters.Add("@CREATE_DATE", MySqlDbType.VarChar).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                cmd.Parameters.Add("@PERIOD", MySqlDbType.VarChar).Value = period;
                cmd.Parameters.Add("@LOTTERY_DATE", MySqlDbType.VarChar).Value = date;
                cmd.Parameters.Add("@NUM1", MySqlDbType.VarChar).Value = num1;
                cmd.Parameters.Add("@NUM2", MySqlDbType.VarChar).Value = num2;
                cmd.Parameters.Add("@NUM3", MySqlDbType.VarChar).Value = num3;
                cmd.Parameters.Add("@NUM4", MySqlDbType.VarChar).Value = num4;
                cmd.Parameters.Add("@NUM5", MySqlDbType.VarChar).Value = num5;
                cmd.Parameters.Add("@NUM6", MySqlDbType.VarChar).Value = num6;
                cmd.Parameters.Add("@ESP_NUM", MySqlDbType.VarChar).Value = espnum;

                md.ExecuteNonQuery(cmd);

                //寄送處理的信件
                string subject = "威力彩號碼抓取完成";
                string body = "本次新增期數" + period + "\r\n" + "號碼：" + num1 + "," + num2 + "," + num3 + "," + num4 + "," + num5 + "," + num6 + ",特別號" + espnum;
                sendMail(subject, body);
            }

        }


        /// <summary>
        /// 發送結果email
        /// </summary>
        private static void sendMail(string subject , string body) {

            MailClient mc = new MailClient("smtp.gmail.com", 587, "nsking365@gmail.com", "vvtfbyuq0416");
            MailContent c = new MailContent();
            c.From = "nsking365@gmail.com";
            c.FromDisplayName = "樂透號碼抓取通知";
            c.To = "chienlung1977@gmail.com";
            c.Subject =subject;
            c.Body = body;
            c.EnableSsl = true;
            mc.Send(c);


        }



        #region 呼叫函式

        public static  Boolean  downloadContent(string url ,out string responseContent)
        {

          
            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                req.Method = "GET";
                req.Timeout = 120000;
 
                var response = (HttpWebResponse)req.GetResponse();

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

               // System.IO.File.AppendAllText(DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt", responseString);

                responseContent= responseString;

                return true;


            }
            catch (Exception ex)
            {
                //System.IO.File.AppendAllText(DateTime.Now.ToString("yyyyMMddHHmmss") + "_ERR" + ".txt", ex.ToString());
                responseContent = "";
                return false;
            }

           
        }


        #endregion

    }
}
