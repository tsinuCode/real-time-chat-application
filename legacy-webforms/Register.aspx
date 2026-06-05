<%@ Page Title="Register" Language="C#" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="chatapp.Register" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Register</title>
    <style type="text/css">
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            background-color: white;
            font-family: Arial, sans-serif;
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
        }

        .register-form {
            border: 1px solid #ccc;
            padding: 40px;
            width: 100%;
            max-width: 400px;
            box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
        }

        .form-group {
            margin-bottom: 20px;
        }

        .form-group label {
            display: block;
            margin-bottom: 5px;
            font-weight: bold;
        }

        .form-group input[type="text"],
        .form-group input[type="password"],
        .form-group input[type="tel"] {
            width: 100%;
            padding: 10px;
            border: 1px solid #ddd;
            font-size: 14px;
        }

        .register-btn {
            background-color: white;
            color: #333;
            border: 1px solid #ddd;
            padding: 10px;
            font-size: 14px;
            cursor: pointer;
            width: 100%;
            margin-top: 20px;
        }

        .register-btn:hover {
            background-color: #f0f0f0;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="register-form">
            <h2 style="text-align: center; margin-bottom: 30px;">Register</h2>

            <div class="form-group">
                <label for="txtPhoneNumber">Phone Number:</label>
                <asp:TextBox ID="txtPhoneNumber" runat="server" TextMode="Phone"></asp:TextBox>
            </div>

            <div class="form-group">
                <label for="txtUsername">Username:</label>
                <asp:TextBox ID="txtUsername" runat="server"></asp:TextBox>
            </div>

            <div class="form-group">
                <label for="txtPassword">Password:</label>
                <asp:TextBox ID="txtPassword" runat="server" TextMode="Password"></asp:TextBox>
            </div>

            <div class="form-group">
                <label for="txtConfirmPassword">Confirm Password:</label>
                <asp:TextBox ID="txtConfirmPassword" runat="server" TextMode="Password"></asp:TextBox>
            </div>

            <asp:Button ID="btnRegister" CssClass="register-btn" Text="Register" runat="server" OnClick="btnRegister_Click" />
        </div>
    </form>
</body>
</html>
