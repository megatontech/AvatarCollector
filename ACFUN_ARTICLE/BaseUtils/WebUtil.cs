using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace Article.baseUtils
{
    /// <summary>
    /// 实现网站操作
    /// </summary>
    public class WebUtil
    {
        #region - 属性 -

        public CookieContainer cookiec = new CookieContainer();

        /// <summary>
        /// 网站Cookies
        /// </summary>
        private string _cookieHeader = string.Empty;

        /// <summary>
        /// cookie管理
        /// </summary>
        private CookieContainer cookiecontainer;

        public string CookieHeader
        {
            get
            {
                return _cookieHeader;
            }
            set
            {
                _cookieHeader = value;
            }
        }

        private string ArticlePath = "";

        /// <summary>
        /// 用户信息
        /// </summary>
        private string userCardUrl = "http://www.acfun.tv/usercard.aspx?username=";

        #endregion - 属性 -

        #region - 基础方法 -

        /// <summary>
        /// 功能描述：在PostLogin成功登录后记录下Headers中的cookie，然后获取此网站上其他页面的内容
        /// </summary>
        /// <param name="strURL">获取网站的某页面的地址</param>
        /// <param name="strReferer">引用的地址</param>
        /// <returns>返回页面内容</returns>
        public string GetPage(string strURL, string strReferer, CookieContainer cookiec)
        {
            string strResult = "";
            try
            {
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(strURL);
                myHttpWebRequest.AllowAutoRedirect = true;
                myHttpWebRequest.KeepAlive = true;
                myHttpWebRequest.Accept = "*/*";
                myHttpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate ");
                myHttpWebRequest.CookieContainer = cookiec;
                myHttpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; Maxthon; .NET CLR 2.0.50727)";
                myHttpWebRequest.ContentType = "application/json;charset=UTF-8";
                myHttpWebRequest.Method = "GET";
                myHttpWebRequest.Timeout = 300000;
                HttpWebResponse response = null;
                System.IO.StreamReader sr = null;
                BinaryReader br = null;
                response = (HttpWebResponse)myHttpWebRequest.GetResponse();
                if (cookiec != null)
                {
                    response.Cookies = cookiec.GetCookies(myHttpWebRequest.RequestUri);
                }
                Stream streamReceive;
                string gzip = response.ContentEncoding;
                if (string.IsNullOrEmpty(gzip) || gzip.ToLower() != "gzip")
                {
                    streamReceive = response.GetResponseStream();
                }
                else
                {
                    streamReceive = new System.IO.Compression.GZipStream(response.GetResponseStream(), System.IO.Compression.CompressionMode.Decompress);
                }
                sr = new System.IO.StreamReader(streamReceive, Encoding.UTF8);
                br = new System.IO.BinaryReader(streamReceive, Encoding.UTF8);
                //if (response.ContentLength > 1)
                //{
                strResult = sr.ReadToEnd();
                //}
                //else
                //{
                //    char[] buffer = new char[64];
                //    int count = 0;
                //    StringBuilder sb = new StringBuilder();
                //    while ((count = br.Read(buffer, 0, buffer.Length)) > 0)
                //    {
                //        sb.Append(new string(buffer));
                //    }
                //    strResult = sb.ToString();
                //}
                sr.Close();
                response.Close();
            }
            catch (Exception e)
            {
                //throw;
            }
            finally
            {
                Console.WriteLine("!!!!!!!!!");
            }
            return strResult;
        }

        /// <summary>
        /// 登录获得post请求后响应的数据
        /// </summary>
        /// <param name="postUrl">请求地址</param>
        /// <param name="referUrl">请求引用地址</param>
        /// <param name="data">请求带的数据</param>
        /// <returns>响应内容</returns>
        public string PostData(string postUrl, string referUrl, string data)
        {
            string result = "";
            try
            {
                //命名空间System.Net下的HttpWebRequest类
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(postUrl);
                //参照浏览器的请求报文 封装需要的参数 这里参照ie9
                //浏览器可接受的MIME类型
                request.Accept = "text/plain, */*; q=0.01";
                //包含一个URL，用户从该URL代表的页面出发访问当前请求的页面
                request.Referer = referUrl;
                //浏览器类型，如果Servlet返回的内容与浏览器类型有关则该值非常有用
                request.UserAgent = "ShengDiYaGe/1.0 (compatible; MSIE 7.0; Windows NT 6.1; Trident/5.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; .NET4.0C; .NET4.0E)";
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                //请求方式
                request.Method = "POST";
                request.Timeout = 20000;
                //是否保持常连接
                request.KeepAlive = true;
                request.Headers.Add("Accept-Encoding", "gzip, deflate");
                //表示请求消息正文的长度
                request.ContentLength = data.Length;
                Stream postStream = request.GetRequestStream();
                byte[] postData = Encoding.UTF8.GetBytes(data);
                //将传输的数据，请求正文写入请求流
                postStream.Write(postData, 0, postData.Length);
                postStream.Dispose();
                //写入cookie
                request.CookieContainer = cookiec;
                request.Headers.Add("cookie:" + this.CookieHeader);
                request.CookieContainer.SetCookies(new Uri(postUrl), this.CookieHeader);
                //响应
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                response.Cookies = cookiec.GetCookies(request.RequestUri);
                Stream streamReceive;
                string gzip = response.ContentEncoding;
                if (string.IsNullOrEmpty(gzip) || gzip.ToLower() != "gzip")
                {
                    streamReceive = response.GetResponseStream();
                }
                else
                {
                    streamReceive = new System.IO.Compression.GZipStream(response.GetResponseStream(), System.IO.Compression.CompressionMode.Decompress);
                }
                StreamReader sr = new System.IO.StreamReader(streamReceive, Encoding.UTF8);

                if (response.ContentLength > 1)
                {
                    result = sr.ReadToEnd();
                }
                else
                {
                    char[] buffer = new char[256];
                    int count = 0;
                    StringBuilder sb = new StringBuilder();
                    while ((count = sr.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        sb.Append(new string(buffer));
                    }
                    result = sb.ToString();
                }
                sr.Close();
                response.Close();
                return result;
            }
            catch (Exception)
            {
            }
            finally
            {
                Console.WriteLine("!!!!!!!!!");
            }
            return result;
        }

        /// <summary>
        /// 已登录持有cookie进行post
        /// </summary>
        /// <param name="postUrl"></param>
        /// <param name="referUrl"></param>
        /// <param name="data"></param>
        /// <param name="cookiec"></param>
        /// <returns></returns>
        public string PostData(string postUrl, string referUrl, string data, CookieContainer cookiec)
        {
            //data = System.Web.HttpUtility.UrlEncode(data);
            string result = "";
            try
            {
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(postUrl);
                myHttpWebRequest.AllowAutoRedirect = true;
                myHttpWebRequest.KeepAlive = false;
                myHttpWebRequest.Accept = "*/*";
                myHttpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate");
                myHttpWebRequest.CookieContainer = cookiec;
                myHttpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; Maxthon; .NET CLR 2.0.50727)";
                myHttpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                myHttpWebRequest.Referer = referUrl;
                myHttpWebRequest.Method = "POST";
                myHttpWebRequest.Timeout = 80000;
                myHttpWebRequest.ContentLength = data.Length;
                Stream postStream = myHttpWebRequest.GetRequestStream();
                byte[] postData = Encoding.UTF8.GetBytes(data);
                postStream.Write(postData, 0, postData.Length);
                postStream.Dispose();
                HttpWebResponse response = (HttpWebResponse)myHttpWebRequest.GetResponse();
                response.Cookies = cookiec.GetCookies(myHttpWebRequest.RequestUri);
                Stream streamReceive;
                string gzip = response.ContentEncoding;
                if (string.IsNullOrEmpty(gzip) || gzip.ToLower() != "gzip")
                {
                    streamReceive = response.GetResponseStream();
                }
                else
                {
                    streamReceive = new System.IO.Compression.GZipStream(response.GetResponseStream(), System.IO.Compression.CompressionMode.Decompress);
                }
                StreamReader sr = new System.IO.StreamReader(streamReceive, Encoding.UTF8);
                if (response.ContentLength > 1)
                {
                    result = sr.ReadToEnd();
                }
                else
                {
                    char[] buffer = new char[256];
                    int count = 0;
                    StringBuilder sb = new StringBuilder();
                    while ((count = sr.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        sb.Append(new string(buffer));
                    }
                    result = sb.ToString();
                }
                sr.Close();
                response.Close();
                string s = result;
                return s;
            }
            catch { }
            finally
            {
                Console.WriteLine("!!!!!!!!!");
            }
            return "";
        }

        /// <summary>
        /// url 解码
        /// </summary>
        /// <param name="str"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public string MyUrlDeCode(string str, Encoding encoding)
        {
            return HttpUtility.UrlDecode(str, encoding);
        }

        /// <summary>
        /// url 解码
        /// </summary>
        /// <param name="str"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public string MyUrlEnCode(string str, Encoding encoding)
        {
            return HttpUtility.UrlEncode(str, encoding);
        }

        /// <summary>
        /// 获取服务器图片
        /// </summary>
        /// <param name="imageUrl"></param>
        /// <returns></returns>
        public Bitmap getImage(string imageUrl)
        {
            string errorString = "";
            Bitmap b = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(imageUrl);
                request.Timeout = 88000;
                request.CookieContainer = cookiec;
                request.ServicePoint.ConnectionLimit = 1000;
                request.ReadWriteTimeout = 13000;
                request.Method = "GET";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                response.Cookies = cookiec.GetCookies(request.RequestUri);
                Stream resStream = response.GetResponseStream();
                b = new Bitmap(resStream);
                response.Close();
                return b;
            }
            catch (Exception e)
            {
                errorString = e.Message;
            }
            finally
            {
                Console.WriteLine("!!!!!!!!!" + errorString);
            } return b;
        }

        /// <summary>
        /// get获取json信息
        /// </summary>
        /// <param name="getStringUrl"></param>
        /// <returns></returns>
        public string getString(string getStringUrl)
        {
            string strResult = "";
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(getStringUrl);
                request.AllowAutoRedirect = true;
                request.KeepAlive = true;
                request.Accept = "*/*";
                request.Headers.Add("Accept-Encoding", "gzip, deflate");
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; Maxthon; .NET CLR 2.0.50727)";
                request.Timeout = 200000;
                request.ServicePoint.ConnectionLimit = 100;
                request.ReadWriteTimeout = 300000;
                request.Method = "GET";
                request.ContentType = "application/json;charset=UTF-8";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream resStream = response.GetResponseStream();
                Stream streamReceive;
                string gzip = response.ContentEncoding;
                if (string.IsNullOrEmpty(gzip) || gzip.ToLower() != "gzip")
                {
                    streamReceive = response.GetResponseStream();
                }
                else
                {
                    streamReceive = new System.IO.Compression.GZipStream(response.GetResponseStream(), System.IO.Compression.CompressionMode.Decompress);
                }
                StreamReader sr = new System.IO.StreamReader(streamReceive, Encoding.UTF8);
                if (response.ContentLength > 1)
                {
                    strResult = sr.ReadToEnd();
                }
                else
                {
                    char[] buffer = new char[2048];
                    int count = 0;
                    StringBuilder sb = new StringBuilder();
                    while ((count = sr.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        sb.Append(new string(buffer));
                    }
                    strResult = sb.ToString();
                }
                sr.Close();
                response.Close();
                return strResult;
            }
            catch (Exception e)
            {
            }
            finally
            {
                Console.WriteLine("!!!!!!!!!");
            } return strResult;
        }

        #endregion - 基础方法 -

        #region - 系统功能 -

        /// <summary>
        /// 获取评论列表
        /// </summary>
        /// <param name="acNo"></param>
        /// <param name="size"></param>
        /// <param name="pageNo"></param>
        /// <returns></returns>
        public string GetCommentList(string acNo, int count, int size)
        {
            //http://www.acfun.tv/comment/content/web/list?contentId=1887142&pageNo=1&pageSize=100

            string url = "http://www.acfun.tv/comment_list_json.aspx?contentId=" + acNo + "&currentPage=" + count;
            //string url = "http://www.acfun.tv/comment/content/web/list?contentId=" + acNo + "&pageNo=" + pageNo + "&pageSize=100";
            string s = GetPage(url, url, null);
            return s;
        }

        /// <summary>
        /// 获取文章列表
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageNo"></param>
        /// <returns></returns>
        public string GetArticleList(int pageSize, int pageNo)
        {
            string url = "http://api.acfun.tv/apiserver/content/channel?channelId=110&pageNo=" + pageNo + "&pageSize=" + pageSize;
            string s = GetPage(url, url, null);
            return s;
        }

        /// <summary>
        /// 从网页获取文章
        /// </summary>
        /// <param name="articleNo"></param>
        /// <returns></returns>
        public string GetArticle(string articleNo)
        {
            //http://api.acfun.tv/apiserver/content/info?contentId=1887142
            string url = "http://api.acfun.tv/apiserver/content/info?contentId=" + articleNo;
            string s = GetPage(url, url, null);
            return s;
        }

        /// <summary>
        /// 获取图片
        /// </summary>
        /// <returns></returns>
        public Bitmap getWebPic(string url)
        {
            return getImage(url);
        }

        public string PostComment()
        {
            return "";
        }

        /// <summary>
        /// 通过用户名获取用户信息
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public string getUserInfo(string username)
        {
            //{"userjson":{"currExp":61,"stows":1,"comments":65,"gender":1,"level":22,"sign":"人家真的不搞基的。。","follows":0,"lastLoginDate":"2015-04-26 15:01:46.0","avatar":"http://static.acfun.mm111.net/dotnet/artemis/u/cms/www/201504/26143138p36q.jpg","posts":1,"followed":0,"lastLoginIp":"113.90.21.*","fans":0,"uid":1292228,"regTime":"2015-04-25 22:10:44.0","nextLevelNeed":100,"comeFrom":"","name":"一个不搞基的男孩子","dTime":"","expPercent":22,"isFriend":0,"views":0},"success":true}
            username = System.Web.HttpUtility.UrlEncode(username);
            return getString(userCardUrl + username);
        }

        /// <summary>
        /// 发布评论
        /// </summary>
        /// <param name="PostNo">纯数字ac号</param>
        /// <param name="data"></param>
        public string comment(string PostNo, string data, CookieContainer cookiec)
        {
            //posturl
            //http://www.acfun.tv/comment.aspx
            //data:
            //name:sendComm()
            //token:mimiko
            //quoteId:0
            //text:%E8%A6%81%E9%82%A3%E4%B9%88%E5%A4%9A%E7%9F%B3%E6%B2%B9%E5%B9%B2%E5%98%9B%EF%BC%8C%E5%8D%96%E4%BA%86%E5%8D%96%E4%BA%86
            //cooldown:5000
            //contentId:1832378
            //name=sendComm()&token=mimiko&quoteId=0&text=%E8%A6%81%E9%82%A3%E4%B9%88%E5%A4%9A%E7%9F%B3%E6%B2%B9%E5%B9%B2%E5%98%9B%EF%BC%8C%E5%8D%96%E4%BA%86%E5%8D%96%E4%BA%86&cooldown=5000&contentId=1832378
            //refer
            //http://www.acfun.tv/a/ac1832378
            data = System.Web.HttpUtility.UrlEncode(data);
            data = "name=sendComm()&token=mimiko&quoteId=0&text=" + data + "&cooldown=5000&contentId=" + PostNo + "&quoteName=";
            string result = "";
            try
            {
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create("http://www.acfun.tv/comment.aspx");
                myHttpWebRequest.AllowAutoRedirect = true;
                myHttpWebRequest.KeepAlive = true;
                myHttpWebRequest.Accept = "*/*";
                myHttpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate");
                myHttpWebRequest.CookieContainer = cookiec;
                myHttpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; Maxthon; .NET CLR 2.0.50727)";
                myHttpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                myHttpWebRequest.Referer = "http://www.acfun.tv/a/ac" + PostNo;
                myHttpWebRequest.Method = "POST";
                myHttpWebRequest.Timeout = 100000;
                myHttpWebRequest.ContentLength = data.Length;
                Stream postStream = myHttpWebRequest.GetRequestStream();
                byte[] postData = Encoding.UTF8.GetBytes(data);
                postStream.Write(postData, 0, postData.Length);
                postStream.Dispose();
                HttpWebResponse response = (HttpWebResponse)myHttpWebRequest.GetResponse();
                response.Cookies = cookiec.GetCookies(myHttpWebRequest.RequestUri);
                Stream streamReceive;
                string gzip = response.ContentEncoding;
                if (string.IsNullOrEmpty(gzip) || gzip.ToLower() != "gzip")
                {
                    streamReceive = response.GetResponseStream();
                }
                else
                {
                    streamReceive = new System.IO.Compression.GZipStream(response.GetResponseStream(), System.IO.Compression.CompressionMode.Decompress);
                }
                StreamReader sr = new System.IO.StreamReader(streamReceive, Encoding.UTF8);
                if (response.ContentLength > 1)
                {
                    result = sr.ReadToEnd();
                }
                else
                {
                    char[] buffer = new char[256];
                    int count = 0;
                    StringBuilder sb = new StringBuilder();
                    while ((count = sr.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        sb.Append(new string(buffer));
                    }
                    result = sb.ToString();
                }
                sr.Close();
                response.Close();
                string s = result;
                return s;
            }
            catch (Exception)
            {
                //throw;
            }
            finally
            {
                Console.WriteLine("!!!!!!!!!");
            }
            return "";
        }

        #endregion - 系统功能 -
    }
}