// ============================================================
// SupportBot Chat UI - Vanilla JavaScript
// ============================================================

(function () {
    'use strict';

    // ── DOM Elements ──────────────────────────────────────────
    const chatForm = document.getElementById('chat-form');
    const messageInput = document.getElementById('message-input');
    const btnSend = document.getElementById('btn-send');
    const sendIcon = document.getElementById('send-icon');
    const sendSpinner = document.getElementById('send-spinner');
    const messagesContainer = document.getElementById('messages-container');
    const messagesDiv = document.getElementById('messages');
    const typingIndicator = document.getElementById('typing-indicator');
    const welcomeScreen = document.getElementById('welcome-screen');
    const btnNewChat = document.getElementById('btn-new-chat');
    const btnThemeToggle = document.getElementById('btn-theme-toggle');

    // ── State ─────────────────────────────────────────────────
    let sessionId = localStorage.getItem('supportbot-session') || generateSessionId();
    let isLoading = false;

    // ── Initialize ────────────────────────────────────────────
    initTheme();
    messageInput.focus();
    saveSessionId(sessionId);

    // ── Event Listeners ───────────────────────────────────────
    chatForm.addEventListener('submit', function (e) {
        e.preventDefault();
        handleSend();
    });

    messageInput.addEventListener('input', function () {
        btnSend.disabled = !messageInput.value.trim() || isLoading;
        autoResizeTextarea();
    });

    messageInput.addEventListener('keydown', function (e) {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            handleSend();
        }
    });

    btnNewChat.addEventListener('click', handleNewChat);
    btnThemeToggle.addEventListener('click', toggleTheme);

    // Expose for quick prompt buttons in the HTML
    window.sendQuickMessage = function (text) {
        messageInput.value = text;
        handleSend();
    };

    // ── Core Chat Logic ───────────────────────────────────────

    async function handleSend() {
        const text = messageInput.value.trim();
        if (!text || isLoading) return;

        // Hide welcome screen on first message
        hideWelcomeScreen();

        // Clear input
        messageInput.value = '';
        btnSend.disabled = true;
        autoResizeTextarea();

        // Add user message to the UI
        appendMessage('user', text);

        // Show typing indicator
        setLoading(true);

        try {
            const response = await fetch('/chat', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    sessionId: sessionId,
                    message: text
                })
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText || `Error ${response.status}`);
            }

            const data = await response.json();
            appendMessage('bot', data.reply);
        } catch (error) {
            appendError(error.message || 'Error de conexión. Intenta de nuevo.');
        } finally {
            setLoading(false);
            messageInput.focus();
        }
    }

    async function handleNewChat() {
        // Delete current session on the server
        try {
            await fetch(`/chat/${encodeURIComponent(sessionId)}`, {
                method: 'DELETE'
            });
        } catch (_) {
            // Ignore errors — session might not exist
        }

        // Generate new session
        sessionId = generateSessionId();
        saveSessionId(sessionId);

        // Clear messages and show welcome screen
        messagesDiv.innerHTML = '';
        showWelcomeScreen();

        messageInput.value = '';
        messageInput.focus();
    }

    // ── Message Rendering ─────────────────────────────────────

    function appendMessage(role, text) {
        const wrapper = document.createElement('div');
        wrapper.className = 'message-bubble flex ' + (role === 'user' ? 'justify-end' : 'justify-start items-start gap-3');

        if (role === 'bot') {
            wrapper.innerHTML = `
                <div class="flex-shrink-0 w-8 h-8 rounded-lg bg-brand-100 dark:bg-brand-900/40 flex items-center justify-center mt-0.5">
                    <svg class="w-4 h-4 text-brand-600 dark:text-brand-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                              d="M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-5 5v-5z" />
                    </svg>
                </div>
                <div class="bot-content max-w-[85%] sm:max-w-[75%] px-4 py-3 rounded-2xl rounded-tl-sm
                            bg-white dark:bg-gray-800 border border-gray-100 dark:border-gray-700
                            text-gray-800 dark:text-gray-200 text-sm leading-relaxed shadow-sm">
                    ${formatBotMessage(text)}
                </div>
            `;
        } else {
            wrapper.innerHTML = `
                <div class="max-w-[85%] sm:max-w-[75%] px-4 py-3 rounded-2xl rounded-tr-sm
                            bg-brand-600 text-white text-sm leading-relaxed shadow-sm">
                    ${escapeHtml(text)}
                </div>
            `;
        }

        messagesDiv.appendChild(wrapper);
        scrollToBottom();
    }

    function appendError(text) {
        const wrapper = document.createElement('div');
        wrapper.className = 'message-bubble error-message flex justify-center';
        wrapper.innerHTML = `
            <div class="inline-flex items-center gap-2 px-4 py-2 rounded-xl
                        bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800
                        text-red-600 dark:text-red-400 text-sm">
                <svg class="w-4 h-4 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                          d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                <span>${escapeHtml(text)}</span>
            </div>
        `;
        messagesDiv.appendChild(wrapper);
        scrollToBottom();
    }

    // ── Text Formatting ───────────────────────────────────────

    function formatBotMessage(text) {
        // Simple Markdown-like formatting for bot messages
        let html = escapeHtml(text);

        // Bold: **text**
        html = html.replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>');

        // Inline code: `code`
        html = html.replace(/`(.*?)`/g, '<code>$1</code>');

        // Unordered lists: lines starting with - or *
        html = html.replace(/^[\-\*]\s+(.+)$/gm, '<li>$1</li>');
        html = html.replace(/(<li>.*<\/li>\n?)+/g, '<ul class="list-disc">$&</ul>');

        // Ordered lists: lines starting with number.
        html = html.replace(/^\d+\.\s+(.+)$/gm, '<li>$1</li>');

        // Paragraphs: double newlines
        html = html.replace(/\n\n/g, '</p><p>');

        // Single newlines to <br>
        html = html.replace(/\n/g, '<br>');

        // Wrap in paragraph
        html = '<p>' + html + '</p>';

        // Clean up empty paragraphs
        html = html.replace(/<p>\s*<\/p>/g, '');

        return html;
    }

    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    // ── UI Helpers ────────────────────────────────────────────

    function setLoading(loading) {
        isLoading = loading;
        btnSend.disabled = loading || !messageInput.value.trim();
        messageInput.disabled = loading;

        sendIcon.classList.toggle('hidden', loading);
        sendSpinner.classList.toggle('hidden', !loading);
        typingIndicator.classList.toggle('hidden', !loading);

        if (loading) {
            scrollToBottom();
        }
    }

    function scrollToBottom() {
        requestAnimationFrame(() => {
            messagesContainer.scrollTop = messagesContainer.scrollHeight;
        });
    }

    function autoResizeTextarea() {
        // Only needed for browsers without field-sizing support
        if (!CSS.supports('field-sizing', 'content')) {
            messageInput.style.height = 'auto';
            messageInput.style.height = Math.min(messageInput.scrollHeight, 144) + 'px';
        }
    }

    function hideWelcomeScreen() {
        if (welcomeScreen && welcomeScreen.parentNode) {
            welcomeScreen.classList.add('fade-out');
            welcomeScreen.addEventListener('animationend', function () {
                welcomeScreen.remove();
            }, { once: true });
        }
    }

    function showWelcomeScreen() {
        // Re-create the welcome screen
        const welcome = document.createElement('div');
        welcome.id = 'welcome-screen';
        welcome.className = 'flex flex-col items-center justify-center text-center py-16 sm:py-24 message-bubble';
        welcome.innerHTML = `
            <div class="w-16 h-16 rounded-2xl bg-brand-100 dark:bg-brand-900/40 flex items-center justify-center mb-6">
                <svg class="w-8 h-8 text-brand-600 dark:text-brand-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
                          d="M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-5 5v-5z" />
                </svg>
            </div>
            <h2 class="text-xl font-semibold text-gray-900 dark:text-white mb-2">Bienvenido a SupportBot</h2>
            <p class="text-gray-500 dark:text-gray-400 max-w-md text-sm leading-relaxed mb-8">
                Soy tu asistente de soporte técnico interno. Puedo ayudarte con temas de accesos, facturación, reportes y más.
            </p>
            <div class="grid grid-cols-1 sm:grid-cols-3 gap-3 w-full max-w-lg">
                <button onclick="sendQuickMessage('¿Cómo puedo restablecer mi contraseña?')"
                        class="quick-prompt px-4 py-3 text-sm text-left rounded-xl border border-gray-200 dark:border-gray-700
                               bg-white dark:bg-gray-800/50 text-gray-700 dark:text-gray-300
                               hover:border-brand-300 dark:hover:border-brand-600 hover:bg-brand-50 dark:hover:bg-brand-900/20
                               transition-all duration-150">
                    <span class="block text-xs text-gray-400 dark:text-gray-500 mb-1">Accesos</span>
                    ¿Cómo restablecer mi contraseña?
                </button>
                <button onclick="sendQuickMessage('¿Cómo genero una factura?')"
                        class="quick-prompt px-4 py-3 text-sm text-left rounded-xl border border-gray-200 dark:border-gray-700
                               bg-white dark:bg-gray-800/50 text-gray-700 dark:text-gray-300
                               hover:border-brand-300 dark:hover:border-brand-600 hover:bg-brand-50 dark:hover:bg-brand-900/20
                               transition-all duration-150">
                    <span class="block text-xs text-gray-400 dark:text-gray-500 mb-1">Facturación</span>
                    ¿Cómo genero una factura?
                </button>
                <button onclick="sendQuickMessage('¿Qué tipos de reportes hay disponibles?')"
                        class="quick-prompt px-4 py-3 text-sm text-left rounded-xl border border-gray-200 dark:border-gray-700
                               bg-white dark:bg-gray-800/50 text-gray-700 dark:text-gray-300
                               hover:border-brand-300 dark:hover:border-brand-600 hover:bg-brand-50 dark:hover:bg-brand-900/20
                               transition-all duration-150">
                    <span class="block text-xs text-gray-400 dark:text-gray-500 mb-1">Reportes</span>
                    ¿Qué reportes hay disponibles?
                </button>
            </div>
        `;
        messagesDiv.appendChild(welcome);
    }

    // ── Session Management ────────────────────────────────────

    function generateSessionId() {
        // Readable session ID: user-{random}
        return 'user-' + crypto.randomUUID().split('-')[0];
    }

    function saveSessionId(id) {
        localStorage.setItem('supportbot-session', id);
    }

    // ── Theme Management ──────────────────────────────────────

    function initTheme() {
        const stored = localStorage.getItem('supportbot-theme');
        if (stored === 'dark' || (!stored && window.matchMedia('(prefers-color-scheme: dark)').matches)) {
            document.documentElement.classList.add('dark');
        } else {
            document.documentElement.classList.remove('dark');
        }
    }

    function toggleTheme() {
        const isDark = document.documentElement.classList.toggle('dark');
        localStorage.setItem('supportbot-theme', isDark ? 'dark' : 'light');
    }

})();
