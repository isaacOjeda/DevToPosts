// Chat functionality with vanilla JavaScript
class ChatApp {
    constructor() {
        this.conversationId = null;
        this.messageInput = document.getElementById('messageInput');
        this.sendButton = document.getElementById('sendButton');
        this.chatMessages = document.getElementById('chatMessages');
        this.loadingIndicator = document.getElementById('loadingIndicator');
        this.sendIcon = document.getElementById('sendIcon');
        this.sendText = document.getElementById('sendText');
        
        this.initializeChat();
        this.setupEventListeners();
    }

    async initializeChat() {
        try {
            // Create a new conversation when the page loads
            const response = await fetch('/api/agent/start', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const data = await response.json();
                this.conversationId = data.conversationId;
                console.log('New conversation started:', this.conversationId);
            } else {
                this.showError('Error al inicializar la conversación');
            }
        } catch (error) {
            console.error('Error initializing chat:', error);
            this.showError('Error de conexión al inicializar el chat');
        }
    }

    setupEventListeners() {
        // Send button click
        this.sendButton.addEventListener('click', () => this.sendMessage());
        
        // Enter key press
        this.messageInput.addEventListener('keypress', (e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                this.sendMessage();
            }
        });

        // Auto-focus input
        this.messageInput.focus();
    }

    async sendMessage() {
        const message = this.messageInput.value.trim();
        
        if (!message || !this.conversationId) {
            return;
        }

        // Disable input while processing
        this.setInputState(false);
        
        // Add user message to chat
        this.addMessageToChat(message, 'user');
        
        // Clear input
        this.messageInput.value = '';

        try {
            // Send message to API
            const response = await fetch('/api/agent/', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    question: message,
                    threadId: this.conversationId
                })
            });

            if (response.ok) {
                const data = await response.json();
                this.addMessageToChat(data.message, 'assistant');
            } else {
                const errorData = await response.json();
                this.showError(`Error: ${errorData.detail || 'Error desconocido'}`);
            }
        } catch (error) {
            console.error('Error sending message:', error);
            this.showError('Error de conexión. Intenta nuevamente.');
        } finally {
            // Re-enable input
            this.setInputState(true);
        }
    }

    addMessageToChat(message, sender) {
        const messageDiv = document.createElement('div');
        messageDiv.className = `message message-${sender}`;
        
        const bubbleDiv = document.createElement('div');
        bubbleDiv.className = `message-bubble ${sender}`;
        
        // Add icon for message sender
        const iconElement = document.createElement('i');
        if (sender === 'user') {
            iconElement.className = 'fas fa-user me-2';
        } else {
            iconElement.className = 'fas fa-robot me-2';
        }
        
        // Convert line breaks to <br> tags and preserve formatting
        const formattedMessage = this.formatMessage(message);
        
        // Create content with icon
        const contentDiv = document.createElement('div');
        contentDiv.appendChild(iconElement);
        contentDiv.innerHTML += formattedMessage;
        
        bubbleDiv.appendChild(contentDiv);
        
        const timeDiv = document.createElement('div');
        timeDiv.className = 'message-time';
        timeDiv.innerHTML = `<i class="fas fa-clock me-1"></i>${new Date().toLocaleTimeString('es-ES', { 
            hour: '2-digit', 
            minute: '2-digit' 
        })}`;
        
        messageDiv.appendChild(bubbleDiv);
        messageDiv.appendChild(timeDiv);
        this.chatMessages.appendChild(messageDiv);
        
        // Scroll to bottom
        this.scrollToBottom();
    }

    formatMessage(message) {
        // Convert line breaks to <br> tags
        let formatted = message.replace(/\n/g, '<br>');
        
        // Make bold text with ** ** or ?? markers more visible
        formatted = formatted.replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>');
        
        // Keep emojis and special formatting
        return formatted;
    }

    setInputState(enabled) {
        this.messageInput.disabled = !enabled;
        this.sendButton.disabled = !enabled;
        
        if (enabled) {
            this.loadingIndicator.style.display = 'none';
            this.sendIcon.className = 'fas fa-paper-plane';
            this.sendText.textContent = 'Enviar';
            this.messageInput.focus();
        } else {
            this.loadingIndicator.style.display = 'block';
            this.sendIcon.className = 'fas fa-spinner fa-spin';
            this.sendText.textContent = 'Enviando...';
        }
    }

    showError(errorMessage) {
        this.addMessageToChat(`<i class="fas fa-exclamation-triangle text-danger me-2"></i>${errorMessage}`, 'assistant');
    }

    scrollToBottom() {
        this.chatMessages.scrollTop = this.chatMessages.scrollHeight;
    }
}

// Initialize chat when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new ChatApp();
});

// Global site JavaScript (placeholder for future features)
console.log('Sistema de Gestión de Facturas - Chat iniciado');