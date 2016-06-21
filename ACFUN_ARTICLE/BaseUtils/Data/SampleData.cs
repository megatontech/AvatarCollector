using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Article.BaseUtils;
using System.Threading.Tasks;
using System.Data;
using Article.baseUtils;

// The data model defined by this file serves as a representative example of a strongly-typed
// model that supports notification when members are added, removed, or modified.  The property
// names chosen coincide with data bindings in the standard item WindowsUIViewApplications.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs.

namespace Article
{
    /// <summary>
    /// Base class for <see cref="ArticleDataItem"/> and <see cref="PageDataGroup"/> that
    /// defines properties common to both.
    /// </summary>
    public class ArticleData
    {
        private string subtitleCore, imagePathCore, descriptionCore, titleCore;

        public string Title { get { return titleCore; } }

        public string Subtitle { get { return subtitleCore; } }

        public string ImagePath { get { return imagePathCore; } }

        public string Description { get { return descriptionCore; } }

        public ArticleData(string title, string subtitle, string imagePath, string description)
        {
            titleCore = title;
            subtitleCore = subtitle;
            imagePathCore = imagePath;
            descriptionCore = description;
        }

        public ArticleData()
        {
        }
    }

    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class ArticleDataItem : ArticleData
    {
        private string contentCore, groupNameCore;

        public ArticleDataItem(string title, string subtitle, string imagePath, string description, string content, string groupName, string commentCount, string acNo)
            : base(title, subtitle, imagePath, description)
        {
            contentCore = content;
            groupNameCore = groupName;
            CommentCount = commentCount;
            AcNo = acNo;
        }

        public string Content { get { return contentCore; } }

        public string GroupName { get { return groupNameCore; } }

        private string CommentCount;

        public string commentCount
        {
            get { return CommentCount; }
            set { CommentCount = value; }
        }

        private string AcNo;

        public string acNo
        {
            get { return AcNo; }
            set { AcNo = value; }
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class PageDataGroup : ArticleData
    {
        private string nameCore;
        private Collection<ArticleDataItem> itemsCore;

        public PageDataGroup(string name)
            : base()
        {
            this.nameCore = name;
            itemsCore = new Collection<ArticleDataItem>();
        }

        public PageDataGroup(string name, string title, string subtitle, string imagePath, string description)
            : base(title, subtitle, imagePath, description)
        {
            this.nameCore = name;
            itemsCore = new Collection<ArticleDataItem>();
        }

        public string Name { get { return nameCore; } }

        public Collection<ArticleDataItem> Items { get { return itemsCore; } }

        public bool AddItem(ArticleDataItem tile)
        {
            if (!itemsCore.Contains(tile))
            {
                itemsCore.Add(tile);
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Generic data model.
    /// </summary>
    internal class ContentDataModel
    {
        private Collection<PageDataGroup> groupsCore;

        public ContentDataModel()
        {
            groupsCore = new Collection<PageDataGroup>();
        }

        public Collection<PageDataGroup> Groups { get { return groupsCore; } }

        private PageDataGroup GetGroup(string name)
        {
            foreach (var group in groupsCore)
                if (group.Name == name) return group;
            return null;
        }

        public bool AddItem(ArticleDataItem tile)
        {
            if (tile == null) return false;
            string groupName = tile.GroupName == null ? "" : tile.GroupName;
            PageDataGroup thisGroup = GetGroup(groupName);
            if (thisGroup == null)
            {
                thisGroup = new PageDataGroup(groupName);
                groupsCore.Add(thisGroup);
            }
            return thisGroup.AddItem(tile);
        }

        private bool ContainsGroup(string name)
        {
            return GetGroup(name) != null;
        }

        public void CreateGroup(string name, string title, string subtitle, string imagePath, string description)
        {
            if (ContainsGroup(name)) return;
            PageDataGroup group = new PageDataGroup(name, title, subtitle, imagePath, description);
            groupsCore.Add(group);
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with hard-coded content.
    ///
    /// SampleDataSource initializes with placeholder data rather than live production
    /// data so that sample data is provided at both design-time and run-time.
    /// </summary>
    internal class AcfunDataSource
    {
        private DataTable dt = null;
        private SqlLiteConn conn = new SqlLiteConn();
        private WebUtil webutil = new WebUtil();
        private ContentDataModel dataCore= new ContentDataModel();
        ArticleOper oper = new ArticleOper();
        /// <summary>
        /// 构造数据源
        /// </summary>
        public AcfunDataSource()
        {
            for (int count = 1; count < 11; count++)
            {
                //页码
                dataCore.CreateGroup(" 第-" + count + "-页 ",
                    "Group Title: " + count,
                    "Group Subtitle: " + count,
                    typeof(MainForm).Namespace + ".Assets.DarkGray.png",
                    "哈哈哈哈哈哈哈哈哈哈");
                //数据库分页查10个
                string sql = "";
                dt = conn.ExecuteDataTable(sql);
                //循环写入
                for (int dtcount = 0; dtcount < dt.Rows.Count; dtcount++)
                {
                    Article.Model.Article article = new Model.Article();
                    article.setComments(dt.Rows[dtcount]["comments"].ToString());
                    article.setTitle(dt.Rows[dtcount]["title"].ToString());
                    article.setContentId(dt.Rows[dtcount]["contentId"].ToString());
                    article.setDescription(dt.Rows[dtcount]["description"].ToString());
                    article.setTxt(dt.Rows[dtcount]["txt"].ToString());
                    dataCore.AddItem(new ArticleDataItem(article.getTitle(),
                    "  评论数：  " + article.getComments(),
                    typeof(MainForm).Namespace + ".Assets.LightGray.png",
                    "Description: " + article.getDescription(),
                    article.getTxt(),
                    " 第-" + count + "-页 ", article.getComments(), article.getContentId()));
                }
            }
        }
        /// <summary>
        /// 并发准备数据
        /// </summary>
        public void parallelData()
        {
            Parallel.For(0,10, (i) => { GetData(i); });



            //////97752*10 20.24 10000
            //for (int count = 0; count < 24; count++)
            //{
            //    Parallel.For(count * 10, (count + 1) * 10, (i) => { GetData(i); });
            //}

        }
        /// <summary>
        /// 保存文件和数据
        /// </summary>
        /// <param name="count"></param>
        public void GetData(int count) 
        {
            List<Article.Model.Article> articleList = oper.getArticleList(count);
                foreach (Article.Model.Article article in articleList)
                {
                    Article.Model.Article fullarticle = new Model.Article();
                    fullarticle = oper.GetArticleFromWeb(article.getContentId());
                    oper.SaveArticle(fullarticle);
                    
                }
        }

        public ContentDataModel Data { get { return dataCore; } }
    }
}