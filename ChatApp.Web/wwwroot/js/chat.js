(function () {
    const dashboard = document.getElementById("chatDashboard");
    const currentUser = document.body.dataset.username || "You";
    const currentUserId = dashboard?.dataset.userId || "";
    const jwtToken = dashboard?.dataset.jwtToken || "";
    const apiBaseUrl = dashboard?.dataset.apiUrl || "";
    const hubUrl = dashboard?.dataset.hubUrl || "";

    const fallbackConversations = [
        { id: "user-1", receiverId: "user-alice", name: "Alice Johnson", preview: "See you tomorrow!", time: "10:42 AM", unread: 2, online: true, isGroup: false },
        { id: "user-2", receiverId: "user-bob", name: "Bob Smith", preview: "Thanks for the update.", time: "Yesterday", unread: 0, online: false, isGroup: false },
        { id: "group-1", groupId: 1, name: "CS Team", preview: "Meeting at 3 PM", time: "Mon", unread: 5, online: null, isGroup: true },
        { id: "user-3", receiverId: "user-carol", name: "Carol Lee", preview: "Got it", time: "Sun", unread: 0, online: true, isGroup: false }
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
    const typingIndicator = document.getElementById("typingIndicator");
    const connectionStatus = document.getElementById("connectionStatus");

    let conversations = [];
    let activeChat = null;
    let hubConnection = null;
    let typingTimer = null;
    const conversationMessages = JSON.parse(JSON.stringify(sampleMessages));

    if (!listContainer) {
        return;
    }

    function setConnectionStatus(text, isOnline) {
        if (!connectionStatus) return;
        connectionStatus.textContent = text;
        connectionStatus.classList.toggle("text-success", isOnline === true);
        connectionStatus.classList.toggle("text-danger", isOnline === false);
        connectionStatus.classList.toggle("text-muted", isOnline === null);
    }

    function formatTime(dateValue) {
        const date = dateValue ? new Date(dateValue) : new Date();
        return date.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
    }

    function mapSummaryToChat(summary) {
        const isGroup = summary.conversationType === "group";
        const conversationId = summary.conversationId;
        return {
            id: isGroup ? `group-${conversationId}` : `user-${conversationId}`,
            receiverId: isGroup ? null : conversationId,
            groupId: isGroup ? parseInt(conversationId, 10) : null,
            name: summary.title,
            preview: summary.lastMessagePreview,
            time: summary.lastMessageAt ? formatTime(summary.lastMessageAt) : "",
            unread: summary.unreadCount ?? 0,
            online: isGroup ? null : summary.isOnline,
            isGroup
        };
    }

    function mapDtoToMessage(dto) {
        return {
            id: dto.id,
            sender: dto.senderUsername,
            text: dto.content,
            sentAt: formatTime(dto.sentAt),
            isMine: dto.senderId === currentUserId
        };
    }

    function refreshList() {
        const query = searchInput?.value || "";
        if (query.trim()) {
            filterList(query);
            return;
        }

        renderList(conversations);
        if (activeChat) {
            listContainer.querySelector(`[data-chat-id="${activeChat.id}"]`)?.classList.add("active");
        }
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
                        <span class="chat-list-name">${escapeHtml(chat.name)}</span>
                        <span class="chat-list-time">${escapeHtml(chat.time)}</span>
                    </div>
                    <div class="chat-list-bottom">
                        <span class="chat-list-preview">${escapeHtml(chat.preview)}</span>
                        ${chat.unread > 0 ? `<span class="chat-list-badge">${chat.unread}</span>` : ""}
                    </div>
                </div>
                ${chat.online === true ? '<span class="online-dot"></span>' : ""}
            `;

            item.addEventListener("click", () => selectChat(chat, item));
            listContainer.appendChild(item);
        });
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
                    ${!msg.isMine && activeChat?.isGroup ? `<div class="message-sender">${escapeHtml(msg.sender)}</div>` : ""}
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

    function appendMessage(chatId, message) {
        if (!conversationMessages[chatId]) {
            conversationMessages[chatId] = [];
        }

        const exists = conversationMessages[chatId].some((m) => m.id === message.id);
        if (!exists) {
            conversationMessages[chatId].push(message);
        }

        if (activeChat?.id === chatId) {
            renderMessages(chatId);
        }
    }

    function updateListPreview(chatId, text) {
        const chat = conversations.find((c) => c.id === chatId);
        if (chat) {
            chat.preview = text;
            chat.time = formatTime();
            refreshList();
        }
    }

    function findChatForMessage(dto) {
        if (dto.groupId) {
            return conversations.find((c) => c.isGroup && c.groupId === dto.groupId);
        }

        const otherUserId = dto.senderId === currentUserId ? dto.receiverId : dto.senderId;
        return conversations.find((c) => !c.isGroup && c.receiverId === otherUserId);
    }

    function findChatByConversation(conversationType, conversationId) {
        if (conversationType === "group") {
            const groupId = parseInt(conversationId, 10);
            return conversations.find((c) => c.isGroup && c.groupId === groupId);
        }

        return conversations.find((c) => !c.isGroup && c.receiverId === conversationId);
    }

    async function loadConversations() {
        if (!apiBaseUrl || !jwtToken) {
            conversations = [...fallbackConversations];
            return;
        }

        try {
            const response = await fetch(`${apiBaseUrl}/api/messages/summaries`, {
                headers: { Authorization: `Bearer ${jwtToken}` }
            });

            if (!response.ok) {
                conversations = [...fallbackConversations];
                return;
            }

            const payload = await response.json();
            if (payload?.success && Array.isArray(payload.data)) {
                conversations = payload.data.map(mapSummaryToChat);
            } else {
                conversations = [...fallbackConversations];
            }
        } catch {
            conversations = [...fallbackConversations];
        }
    }

    async function loadHistory(chat) {
        if (!apiBaseUrl || !jwtToken) return false;

        const endpoint = chat.isGroup
            ? `${apiBaseUrl}/api/messages/group/${chat.groupId}`
            : `${apiBaseUrl}/api/messages/private/${chat.receiverId}`;

        try {
            const response = await fetch(endpoint, {
                headers: { Authorization: `Bearer ${jwtToken}` }
            });

            if (!response.ok) return false;

            const payload = await response.json();
            if (!payload?.success || !Array.isArray(payload.data)) return false;

            conversationMessages[chat.id] = payload.data.map(mapDtoToMessage);
            renderMessages(chat.id);
            return true;
        } catch {
            return false;
        }
    }

    async function joinGroupIfNeeded(chat) {
        if (!hubConnection || hubConnection.state !== signalR.HubConnectionState.Connected) return;
        if (!chat.isGroup || !chat.groupId) return;

        try {
            await hubConnection.invoke("JoinGroupChat", chat.groupId);
        } catch {
            // Group join may fail for demo/sample data.
        }
    }

    async function sendMessage() {
        if (!activeChat || !messageInput) return;

        const text = messageInput.value.trim();
        if (!text) return;

        messageInput.value = "";
        sendTypingState(false);

        if (hubConnection && hubConnection.state === signalR.HubConnectionState.Connected) {
            try {
                if (activeChat.isGroup) {
                    await hubConnection.invoke("SendGroupMessage", activeChat.groupId, text);
                } else {
                    await hubConnection.invoke("SendPrivateMessage", activeChat.receiverId, text);
                }
                return;
            } catch {
                // Fall back to local preview when hub call fails.
            }
        }

        const localMessage = {
            id: Date.now(),
            sender: currentUser,
            text,
            sentAt: formatTime(),
            isMine: true
        };

        appendMessage(activeChat.id, localMessage);
        updateListPreview(activeChat.id, text);
    }

    function handleIncomingMessage(dto) {
        const chat = findChatForMessage(dto);
        if (!chat) return;

        appendMessage(chat.id, mapDtoToMessage(dto));

        if (activeChat?.id !== chat.id && dto.senderId !== currentUserId) {
            chat.unread = (chat.unread || 0) + 1;
        }

        updateListPreview(chat.id, dto.content);
    }

    function handleUnreadCountUpdated(conversationType, conversationId, unreadCount) {
        const chat = findChatByConversation(conversationType, conversationId);
        if (!chat) return;

        chat.unread = unreadCount;
        refreshList();
    }

    function handleTypingIndicator(indicator) {
        if (!activeChat || !typingIndicator) return;

        const isCurrentChat = activeChat.isGroup
            ? indicator.groupId === activeChat.groupId
            : indicator.userId === activeChat.receiverId || indicator.receiverId === currentUserId;

        if (!isCurrentChat || indicator.userId === currentUserId) return;

        typingIndicator.classList.toggle("d-none", !indicator.isTyping);
        const text = typingIndicator.querySelector(".typing-text");
        if (text) {
            text.textContent = indicator.isTyping ? `${indicator.username} is typing...` : "";
        }
    }

    function handleUserStatusChanged(userId, isOnline) {
        conversations.forEach((chat) => {
            if (!chat.isGroup && chat.receiverId === userId) {
                chat.online = isOnline;
            }
        });
        refreshList();

        if (activeChat && !activeChat.isGroup && activeChat.receiverId === userId) {
            const subtitle = document.getElementById("activeChatSubtitle");
            if (subtitle) subtitle.textContent = isOnline ? "Online" : "Offline";
        }
    }

    function sendTypingState(isTyping) {
        if (!hubConnection || hubConnection.state !== signalR.HubConnectionState.Connected || !activeChat) return;

        const payload = activeChat.isGroup
            ? [null, activeChat.groupId, isTyping]
            : [activeChat.receiverId, null, isTyping];

        hubConnection.invoke("SendTypingIndicator", ...payload).catch(() => { });
    }

    async function selectChat(chat, element) {
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
        typingIndicator?.classList.add("d-none");

        renderMessages(chat.id);
        await loadHistory(chat);
        chat.unread = 0;
        refreshList();
        listContainer.querySelector(`[data-chat-id="${chat.id}"]`)?.classList.add("active");
        await joinGroupIfNeeded(chat);
        messageInput?.focus();

        if (window.ChatApp?.showChatView) {
            window.ChatApp.showChatView();
        }
    }

    function filterList(query) {
        const normalized = query.trim().toLowerCase();
        const filtered = conversations.filter((chat) =>
            chat.name.toLowerCase().includes(normalized) ||
            chat.preview.toLowerCase().includes(normalized)
        );
        renderList(filtered);
        if (activeChat) {
            listContainer.querySelector(`[data-chat-id="${activeChat.id}"]`)?.classList.add("active");
        }
    }

    async function initSignalR() {
        if (!hubUrl || !jwtToken || typeof signalR === "undefined") {
            setConnectionStatus("Offline mode", null);
            return;
        }

        hubConnection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl, {
                accessTokenFactory: () => jwtToken,
                withCredentials: true
            })
            .withAutomaticReconnect()
            .build();

        hubConnection.on("ReceivePrivateMessage", handleIncomingMessage);
        hubConnection.on("ReceiveGroupMessage", handleIncomingMessage);
        hubConnection.on("TypingIndicator", handleTypingIndicator);
        hubConnection.on("UserStatusChanged", handleUserStatusChanged);
        hubConnection.on("UnreadCountUpdated", handleUnreadCountUpdated);

        hubConnection.onreconnecting(() => setConnectionStatus("Reconnecting...", null));
        hubConnection.onreconnected(async () => {
            setConnectionStatus("Connected", true);
            if (activeChat?.isGroup) {
                await joinGroupIfNeeded(activeChat);
            }
        });
        hubConnection.onclose(() => setConnectionStatus("Disconnected", false));

        try {
            await hubConnection.start();
            setConnectionStatus("Connected", true);
        } catch {
            setConnectionStatus("Offline mode", false);
        }
    }

    function resetActiveChat() {
        activeChat = null;
        listContainer.querySelectorAll(".chat-list-item").forEach((el) => el.classList.remove("active"));

        const title = document.getElementById("activeChatTitle");
        const subtitle = document.getElementById("activeChatSubtitle");
        const placeholder = document.getElementById("chatPlaceholder");
        const composer = document.getElementById("chatComposer");

        if (title) title.textContent = "Select a chat";
        if (subtitle) subtitle.textContent = "Choose a conversation from the list";
        placeholder?.classList.remove("d-none");
        messagesPanel?.classList.add("d-none");
        composer?.classList.add("d-none");
        typingIndicator?.classList.add("d-none");
    }

    document.addEventListener("chat:reset", resetActiveChat);

    async function init() {
        await loadConversations();
        refreshList();
        await initSignalR();
    }

    init();

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

        messageInput.addEventListener("input", () => {
            clearTimeout(typingTimer);
            sendTypingState(true);
            typingTimer = setTimeout(() => sendTypingState(false), 1200);
        });
    }
})();
