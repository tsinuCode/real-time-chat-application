using System;
using System.Web.UI;

namespace chatapp
{
    public partial class Login : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;

            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {
            }
        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
        }
    }
}
