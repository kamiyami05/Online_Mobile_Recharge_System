$(document).ready(function () {
    const API_URL = '/Api/Chat';

    let messages = [{
        from: 'bot',
        text: '👋 Hello! I\'m your Recharge Assistant. I can help you with mobile recharges, bill payments, and answer any questions about our services!',
        timestamp: new Date()
    }];

    let isOpen = false;
    let isLoading = false;
    let typingTimeout = null;

    function renderChatbox() {
        const container = $('#chatbox-container');
        const buttonHtml = `
            <button id="chatbox-toggle" class="chatbox-toggle-btn">
                <i class="ri-message-3-line"></i>
            </button>
        `;

        const chatWindowHtml = `
            <div id="chatbox-window" class="chatbox-window ${isOpen ? 'open' : 'closed'}">
                <div class="chatbox-header">
                    <div class="chatbox-title">Recharge Assistant</div>
                    <div class="chatbox-subtitle">AI-powered support • Always online</div>
                </div>
                <div id="chatbox-messages" class="chatbox-messages">
                    ${messages.map(msg => renderMessage(msg.from, msg.text, msg.timestamp)).join('')}
                    ${isLoading ? renderThinking() : ''}
                    <div id="chatbox-end"></div>
                </div>
                <form id="chatbox-form" class="chatbox-form">
                    <input type="text" id="chatbox-input" placeholder="Ask about recharge or bills..." required/> 
                    <button type="submit" id="chatbox-send-btn" class="chatbox-send-btn" ${isLoading ? 'disabled' : ''}>
                        <i class="ri-send-plane-fill"></i>
                    </button>
                </form>
            </div>
        `;

        container.html(buttonHtml + chatWindowHtml);
        scrollToBottom();

        $('#chatbox-toggle').off().on('click', handleToggle);
        $('#chatbox-form').off().on('submit', handleSend);
    }

    function renderMessage(from, text, timestamp) {
        const isBot = from === 'bot';
        const timeString = formatTime(timestamp);
        const formattedText = formatMessageText(text);

        return `
            <div class="message-row ${isBot ? 'justify-start' : 'justify-end'}">
                ${isBot ? '<div class="message-avatar bot-avatar">🤖</div>' : ''}
                <div class="message-bubble ${isBot ? 'bot-bubble' : 'user-bubble'}">
                    <div class="message-content">${formattedText}</div>
                    <div class="message-time ${isBot ? 'bot-time' : 'user-time'}">${timeString}</div>
                </div>
                ${!isBot ? '<div class="message-avatar user-avatar">👤</div>' : ''}
            </div>
        `;
    }

    function formatMessageText(text) {
        // Xử lý định dạng số và danh sách
        let formattedText = text
            // Định dạng số với dấu chấm (1. 2. 3.)
            .replace(/(\d+)\.\s/g, '<strong>$1.</strong> ')
            // Định dạng dấu gạch đầu dòng
            .replace(/•\s/g, '• ')
            // Thay thế xuống dòng bằng thẻ <br>
            .replace(/\n/g, '<br>');

        return formattedText;
    }

    function renderThinking() {
        return `
            <div class="message-row justify-start">
                <div class="message-avatar bot-avatar">🤖</div>
                <div class="message-bubble bot-bubble">
                    <div class="typing-indicator">
                        <div class="typing-text">Assistant is thinking</div>
                        <div class="typing-dots">
                            <div class="typing-dot"></div>
                            <div class="typing-dot"></div>
                            <div class="typing-dot"></div>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    function formatTime(date) {
        if (!date) return '';

        const now = new Date();
        const messageDate = new Date(date);

        // Nếu tin nhắn được gửi hôm nay, hiển thị giờ:phút
        if (messageDate.toDateString() === now.toDateString()) {
            return messageDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        }

        // Nếu tin nhắn được gửi trước hôm nay, hiển thị ngày/tháng
        return messageDate.toLocaleDateString([], { day: '2-digit', month: '2-digit' });
    }

    function scrollToBottom() {
        const messagesDiv = document.getElementById('chatbox-messages');
        if (messagesDiv) {
            messagesDiv.scrollTop = messagesDiv.scrollHeight;
        }
    }

    function handleToggle() {
        isOpen = !isOpen;
        renderChatbox();
    }

    async function handleSend(e) {
        e.preventDefault();
        const inputElement = $('#chatbox-input');
        const input = inputElement.val().trim();
        if (!input || isLoading) return;

        isLoading = true;

        messages.push({ from: 'user', text: input, timestamp: new Date() });
        inputElement.val('');
        renderChatbox();

        const apiMessages = messages.map(msg => ({
            role: msg.from === 'bot' ? 'assistant' : 'user',
            content: msg.text
        }));

        try {
            const response = await fetch(API_URL, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ messages: apiMessages }),
            });

            if (!response.ok) {
                throw new Error(`API error: ${response.StatusCode}`);
            }

            const data = await response.json();
            const botMessage = { from: 'bot', text: data.reply.trim(), timestamp: new Date() };
            messages.push(botMessage);

        } catch (error) {
            console.error('Error fetching chat response:', error);
            messages.push({
                from: 'bot',
                text: 'Sorry, I am having trouble connecting. Please try again later.',
                timestamp: new Date()
            });
        } finally {
            isLoading = false;
            renderChatbox();
        }
    }
    renderChatbox();
    $('body').on('click', '#btnOpenChat', function (e) {
        e.preventDefault();
        openChat();
    });
});