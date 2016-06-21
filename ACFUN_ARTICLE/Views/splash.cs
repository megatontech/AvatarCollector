using System;
using System.Windows.Forms;
using Article.baseUtils;
using DevExpress.XtraSplashScreen;

namespace Article
{
    public partial class Splash : SplashScreen
    {
        #region - 构造函数 -

        public Splash()
        {
            this.ShowMode = DevExpress.XtraSplashScreen.ShowMode.Form;
            InitializeComponent();
        }

        #endregion - 构造函数 -

        #region - 事件 -

        /// <summary>
        /// 测试数据库连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Splash_Load(object sender, EventArgs e)
        {
            try
            {
                SqlLiteConn conn = new SqlLiteConn();
                conn.getConn();
            }
            catch (Exception ex)
            {
                MessageBox.Show("数据库迷之消失:" + ex.Message);
            }
            finally
            {
                string connstring = "Data Source=" + System.Environment.CurrentDirectory + "\\DB\\article.db";
                System.Data.SQLite.SQLiteConnection.CreateFile(connstring);
            }
            return;
        }

        #endregion - 事件 -

        #region Overrides

        public override void ProcessCommand(Enum cmd, object arg)
        {
            base.ProcessCommand(cmd, arg);
        }

        #endregion Overrides
    }
}