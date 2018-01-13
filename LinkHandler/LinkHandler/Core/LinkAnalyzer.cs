using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LinkHandler.Core
{
    class LinkAnalyzer
    {
        private string str = ""; // For null-object

        /// <summary>
        /// Обновляет информацию при получении новых данных
        /// </summary>
        public event Action<string> OnNewData = (str) => { };


        /// <summary>
        /// Загружает и анализирует Html
        /// </summary>
        /// <param name="link"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public bool HandleHtml(string link, string pattern)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(link);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            bool isSucceseded = false;
            string data = "";

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null) readStream = new StreamReader(receiveStream);
                else readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));

                data = readStream.ReadToEnd();

                response.Close();
                readStream.Close();
                isSucceseded = true;
                OnNewData.Invoke(link + " downloaded");
                HtmlAnalysis(link, data, pattern);
            }
            else OnNewData.Invoke(link + " fail to download");
            return isSucceseded;
        }

        /// <summary>
        /// Асинхронно загружает и анализирует Html
        /// </summary>
        /// <param name="link"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public async Task<bool> HandleHtmlAsync(string link, string pattern)
        {
            bool isSucceseded = await Task.Run(() => HandleHtml(link, pattern));
            return isSucceseded;
        }

        private void HtmlAnalysis(string link, string htmlSource, string pattern)
        {
            // TODO Some useful analysis logic. This is just example.
            int index = htmlSource.IndexOf(pattern);
            OnNewData($"{link} contains {pattern} at index {index}");
        }
    }
}
