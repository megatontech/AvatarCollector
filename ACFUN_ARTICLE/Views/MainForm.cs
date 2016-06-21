using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Article.baseUtils;
using DevExpress.XtraBars.Docking2010.Views;
using DevExpress.XtraBars.Docking2010.Views.WindowsUI;
using DevExpress.XtraEditors;

namespace Article
{
    public partial class MainForm : XtraForm
    {
        #region - 变量 -

        private AcfunDataSource dataSource;

        private Dictionary<PageDataGroup, PageGroup> groupsItemDetailPage;

        #endregion - 变量 -

        #region - 构造函数 -

        public MainForm()
        {
            InitializeComponent();
            windowsUIView.AddTileWhenCreatingDocument = DevExpress.Utils.DefaultBoolean.False;
            dataLoad();
            dataSource = new AcfunDataSource();
            groupsItemDetailPage = new Dictionary<PageDataGroup, PageGroup>();
            CreateLayout();
            timer1.Start();
        }

        #endregion - 构造函数 -

        #region - 事件 -

        private void tile_Click(object sender, TileClickEventArgs e)
        {
            PageGroup page = ((e.Tile as Tile).ActivationTarget as PageGroup);
            if (page != null)
            {
                page.Parent = tileContainer;
                page.SetSelected((e.Tile as Tile).Document);
            }
        }

        private void buttonClick(object sender, DevExpress.XtraBars.Docking2010.ButtonEventArgs e)
        {
            PageDataGroup tileGroup = (e.Button.Properties.Tag as PageDataGroup);
            if (tileGroup != null)
            {
                windowsUIView.ActivateContainer(groupsItemDetailPage[tileGroup]);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            dataSource.parallelData();
        }

        #endregion - 事件 -

        #region - 方法 -

        /// <summary>
        /// 测试数据库连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataLoad()
        {
            try
            {
                SqlLiteConn conn = new SqlLiteConn();
                conn.getConn();
            }
            catch (Exception ex)
            {
                string dblocation = System.Environment.CurrentDirectory + "\\DB\\article.db";
                string connstring = "Data Source=" + System.Environment.CurrentDirectory + "\\DB\\article.db";
                //System.Data.SQLite.SQLiteConnection.CreateFile(dblocation);
                MessageBox.Show("数据库迷之消失:" + ex.Message);
            }
            finally
            {
            }
            return;
        }

        private void CreateLayout()
        {
            foreach (PageDataGroup group in dataSource.Data.Groups)
            {
                tileContainer.Buttons.Add(new DevExpress.XtraBars.Docking2010.WindowsUIButton(group.Title, null, -1, DevExpress.XtraBars.Docking2010.ImageLocation.AboveText, DevExpress.XtraBars.Docking2010.ButtonStyle.PushButton, null, true, -1, true, null, false, false, true, null, group, -1, false, false));
                PageGroup pageGroup = new PageGroup();
                pageGroup.Parent = tileContainer;
                pageGroup.Caption = group.Title;
                windowsUIView.ContentContainers.Add(pageGroup);
                groupsItemDetailPage.Add(group, CreateGroupItemDetailPage(group, pageGroup));
                foreach (ArticleDataItem item in group.Items)
                {
                    ItemDetailPage itemDetailPage = new ItemDetailPage(item);
                    itemDetailPage.Dock = System.Windows.Forms.DockStyle.Fill;
                    BaseDocument document = windowsUIView.AddDocument(itemDetailPage);
                    document.Caption = item.Title;
                    pageGroup.Items.Add(document as Document);
                    CreateTile(document as Document, item).ActivationTarget = pageGroup;
                }
            }
            tileContainer.Buttons.Add(new DevExpress.XtraBars.Docking2010.WindowsUIButton("参与", null, -1, DevExpress.XtraBars.Docking2010.ImageLocation.AboveText, DevExpress.XtraBars.Docking2010.ButtonStyle.PushButton, null, true, -1, true, null, false, false, true, null, null, -1, false, false));
            windowsUIView.ActivateContainer(tileContainer);
            tileContainer.ButtonClick += new DevExpress.XtraBars.Docking2010.ButtonEventHandler(buttonClick);
        }

        private Tile CreateTile(Document document, ArticleDataItem item)
        {
            Tile tile = new Tile();
            tile.Document = document;
            tile.Group = item.GroupName;
            tile.Tag = item;
            tile.Elements.Add(CreateTileItemElement(item.Subtitle, TileItemContentAlignment.BottomLeft, Point.Empty, 9));
            tile.Elements.Add(CreateTileItemElement(item.Title, TileItemContentAlignment.MiddleLeft, Point.Empty, 14));
            tile.Appearances.Selected.BackColor = tile.Appearances.Hovered.BackColor = tile.Appearances.Normal.BackColor = Color.FromArgb(140, 140, 140);
            tile.Appearances.Selected.BorderColor = tile.Appearances.Hovered.BorderColor = tile.Appearances.Normal.BorderColor = Color.FromArgb(140, 140, 140);
            if (item.commentCount == "0")
            {
                tile.Appearances.Selected.BorderColor = tile.Appearances.Hovered.BorderColor = tile.Appearances.Normal.BorderColor = Color.FromArgb(156, 184, 180);
            }
            else if (item.commentCount.Length > 2)
            {
                tile.Appearances.Selected.BorderColor = tile.Appearances.Hovered.BorderColor = tile.Appearances.Normal.BorderColor = Color.FromArgb(197, 31, 31);
            }
            tile.Click += new TileClickEventHandler(tile_Click);
            windowsUIView.Tiles.Add(tile);
            tileContainer.Items.Add(tile);
            return tile;
        }

        private TileItemElement CreateTileItemElement(string text, TileItemContentAlignment alignment, Point location, float fontSize)
        {
            TileItemElement element = new TileItemElement();
            element.TextAlignment = alignment;
            if (!location.IsEmpty) element.TextLocation = location;
            element.Appearance.Normal.Options.UseFont = true;
            element.Appearance.Normal.Font = new System.Drawing.Font(new FontFamily("Segoe UI Symbol"), fontSize);
            element.Text = text;
            return element;
        }

        private PageGroup CreateGroupItemDetailPage(PageDataGroup group, PageGroup child)
        {
            GroupDetailPage page = new GroupDetailPage(group, child);
            PageGroup pageGroup = page.PageGroup;
            BaseDocument document = windowsUIView.AddDocument(page);
            pageGroup.Parent = tileContainer;
            pageGroup.Items.Add(document as Document);
            windowsUIView.ContentContainers.Add(pageGroup);
            windowsUIView.ActivateContainer(pageGroup);
            return pageGroup;
        }

        #endregion - 方法 -

    }
}