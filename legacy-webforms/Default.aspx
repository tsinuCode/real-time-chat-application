<%@ Page Title="Chat Application" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="chatapp._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <style type="text/css">
        body {
            background-color: white;
        }

        .chat-container {
            display: flex;
            height: 600px;
            border: 1px solid #ddd;
            background-color: white;
        }

        .chat-main {
            flex: 1;
            display: flex;
            flex-direction: column;
        }

        .chat-sidebar {
            width: 200px;
            border-left: 1px solid #ddd;
            padding: 10px;
            background-color: white;
        }

        .tabs {
            display: flex;
            border-bottom: 1px solid #ddd;
        }

        .tab-button {
            padding: 10px 20px;
            cursor: pointer;
            background-color: #f5f5f5;
            border: 1px solid #ddd;
            border-bottom: none;
            margin-right: 5px;
        }

        .tab-button.active {
            background-color: white;
            border-bottom: 3px solid #007bff;
        }

        .tab-content {
            flex: 1;
            display: none;
        }

        .tab-content.active {
            display: flex;
            flex-direction: column;
        }

        .messages-panel {
            flex: 1;
            overflow-y: auto;
            padding: 10px;
            background-color: white;
            border: 1px solid #e0e0e0;
            margin-bottom: 10px;
        }

        .input-section {
            padding: 10px;
            display: flex;
            gap: 5px;
            background-color: white;
        }

        .message-input {
            flex: 1;
            padding: 8px;
            border: 1px solid #ddd;
        }

        .users-label {
            font-weight: bold;
            margin-bottom: 10px;
            color: #333;
        }

        .users-list {
            height: 300px;
            border: 1px solid #ddd;
            background-color: white;
        }
    </style>

    <div class="chat-container">
        <div class="chat-main">
            <div class="tabs">
                <button type="button" class="tab-button active" onclick="switchTab('groupChat')">Group Chat</button>
                <button type="button" class="tab-button" onclick="switchTab('privateChat')">Private Chat</button>
            </div>

            <div id="groupChat" class="tab-content active">
                <asp:Panel ID="pnlGroupMessages" CssClass="messages-panel" runat="server">
                </asp:Panel>
            </div>

            <div id="privateChat" class="tab-content">
                <asp:Panel ID="pnlPrivateMessages" CssClass="messages-panel" runat="server">
                </asp:Panel>
            </div>

            <div class="input-section">
                <asp:TextBox ID="txtMessage" CssClass="message-input" placeholder="Type your message..." runat="server"></asp:TextBox>
                <asp:Button ID="btnSend" Text="Send" runat="server" OnClick="btnSend_Click" />
                <asp:FileUpload ID="fileUpload" runat="server" AllowMultiple="false" />
            </div>
        </div>

        <div class="chat-sidebar">
            <div class="users-label">Online Users</div>
            <asp:ListBox ID="lstOnlineUsers" CssClass="users-list" runat="server"></asp:ListBox>
        </div>
    </div>

    <script type="text/javascript">
        function switchTab(tabName) {
            var tabs = document.querySelectorAll('.tab-content');
            for (var i = 0; i < tabs.length; i++) {
                tabs[i].classList.remove('active');
            }

            var buttons = document.querySelectorAll('.tab-button');
            for (var j = 0; j < buttons.length; j++) {
                buttons[j].classList.remove('active');
            }

            var activeTab = document.getElementById(tabName);
            if (activeTab) {
                activeTab.classList.add('active');
            }

            if (event.target) {
                event.target.classList.add('active');
            }
        }

        document.addEventListener('DOMContentLoaded', function () {
            var fileInput = document.getElementById('<%= fileUpload.ClientID %>');
            var dropZone = document.querySelector('.input-section');

            if (dropZone) {
                dropZone.addEventListener('dragover', function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                    dropZone.style.backgroundColor = '#f0f0f0';
                });

                dropZone.addEventListener('dragleave', function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                    dropZone.style.backgroundColor = '';
                });

                dropZone.addEventListener('drop', function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                    dropZone.style.backgroundColor = '';
                    if (e.dataTransfer && e.dataTransfer.files && fileInput) {
                        fileInput.files = e.dataTransfer.files;
                    }
                });
            }
        });
    </script>

</asp:Content>
