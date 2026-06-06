(function () {
    const currentUser = document.body.dataset.username || "You";

    const sampleConversations = [
        { id: "user-1", name: "Alice Johnson", preview: "See you tomorrow!", time: "10:42 AM", unread: 2, online: true },
        { id: "user-2", name: "Bob Smith", preview: "Thanks for the update.", time: "Yesterday", unread: 0, online: false },
        { id: "group-1", name: "CS Team", preview: "Meeting at 3 PM", time: "Mon", unread: 5, online: null, isGroup: true },
        { id: "user-3", name: "Carol Lee", preview: "Got it", time: "Sun", unread: 0, online: true }
    ];

    const sampleMessages = {
        "user-1": [
            { id: 1, sender: "Alice Johnson", text: "Hey! Are we still meeting tomorrow?", sentAt: "10:30 AM", isMine: false },
            { id: 2, sender: currentUser, text: "Yes, 10 AM works for me.", sentAt: "10:35 AM", isMine: true },
            { id: 3, sender: "Alice Johnson", text: "See you tomorrow!", sentAt: "10:42 AM", isMine: false }
        ],
        "user-2": [
            { id: 1, sender: "Bob Smith", text: "I pushed the latest changes.", sentAt: "Yesterday", isMine: false },
            { id: 2, sender: currentUser, text: "Thanks for the update.", sentAt: "Yesterday", isMine: true }
        ],
        "group-1": [
            { id: 1, sender: "Alice Johnson", text: "Don't forget standup.", sentAt: "Mon", isMine: false },
            { id: 2, sender: "Bob Smith", text: "Meeting at 3 PM", sentAt: "Mon", isMine: false },
            { id: 3, sender: currentUser, text: "I'll be there.", sentAt: "Mon", isMine: true }
        ],
        "user-3": [
            { id: 1, sender: currentUser, text: "Can you review my PR?", sentAt: "Sun", isMine: true },
            { id: 2, sender: "Carol Lee", text: "Got it", sentAt: "Sun", isMine: false }
        ]
    };

    const listContainer = document.getElementById("chatListItems");
    const searchInput = document.getElementById("chatSearch");
    const emptyState = document.getElementById("chatListEmpty");
    const messagesPanel = document.getElementById("messagesPanel");
    const messagesContainer = document.getElementById("messagesContainer");
    const messageInput = document.getElementById("messageInput");
    const sendButton = document.getElementById("sendButton");

    let activeChat = null;
    const conversationMessages = JSON.parse(JSON.stringify(sampleMessages));

    if (!listContainer) {
        return;
    }

    function renderList(items) {
        listContainer.querySelectorAll(".chat-list-item").forEach((el) => el.remove());

        if (items.length === 0) {
            emptyState?.classList.remove("d-none");
            return;
        }

        emptyState?.classList.add("d-none");

        items.forEach((chat) => {
            const item = document.createElement("button");
            item.type = "button";
            item.className = "chat-list-item";
            item.dataset.chatId = chat.id;
            item.innerHTML = `
                <div class="chat-list-avatar">${chat.isGroup ? '<i class="bi bi-people-fill"></i>' : chat.name.charAt(0)}</div>
                <div class="chat-list-body">
                    <div class="chat-list-top">
                        <span class="chat-list-name">${chat.name}</span>
                        <span class="chat-list-time">${chat.time}</span>
                    </div>
                    <div class="chat-list-bottom">
                        <span class="chat-list-preview">${chat.preview}</span>
                        ${chat.unread > 0 ? `<span class="chat-list-badge">${chat.unread}</span>` : ""}
                    </div>
                </div>
                ${chat.online === true ? '<span class="online-dot"></span>' : ""}
            `;

            item.addEventListener("click", () => selectChat(chat, item));
            listContainer.appendChild(item);
        });
    }

    function formatTime() {
        return new Date().toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
    }

    function renderMessages(chatId) {
        if (!messagesContainer) return;

        const messages = conversationMessages[chatId] || [];
        messagesContainer.innerHTML = "";

        messages.forEach((msg) => {
            const bubble = document.createElement("div");
            bubble.className = `message-row ${msg.isMine ? "mine" : "theirs"}`;
            bubble.innerHTML = `
                <div class="message-bubble">
                    ${!msg.isMine && activeChat?.isGroup ? `<div class="message-sender">${msg.sender}</div>` : ""}
                    <div class="message-text">${escapeHtml(msg.text)}</div>
                    <div class="message-time">${msg.sentAt}</div>
                </div>
            `;
            messagesContainer.appendChild(bubble);
        });

        messagesContainer.scrollTop = messagesContainer.scrollHeight;
    }

    function escapeHtml(text) {
        const div = document.createElement("div");
        div.textContent = text;
        return div.innerHTML;
    }

    function updateListPreview(chatId, text) {
        const chat = sampleConversations.find((c) => c.id === chatId);
        if (chat) {
            chat.preview = text;
            chat.time = formatTime();
            renderList(sampleConversations);
            const activeItem = listContainer.querySelector(`[data-chat-id="${chatId}"]`);
            activeItem?.classList.add("active");
        }
    }

    function sendMessage() {
        if (!activeChat || !messageInput) return;

        const text = messageInput.value.trim();
        if (!text) return;

        const newMessage = {
            id: Date.now(),
            sender: currentUser,
            text,
            sentAt: formatTime(),
            isMine: true
        };

        if (!conversationMessages[activeChat.id]) {
            conversationMessages[activeChat.id] = [];
        }

        conversationMessages[activeChat.id].push(newMessage);
        messageInput.value = "";
        renderMessages(activeChat.id);
        updateListPreview(activeChat.id, text);
    }

    function selectChat(chat, element) {
        activeChat = chat;

        listContainer.querySelectorAll(".chat-list-item").forEach((el) => el.classList.remove("active"));
        element.classList.add("active");

        const title = document.getElementById("activeChatTitle");
        const subtitle = document.getElementById("activeChatSubtitle");
        const placeholder = document.getElementById("chatPlaceholder");
        const composer = document.getElementById("chatComposer");

        if (title) title.textContent = chat.name;
        if (subtitle) {
            subtitle.textContent = chat.isGroup
                ? "Group chat"
                : chat.online ? "Online" : "Offline";
        }

        placeholder?.classList.add("d-none");
        messagesPanel?.classList.remove("d-none");
        composer?.classList.remove("d-none");

        renderMessages(chat.id);
        messageInput?.focus();

        if (window.ChatApp?.showChatView) {
            window.ChatApp.showChatView();
        }
    }

    function filterList(query) {
        const normalized = query.trim().toLowerCase();
        const filtered = sampleConversations.filter((chat) =>
            chat.name.toLowerCase().includes(normalized) ||
            chat.preview.toLowerCase().includes(normalized)
        );
        renderList(filtered);
    }

    renderList(sampleConversations);

    if (searchInput) {
        searchInput.addEventListener("input", (e) => filterList(e.target.value));
    }

    if (sendButton) {
        sendButton.addEventListener("click", sendMessage);
    }

    if (messageInput) {
        messageInput.addEventListener("keydown", (e) => {
            if (e.key === "Enter") {
                e.preventDefault();
                sendMessage();
            }
        });
    }
})();
