(function () {
    const sampleConversations = [
        { id: "user-1", name: "Alice Johnson", preview: "See you tomorrow!", time: "10:42 AM", unread: 2, online: true },
        { id: "user-2", name: "Bob Smith", preview: "Thanks for the update.", time: "Yesterday", unread: 0, online: false },
        { id: "group-1", name: "CS Team", preview: "Meeting at 3 PM", time: "Mon", unread: 5, online: null, isGroup: true },
        { id: "user-3", name: "Carol Lee", preview: "Got it 👍", time: "Sun", unread: 0, online: true }
    ];

    const listContainer = document.getElementById("chatListItems");
    const searchInput = document.getElementById("chatSearch");
    const emptyState = document.getElementById("chatListEmpty");

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

    function selectChat(chat, element) {
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
        if (placeholder) placeholder.classList.add("d-none");
        if (composer) composer.classList.remove("d-none");
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
})();
