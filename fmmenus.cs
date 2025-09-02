using System;
using System.Drawing;
using System.Windows.Forms;

namespace FamAlbum
{

    static class fmmenus
    {
        private readonly static MenuStrip menuStrip = new MenuStrip();
        private static string[] Namesselected = null;
        private static ConnectionManager Manager = new ConnectionManager(SharedCode.GetConnectionString());

        public static MenuStrip fmenus()
        {
            var mainmenustrip = new MenuStrip();

            // Exit
            var menuItemExit = new ToolStripMenuItem("Exit");
            menuItemExit.Click += ExitApp;
            mainmenustrip.Items.Add(menuItemExit);

            // Select People
            var menuItemSelectPeople = new ToolStripMenuItem("Select People");
            menuItemSelectPeople.Click += MenuItemPeople_Click;
            mainmenustrip.Items.Add(menuItemSelectPeople);

            // Select Event
            var menuItemSelectEvent = new ToolStripMenuItem("Select Event");
            menuItemSelectEvent.Click += MenuItemEvent_Click;
            mainmenustrip.Items.Add(menuItemSelectEvent);

            // Pictures with no names
            var menuItemNoname = new ToolStripMenuItem("Pictures with no names listed");
            menuItemNoname.Click += MenuitemNoname_Click;
            mainmenustrip.Items.Add(menuItemNoname);

            // Add Pictures submenu
            var menuItemAdd = new ToolStripMenuItem("Add Images");
            var FindPhotos = new ToolStripMenuItem("Find Images in All Folder");
            // Dim FolderPhotos As New ToolStripMenuItem("Select Folder for New Images")
            var NewEvent = new ToolStripMenuItem("Event - New");
            var OldEvent = new ToolStripMenuItem("Event");
            var NoEvent = new ToolStripMenuItem("No Event");

            FindPhotos.Click += FindPhotos_click;
            // AddHandler FolderPhotos.Click, AddressOf FindFilesinFolder_click
            NewEvent.Click += NewEvent_click;
            OldEvent.Click += OldEvent_click;
            NoEvent.Click += NoEvent_click;
            // add folder folders if needed
            menuItemAdd.DropDownItems.AddRange(new[] { FindPhotos, NewEvent, OldEvent, NoEvent });
            mainmenustrip.Items.Add(menuItemAdd);

            // Utilities
            var menuitemUtilities = new ToolStripMenuItem("Utilites");

            var EventManager = new ToolStripMenuItem("Event Manager");
            var Namemanager = new ToolStripMenuItem("Name Manager");
            var menuItemCheck = new ToolStripMenuItem("Check Integrity");
            var menuitemBackup = new ToolStripMenuItem("Backup");
            var menuitemRestore = new ToolStripMenuItem("Restore");

            EventManager.Click += EvManage_click;
            Namemanager.Click += nmManage_click;
            menuItemCheck.Click += MenuitemCheck_Click;
            menuitemBackup.Click += menuitemBackup_click;
            menuitemRestore.Click += menuitemRestore_click;

            menuitemUtilities.DropDownItems.AddRange(new[] { EventManager, Namemanager, menuItemCheck, menuitemBackup, menuitemRestore });
            mainmenustrip.Items.Add(menuitemUtilities);

            // Pending Images
            int pendingCount = SharedCode.GetPendingImageCount();
            var pendingMenuItem = new ToolStripMenuItem($"{pendingCount} images waiting to be added");
            mainmenustrip.Items.Add(pendingMenuItem);

            // Set font
            foreach (ToolStripMenuItem menuItem in mainmenustrip.Items)
                menuItem.Font = new Font("Segoe UI", 9.0f, FontStyle.Bold);

            return mainmenustrip;
        }

        public static void EvManage_click(object sender, EventArgs e)
        {
            var ev = new EventManagmentType();
            ev.Show();
        }
        public static void nmManage_click(object sender, EventArgs e)
        {
            var ev = new NameEditor();
            ev.Show();
        }

        public static void NewEvent_click(object sender, EventArgs e)
        {
            var ad = new Select_Event();
            string Etype = "New";
            ad.Etype = Etype;
            ad.Show();
        }
        public static void OldEvent_click(object sender, EventArgs e)
        {
            var ad = new Select_Event();
            string Etype = "Old";
            ad.Etype = Etype;
            ad.Show();
        }
        public static void NoEvent_click(object sender, EventArgs e)
        {
            var ad = new AddPhoto();
            string Etype = "No";
            ad.Etype = Etype;
            ad.Show();
        }
        public static void FindPhotos_click(object sender, EventArgs e)
        {
            Unindexedfiles.RunUnindexedFileSearchWithSplash();
        }

        public static void MenuitemCheck_Click(object sender, EventArgs e)
        {
            string defaultDir = SharedCode.GetDefaultDir();
            var connection = Manager.GetConnection();
            SharedCode.VerifyPictureFilesExist(defaultDir);
            SharedCode.CleanPpeoplelistAndUpdateCount(connection);
        }
        public static void MenuitemNoname_Click(object sender, EventArgs e)
        {
            Namesselected = new string[6];
            Namesselected[0] = "NP";
            Namesselected[1] = "99999";
            Namesselected[2] = "99999";
            Namesselected[3] = "99999";
            Namesselected[4] = "99999";
            Namesselected[5] = "99999";
            var thumbForm = new Sthumb();
            thumbForm.NamesSelected = Namesselected;
            thumbForm.Show();

        }
        private static void MenuItemEvent_Click(object sender, EventArgs e)
        {
            var evnt = new Select_Event();
            evnt.Etype = "Ign";
            evnt.Show();
        }
        public static void MenuItemPeople_Click(object sender, EventArgs e)
        {
            var pp = new Start();
            pp.Show();
        }
        public static void ExitApp(object sender, EventArgs e)
        {
            Control ctrl = sender as Control;
            if (ctrl is not null)
            {
                var frm = ctrl.FindForm();
                if (frm is not null)
                {
                    frm.Close();
                }
            }
        }
        private static void menuitemBackup_click(object sener, EventArgs e)
        {
            string connectionString = SharedCode.GetConnectionString();
            string filename = "FamAlbum" + DateTime.Now.ToString("MMddyyyy") + ".bak";
            string BackupPath = BackupRest.GetBackupPath() + filename;
            BackupRest.BackupSQLiteDatabase();
        }
        private static void menuitemRestore_click(object sender, EventArgs e)
        {
            BackupRest.RestoreSQLiteDatabase();
        }

    }

}