$(document).ready(function () {
    const API_URL = '/Api/Chat';

    let messages = [{
        from: 'bot',
        text: 'Hello! How can I assist you with your mobile recharge or bill payment today?'
    }];

    let isOpen = false;
    let isLoading = false;
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
                    <h3 class="chatbox-title">Recharge & Bill Payment Assistant</h3> </div>
                <div id="chatbox-messages" class="chatbox-messages">
                    ${messages.map(msg => renderMessage(msg.from, msg.text)).join('')}
                    ${isLoading ? renderLoading() : ''}
                    <div id="chatbox-end"></div>
                </div>
                <form id="chatbox-form" class="chatbox-form">
                    <input type="text" id="chatbox-input" placeholder="Ask about recharge or bills..." required/> <button type="submit" id="chatbox-send-btn" class="chatbox-send-btn" ${isLoading ? 'disabled' : ''}>
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

    function renderMessage(from, text) {
        const isBot = from === 'bot';
        return `
            <div class="message-row ${isBot ? 'justify-start' : 'justify-end'}">
                <div class="message-bubble ${isBot ? 'bot-bubble' : 'user-bubble'}">
                    ${text}
                </div>
            </div>
        `;
    }

    function renderLoading() {
        return `
            <div class="message-row justify-start">
                <div class="message-bubble bot-bubble loading-dots">...</div>
            </div>
        `;
    }
/
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

        messages.push({ from: 'user', text: input });
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
                throw new Error(`API error: ${response.statusText}`);
            }

            const data = await response.json();
            const botMessage = { from: 'bot', text: data.reply.trim() };
            messages.push(botMessage);

        } catch (error) {
            console.error('Error fetching chat response:', error);
            messages.push({ from: 'bot', text: 'Sorry, I am having trouble connecting. Please try again later.' });
        } finally {
            isLoading = false;
            renderChatbox();
        }
    }
    renderChatbox();
});