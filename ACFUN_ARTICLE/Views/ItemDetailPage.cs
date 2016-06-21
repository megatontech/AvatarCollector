using System.Collections.Generic;
using System.Data;
using System.Drawing;
using Article.baseUtils;
using Article.Model;
using DevExpress.XtraEditors;

namespace Article
{
    /// <summary>
    /// A page that displays details for a single item within a group while allowing gestures to
    /// flip through other items belonging to the same group.
    /// </summary>
    public partial class ItemDetailPage : XtraUserControl
    {
        private WebUtil util = new WebUtil();
        public string acNo = "";
        private DataTable dt = null;
        private SqlLiteConn conn = new SqlLiteConn();

        private Article.Model.Article article = new Model.Article();
        private User user = new User();
        private List<Comment> commentList = new List<Comment>();

        public ItemDetailPage(ArticleDataItem item)
        {
            acNo = item.acNo;
            InitializeComponent();
            BindData();
            BindControls();
        }

        /// <summary>
        ///  绑定数据
        /// </summary>
        private void BindData()
        {
            string sqlsearch = "select * from Article where contentid =" + acNo;
            dt = conn.ExecuteDataTable(sqlsearch);
            if (dt != null)
            {
                article.setTxt(dt.Rows[0]["txt"].ToString());
                article.setDescription(dt.Rows[0]["description"].ToString());
                article.setReleaseDate(dt.Rows[0]["releaseDate"].ToString());
                article.setUser(dt.Rows[0]["user"].ToString());
            }

            sqlsearch = "select * from user where userid =" + article.getUser();
            dt = conn.ExecuteDataTable(sqlsearch);
            if (dt != null)
            {
                user.setlocalavatar(dt.Rows[0]["localavatar"].ToString());
                user.setName(dt.Rows[0]["username"].ToString());
                user.setSign(dt.Rows[0]["sign"].ToString());
            }
        }

        /// <summary>
        /// 绑定界面
        /// </summary>
        private void BindControls()
        {
            pictureBox1.Image = Image.FromFile(System.Environment.CurrentDirectory  + user.getlocalavatar());
            label_up.Text += user.getName();
            label_time.Text += article.getReleaseDate();
            label_sign.Text += user.getSign();
            label_context.Text += article.getDescription();
            richTextBox1.Text = article.getTxt();
        }

        /// <summary>
        /// 图片展示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void richTextBox1_DoubuleClick(object sender, System.EventArgs e)
        {
        }

        /// <summary>
        /// 查看评论
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_Click(object sender, System.EventArgs e)
        {
        }

    }
}