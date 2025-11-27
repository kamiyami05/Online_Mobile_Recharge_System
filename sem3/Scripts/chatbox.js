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
                    ${isLoading ? renderTypingIndicator() : ''}
                    <div id="chatbox-end"></div>
                </div>
                <form id="chatbox-form" class="chatbox-form">
                    <input type="text" id="chatbox-input" placeholder="Ask about recharge, bills, or support..." required />
                    <button type="submit" id="chatbox-send-btn" class="chatbox-send-btn" ${isLoading ? 'disabled' : ''}>
                        <i class="ri-send-plane-fill"></i>
                    </button>
                </form>
            </div>
        `;

        container.html(buttonHtml + chatWindowHtml);

        // Scroll to bottom after render
        setTimeout(scrollToBottom, 50);

        $('#chatbox-toggle').off().on('click', handleToggle);
        $('#chatbox-form').off().on('submit', handleSend);

        // Auto-focus input when chatbox opens
        if (isOpen) {
            setTimeout(() => {
                $('#chatbox-input').focus();
            }, 400);
        }
    }

    function renderMessage(from, text, timestamp) {
        const isBot = from === 'bot';
        const time = timestamp ? formatTime(timestamp) : formatTime(new Date());

        return `
            <div class="message-row ${isBot ? 'justify-start' : 'justify-end'}">
                <div class="message-bubble ${isBot ? 'bot-bubble' : 'user-bubble'}">
                    <div class="message-content">${formatMessageText(text)}</div>
                    <div class="message-time">${time}</div>
                </div>
            </div>
        `;
    }

    function formatMessageText(text) {
        // Convert markdown-style formatting to HTML với line breaks
        return text
            .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
            .replace(/\n/g, '<br>')
            .replace(/(📱|💡|🏪|💰|🔧|👋|😊|⚡|✅|📍|💎|🛠️|⏰|💵|🤔|🚀|🇻🇳)/g, '<span style="font-size: 1.1em;">$1</span>');
    }

    function renderTypingIndicator() {
        return `
            <div class="message-row justify-start">
                <div class="typing-indicator">
                    <span class="typing-text">Assistant is typing</span>
                    <div class="typing-dots">
                        <div class="typing-dot"></div>
                        <div class="typing-dot"></div>
                        <div class="typing-dot"></div>
                    </div>
                </div>
            </div>
        `;
    }

    function formatTime(date) {
        return date.toLocaleTimeString('en-US', {
            hour: '2-digit',
            minute: '2-digit',
            hour12: false
        });
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

        // Add user message
        messages.push({
            from: 'user',
            text: input,
            timestamp: new Date()
        });

        inputElement.val('');
        renderChatbox();

        // Clear any existing timeout
        if (typingTimeout) clearTimeout(typingTimeout);

        try {
            const apiMessages = messages
                .filter(msg => msg.from === 'user' || msg.from === 'bot')
                .map(msg => ({
                    role: msg.from === 'bot' ? 'assistant' : 'user',
                    content: msg.text
                }));

            // Simulate AI "thinking" time - random between 1.5-3 seconds
            const thinkingTime = Math.random() * 1500 + 1500;

            typingTimeout = setTimeout(async () => {
                try {
                    const response = await fetch(API_URL, {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            'X-Requested-With': 'XMLHttpRequest'
                        },
                        body: JSON.stringify({ messages: apiMessages }),
                    });

                    if (!response.ok) {
                        throw new Error(`API error: ${response.status}`);
                    }

                    const data = await response.json();

                    // Add bot message with timestamp
                    messages.push({
                        from: 'bot',
                        text: data.reply.trim(),
                        timestamp: new Date()
                    });

                } catch (error) {
                    console.error('Error fetching chat response:', error);

                    // Fallback responses
                    let errorResponse = 'I apologize, but I\'m having trouble connecting right now. ';
                    errorResponse += 'You can ask me about: mobile recharges, bill payments, or account support.';

                    messages.push({
                        from: 'bot',
                        text: errorResponse,
                        timestamp: new Date()
                    });
                } finally {
                    isLoading = false;
                    renderChatbox();
                }
            }, thinkingTime);

        } catch (error) {
            console.error('Error in handleSend:', error);
            isLoading = false;
            renderChatbox();
        }
    }

    // Initialize chatbox
    renderChatbox();

    // Add some welcome effects
    setTimeout(() => {
        $('#chatbox-toggle').addClass('ready');
    }, 1000);
});