using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace chatapp
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadOnlineUsers();
            }
        }

        protected void btnSend_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtMessage.Text))
            {
                string message = txtMessage.Text;

                if (fileUpload.HasFile)
                {
                    string fileName = fileUpload.FileName;
                }

                txtMessage.Text = string.Empty;
            }
        }

        private void LoadOnlineUsers()
        {
            lstOnlineUsers.Items.Add("User 1 - Online");
            lstOnlineUsers.Items.Add("User 2 - Online");
            lstOnlineUsers.Items.Add("User 3 - Away");
        }
    }
}