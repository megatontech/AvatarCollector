using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using Article.baseUtils;
using Article.model;
using Article.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Article.BaseUtils
{
    public class ArticleOper
    {
        #region - 变量 -

        private DataTable dt = null;
        private SqlLiteConn conn = new SqlLiteConn();
        private WebUtil webutil = new WebUtil();

        #endregion - 变量 -

        #region - 评论 -

        /// <summary>
        /// 保存评论到数据库
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        public bool SaveComment(Comment comment)
        {
            string sql = "select * from comment where cid =" + comment.getCid();
            dt = conn.ExecuteDataTable(sql);
            if (dt.Rows.Count == 0)
            {
                string sqlInsert = "INSERT INTO 'comment' ('contentId','cid', 'quoteId', 'content', 'postDate', 'userID', 'userName', 'userImg', 'count', 'deep', 'refCount', 'ups', 'downs') VALUES ( '" + comment.getContentId() + "','" + comment.getCid() + "', '" + comment.getQuoteId() + "', '" + comment.getContent() + "', '" + comment.getPostDate() + "', '" + comment.getUserID() + "', '" + comment.getUserName() + "', '" + comment.getUserImg() + "', '" + comment.getCount() + "', '" + comment.getDeep() + "', '" + comment.getRefCount() + "', '" + comment.getUps() + "', '" + comment.getDowns() + "');";
                conn.ExecuteScalar(sqlInsert);
            }
            return true;
        }

        /// <summary>
        /// 获取并保存所有评论内容
        /// </summary>
        /// <param name="article"></param>
        /// <returns></returns>
        public List<Comment> getCommentByArticle(Article.Model.Article article)
        {
            List<Comment> commentList = new List<Comment>();

            int commentCount = 0;
            int.TryParse(article.getComments(), out commentCount);
            if (commentCount != 0 && commentCount < 50)
            {
                string commentJson = webutil.GetCommentList(article.getContentId(), 1, 100);
                if (commentJson.Length < 250)
                {
                    return null;
                }
                commentList = getCommentFromJson(commentJson, article.getContentId());
            }
            else if (commentCount == 0)
            {
                return null;
            }
            else
            {
                for (int count = 1; count <= commentCount % 50; count++)
                {
                    List<Comment> tempList = null;
                    string commentJson = webutil.GetCommentList(article.getContentId(), count, 100);
                    if (commentJson.Length < 250)
                    {
                        return null;
                    }
                    tempList = getCommentFromJson(commentJson, article.getContentId());
                    tempList.ForEach(i => commentList.Add(i));
                }
            }
            return commentList;
        }

        /// <summary>
        /// 解析Json获得评论
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public List<Comment> getCommentFromJson(string json, string contentId)
        {
            List<Comment> commentList = new List<Comment>();
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }
            JObject jsonObj = JObject.Parse(json);
            string list = ((JObject)jsonObj["commentContentArr"]).ToString();
            if (list.Equals("{}"))
            {
                return null;
            }
            string[] alist = Regex.Split(list, "},", RegexOptions.IgnoreCase);
            for (int i = 0; i < alist.Length; ++i)  //遍历JArray
            {
                string temp = alist[i];
                temp = temp.Replace(":{", ":,");
                temp = temp.Replace(": {", ":,");
                temp = temp.Replace("{", "");
                temp = temp.Replace("}", "");
                temp = "{" + temp + "}";
                Comment comment = new Comment();
                JObject tempo = JObject.Parse(temp);
                comment.setContentId(contentId);
                comment.setCid(tempo["cid"].ToString());
                comment.setContent(ReplaceHtmlTag(tempo["content"].ToString()));
                comment.setCount(tempo["count"].ToString());
                comment.setDowns(tempo["downs"].ToString());
                comment.setPostDate(tempo["postDate"].ToString());
                comment.setQuoteId(tempo["quoteId"].ToString());
                comment.setUps(tempo["ups"].ToString());
                comment.setUserID(tempo["userID"].ToString());
                comment.setUserImg(tempo["userImg"].ToString());
                comment.setUserName(tempo["userName"].ToString());
                SaveComment(comment);
                User user = getUserByComment(comment);
                if (user != null)
                {
                    SaveUser(user);
                }
                commentList.Add(comment);
            }
            return commentList;
        }

        #endregion - 评论 -

        #region - 文章 -

        /// <summary>
        /// 根据页码获取文章列表
        /// </summary>
        /// <param name="pageNo"></param>
        /// <returns></returns>
        public List<Article.Model.Article> getArticleList(int pageNo)
        {
            List<Article.Model.Article> articleList = new List<Model.Article>();
            WebUtil util = new WebUtil();
            //获取该页中所有的文章编号
            string articleNoList = util.GetArticleList(20, pageNo);
            if (articleNoList != "")
            {
                JObject jsonObj = JObject.Parse(articleNoList);
                string list = ((JObject)((JObject)jsonObj["data"])["page"])["list"].ToString();
                JArray alist = JArray.Parse(list);
                for (int i = 0; i < alist.Count; ++i)  //遍历JArray
                {
                    Article.Model.Article article = new Article.Model.Article();
                    JObject tempo = JObject.Parse(alist[i].ToString());
                    article.setChannelId(tempo["channelId"].ToString());
                    article.setComments(tempo["comments"].ToString());
                    article.setContentId(tempo["contentId"].ToString());
                    article.setCover(tempo["cover"].ToString());
                    article.setDescription(tempo["description"].ToString());
                    article.setIsArticle(tempo["isArticle"].ToString());
                    article.setIsRecommend(tempo["isRecommend"].ToString());
                    article.setReleaseDate(tempo["releaseDate"].ToString());
                    article.setStows(tempo["stows"].ToString());
                    article.setTitle(tempo["title"].ToString());
                    article.setToplevel(tempo["toplevel"].ToString());
                    article.setTxt(tempo["description"].ToString());
                    article.setUser(tempo["user"].ToString());
                    article.setViewOnly(tempo["viewOnly"].ToString());
                    article.setViews(tempo["views"].ToString());
                    getCommentByArticle(article);
                    articleList.Add(article);
                }
            }
            return articleList;
        }

        /// <summary>
        /// 获取所有图片
        /// </summary>
        /// <param name="sHtmlText"></param>
        /// <returns></returns>
        public string[] GetHtmlImageUrlList(string sHtmlText)
        {
            // 定义正则表达式用来匹配 img 标签
            Regex regImg = new Regex(@"<img\b[^<>]*?\bsrc[\s\t\r\n]*=[\s\t\r\n]*[""']?[\s\t\r\n]*(?<imgUrl>[^\s\t\r\n""'<>]*)[^<>]*?/?[\s\t\r\n]*>", RegexOptions.IgnoreCase);
            // 搜索匹配的字符串
            MatchCollection matches = regImg.Matches(sHtmlText);
            int i = 0;
            string[] sUrlList = new string[matches.Count];
            // 取得匹配项列表
            foreach (Match match in matches)
                sUrlList[i++] = match.Groups["imgUrl"].Value;
            return sUrlList;
        }

        /// <summary>
        /// 文字编码
        /// </summary>
        /// <param name="utfString"></param>
        /// <returns></returns>
        public string UnicodeEncode(string utfString)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(utfString);
            Encoding enc = new UnicodeEncoding(false, true, true);
            string value = enc.GetString(bytes);
            return value;
        }

        /// <summary>
        /// 替换标签存入数据库
        /// </summary>
        /// <param name="html"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string ReplaceHtmlTag(string html, int length = 0)
        {
            string strText = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", "");
            strText = System.Text.RegularExpressions.Regex.Replace(strText, "&[^;]+;", "");
            strText = strText.Replace("'", " ");
            strText = strText.Replace(",", " ");
            strText = strText.Replace("/", " ");
            strText = strText.Replace("\\", " ");
            strText = strText.Replace(";", " ");
            if (length > 0 && strText.Length > length)
                return strText.Substring(0, length);

            return strText;
        }

        /// <summary>
        /// 根据json保存文章全文并下载图片
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public Article.Model.Article getModelFromJson(string json)
        {
            if ((!json.Contains("该稿件未审核")) || json == "")
            {
                JObject jsonObj = JObject.Parse(json);
                JObject data = ((JObject)jsonObj["data"]);
                JObject tempo = ((JObject)data["fullArticle"]);
                Article.Model.Article article = new Article.Model.Article();
                article.setChannelId(tempo["channelId"].ToString());
                article.setComments(tempo["comments"].ToString());
                article.setContentId(tempo["contentId"].ToString());
                article.setCover(tempo["cover"].ToString());
                article.setDescription(ReplaceHtmlTag(tempo["description"].ToString()));
                article.setIsArticle(tempo["isArticle"].ToString());
                article.setIsRecommend(tempo["isRecommend"].ToString());
                article.setReleaseDate(tempo["releaseDate"].ToString());
                article.setStows(tempo["stows"].ToString());
                article.setTitle(tempo["title"].ToString());
                article.setToplevel(tempo["toplevel"].ToString());
                article.setTxt(ReplaceHtmlTag(tempo["txt"].ToString()));
                JObject user = ((JObject)tempo["user"]);
                article.setUser(user["userId"].ToString());
                article.setViewOnly(tempo["viewOnly"].ToString());
                article.setViews(tempo["views"].ToString());
                SavePostImg(GetHtmlImageUrlList(tempo["txt"].ToString()), article.getContentId());
                getUserByArticle(json);
                return article;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 获取文章内容
        /// </summary>
        /// <param name="acNo"></param>
        /// <returns></returns>
        public Article.Model.Article GetArticleFromWeb(string acNo)
        {
            Article.Model.Article article = new Model.Article();
            string json = webutil.GetArticle(acNo);
            article = getModelFromJson(json);
            return article;
        }

        /// <summary>
        /// 保存文章到数据库
        /// </summary>
        /// <param name="article"></param>
        /// <returns></returns>
        public bool SaveArticle(Article.Model.Article article)
        {
            string sql = "select * from Article where contentId =" + article.getContentId();
            dt = conn.ExecuteDataTable(sql);
            if (dt.Rows.Count == 0)
            {
                string sqlInsert = "INSERT INTO 'Article' ('user', 'txt', 'description', 'contentId', 'isArticle', 'channelId', 'releaseDate', 'title', 'isRecommend', 'views', 'comments', 'stows', 'toplevel', 'cover', 'viewOnly') VALUES ('" + article.getUser() + "', '" + article.getTxt() + "', '" + article.getDescription() + "', '" + article.getContentId() + "', '" + article.getIsArticle() + "', '" + article.getChannelId() + "', '" + article.getReleaseDate() + "', '" + article.getTitle() + "', '" + article.getIsRecommend() + "', '" + article.getViews() + "', '" + article.getComments() + "', '" + article
                    .getStows() + "', '" + article.getToplevel() + "', '" + article.getCover() + "', '" + article.getViewOnly() + "');";
                conn.ExecuteScalar(sqlInsert);
            }
            return true;
        }

        /// <summary>
        /// 保存文章中的图片
        /// </summary>
        /// <param name="urlList"></param>
        /// <param name="contentid"></param>
        public void SavePostImg(string[] urlList, string contentid)
        {
            string sql = "select * from image where contentid =" + contentid;
            dt = conn.ExecuteDataTable(sql);
            if (dt.Rows.Count != 0)
            {
                return;
            }
            Bitmap bitImage = null;
            foreach (string temp in urlList)
            {
                bitImage = webutil.getWebPic(temp);
                string imagename = Guid.NewGuid().ToString();
                string imagetype = "";
                if (bitImage != null)
                {
                    try
                    {
                        if (temp.EndsWith("png"))
                        {
                            imagetype = ".png";
                            bitImage.Save(System.Environment.CurrentDirectory + "\\Post\\" + imagename + imagetype, System.Drawing.Imaging.ImageFormat.Png);
                        }
                        else if (temp.EndsWith("gif"))
                        {
                            imagetype = ".gif";
                            bitImage.Save(System.Environment.CurrentDirectory + "\\Post\\" + imagename + imagetype, System.Drawing.Imaging.ImageFormat.Gif);
                        }
                        else
                        {
                            imagetype = ".jpg";
                            bitImage.Save(System.Environment.CurrentDirectory + "\\Post\\" + imagename + imagetype, System.Drawing.Imaging.ImageFormat.Jpeg);
                        }
                        string sqlInsert = "INSERT INTO 'image' ('imagename', 'contentid', 'imagetype') VALUES ('" + imagename + "', '" + contentid + "', '" + imagetype + "');";
                        conn.ExecuteScalar(sqlInsert);
                    }
                    catch(Exception e)
                    {
                        System.Console.Write(e.Message);
                    }finally
                    {
                        
                    }
                }
            }
        }

        #endregion - 文章 -

        #region - 用户 -

        /// <summary>
        /// 保存评论中的用户信息
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        public User getUserByComment(Comment comment)
        {
            User user = new User();

            user.setName(comment.getUserName());
            user.setUid(comment.getUserID());
            user.setAvatar(comment.getUserImg());
            string userJson = webutil.getUserInfo(user.getName());
            if (!userJson.Contains("用户被封禁") && !userJson.Contains("用户不存在"))
            {
                string uid = "";
                Root root = (Root)JsonConvert.DeserializeObject(userJson, typeof(Root));
                if (root != null)
                {
                    uid = root.getuserjson().getUid();
                    user.setComeFrom(root.getuserjson().getComeFrom());
                    user.setComments(root.getuserjson().getComments());
                    user.setCurrExp(root.getuserjson().getCurrExp());
                    user.setDTime(root.getuserjson().getDTime());
                    user.setExpPercent(root.getuserjson().getExpPercent());
                    user.setFans(root.getuserjson().getFans());
                    user.setFollowed(root.getuserjson().getFollowed());
                    user.setFollows(root.getuserjson().getFollows());
                    user.setGender(root.getuserjson().getGender());
                    user.setIsFriend(root.getuserjson().getIsFriend());
                    user.setLastLoginDate(root.getuserjson().getLastLoginDate());
                    user.setLastLoginIp(root.getuserjson().getLastLoginIp());
                    user.setLevel(root.getuserjson().getLevel());
                    user.setNextLevelNeed(root.getuserjson().getNextLevelNeed());
                    user.setPosts(root.getuserjson().getPosts());
                    user.setRegTime(root.getuserjson().getRegTime());
                    user.setSign(root.getuserjson().getSign());
                    user.setStows(root.getuserjson().getStows());
                    user.setViews(root.getuserjson().getViews());
                }
            }
            return user;
        }

        /// <summary>
        /// 保存up信息
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public User getUserByArticle(string json)
        {
            User user = new User();
            JObject jsonObj = JObject.Parse(json);
            JObject data = ((JObject)jsonObj["data"]);
            JObject tempo = ((JObject)data["fullArticle"]);
            JObject userObject = ((JObject)tempo["user"]);
            user.setName(userObject["username"].ToString());
            user.setUid(userObject["userId"].ToString());
            user.setAvatar(userObject["userImg"].ToString());
            string userJson = webutil.getUserInfo(user.getName());
            if (!userJson.Contains("用户被封禁") && !userJson.Contains("用户不存在"))
            {
                string uid = "";
                Root root = (Root)JsonConvert.DeserializeObject(userJson, typeof(Root));
                if (root != null)
                {
                    uid = root.getuserjson().getUid();
                    user.setComeFrom(root.getuserjson().getComeFrom());
                    user.setComments(root.getuserjson().getComments());
                    user.setCurrExp(root.getuserjson().getCurrExp());
                    user.setDTime(root.getuserjson().getDTime());
                    user.setExpPercent(root.getuserjson().getExpPercent());
                    user.setFans(root.getuserjson().getFans());
                    user.setFollowed(root.getuserjson().getFollowed());
                    user.setFollows(root.getuserjson().getFollows());
                    user.setGender(root.getuserjson().getGender());
                    user.setIsFriend(root.getuserjson().getIsFriend());
                    user.setLastLoginDate(root.getuserjson().getLastLoginDate());
                    user.setLastLoginIp(root.getuserjson().getLastLoginIp());
                    user.setLevel(root.getuserjson().getLevel());
                    user.setNextLevelNeed(root.getuserjson().getNextLevelNeed());
                    user.setPosts(root.getuserjson().getPosts());
                    user.setRegTime(root.getuserjson().getRegTime());
                    user.setSign(root.getuserjson().getSign());
                    user.setStows(root.getuserjson().getStows());
                    user.setViews(root.getuserjson().getViews());
                    SaveUser(user);
                    //string sqlsearch = "select * from user where userid =" + uid;
                    //dt = conn.ExecuteDataTable(sqlsearch);
                    //if (dt.Rows.Count == 0)
                    //{
                    //    string sql = "INSERT INTO 'user' ( 'userid', 'username', 'userpassword', 'moni', 'mailbox', 'currexp', 'level', 'follows', 'lastlogindate', 'stows', 'comments', 'gender', 'sign', 'avatar', 'posts', 'followed', 'lastloginip', 'fans', 'regTime', 'nextlevelneed', 'comefrom', 'dtime', 'exppercent', 'isfriend', 'views','localavatar') VALUES ( '" + uid + "', '" + user.getName() + "', '', NULL, NULL, '0', '00', '0', '" + user.getLastLoginDate() + "', '0', '0', '-1', '" + user.getSign() + "', '" + user.getAvatar() + "', '0', '0', '" + user.getLastLoginIp() + "', '0', '" + user.getRegTime() + "', '0', '', '', '0', '0', '0','" + "\\Image\\" + user.getUid() + ".jpg" + "');";
                    //    conn.ExecuteScalar(sql);
                    //}
                    //else
                    //{
                    //    if (uid == "" || uid == null)
                    //    {
                    //        return null;
                    //    }
                    //    else
                    //    {
                    //        string sql = "update user set currexp = " + root.getuserjson().getCurrExp() + ", level = " + root.getuserjson().getLevel() + " ,follows=" + root.getuserjson().getFollows() + " ,lastlogindate= '" + root.getuserjson().getLastLoginDate() + "',stows=" + root.getuserjson().getStows() + " ,comments=" + root.getuserjson().getComments() + " ,gender=" + root.getuserjson().getGender() + " ,sign='" + root.getuserjson().getSign() + "' ,avatar='" + root.getuserjson().getAvatar() + "' ,posts=" + root.getuserjson().getPosts() + " ,followed=" + root.getuserjson().getFollowed() + " ,lastloginip='" + root.getuserjson().getLastLoginIp() + "' ,fans=" + root.getuserjson().getFans() + " ,regTime='" + root.getuserjson().getRegTime() + "' ,nextlevelneed=" + root.getuserjson().getNextLevelNeed() + "  ,comefrom= '" + root.getuserjson().getComeFrom() + "',dtime='" + root.getuserjson().getDTime() + "' ,exppercent=" + root.getuserjson().getExpPercent() + " ,isfriend=" + root.getuserjson().getIsFriend() + " ,views=" + root.getuserjson().getViews() + " " +
                    //            " where  userid =" + uid + ";";
                    //        conn.ExecuteScalar(sql);
                    //    }
                    //}
                }
            }
            return user;
        }

        

        /// <summary>
        /// 保存用户到数据库
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool SaveUser(User user)
        {
            string sql = "select * from user where userid =" + user.getUid();
            dt = conn.ExecuteDataTable(sql);
            if (dt.Rows.Count == 0)
            {
                string sqlInsert = "INSERT INTO 'user' ('userid', 'username',  'currExp', 'stows', 'comments', 'gender', 'level', 'sign', 'follows', 'lastLoginDate', 'avatar', 'posts', 'followed', 'lastLoginIp', 'fans', 'regTime', 'nextLevelNeed', 'comeFrom', 'dTime', 'expPercent', 'isFriend', 'views', 'localavatar') VALUES ('" + user.getUid() + "', '" + user.getName() + "', '" + user.getCurrExp() + "', '" + user.getStows() + "', '" + user.getComments() + "', '" + user.getGender() + "', '" + user.getLevel() + "', '" + user.getSign() + "', '" + user.getFollows() + "', '" + user.getLastLoginDate() + "', '" + user.getAvatar() + "', '" + user.getPosts() + "', '" + user.getFollowed() + "', '" + user.getLastLoginIp() + "', '" + user.getFans() + "', '" + user.getRegTime() + "', '" + user.getNextLevelNeed() + "', '" + user.getComeFrom() + "', '" + user.getDTime() + "', '" + user.getExpPercent() + "', '" + user.getIsFriend() + "', '" + user.getViews() + "',  '" + "\\Image\\" + user.getUid() + ".jpg" + "');";
                conn.ExecuteScalar(sqlInsert);
                SaveLocalImg(user.getAvatar(),user.getUid());
            }
            return true;
        }

        /// <summary>
        /// 保存本地用户头像
        /// </summary>
        /// <returns></returns>
        public void SaveLocalImg(string url, string uid)
        {
            Bitmap bitImage = null;
            bitImage = webutil.getWebPic(url);
            if (bitImage != null)
            {
                bitImage.Save(System.Environment.CurrentDirectory + "\\Image\\" + uid + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            }
        }

        #endregion - 用户 -
    }
}