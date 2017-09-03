using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using CoreLib;

/// <summary>
/// 樂透抓取器
/// </summary>
namespace LottoSpider
{
    class Program
    {

        public static  string CN;

        static void Main(string[] args)
        {

            //初始化參數
            initPara();

            //抓取資料
            getData();

            //正規化過濾資料
            filterData();

            //將資料匯入DB
            importData();

            //發送結果email

            sendResultMail();

        }


        /// <summary>
        /// 初始各字串
        /// </summary>
        private static void  initPara() {

            CN = Config.getConnectionString("CN");
            

        }

        private static void getData() {

        }

        private static void filterData() {

        }

        private static void importData() {

        }

        /// <summary>
        /// 發送結果email
        /// </summary>
        private static void sendResultMail() {

        }

    }
}
