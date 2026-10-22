(function () {
  'use strict';

  // API endpoint configuration
  const API_BASE = window.KITTYBOT_API_URL || 'http://localhost:3001';
  const CHAT_ENDPOINT = `${API_BASE}/api/chat`;

  let isChatOpen = false;
  let chatHistory = [];
  let isSending = false;

  // Simple Markdown Parser
  function parseMarkdown(text) {
    if (!text) return '';
    let escaped = text
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;');

    // Bold **text**
    escaped = escaped.replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>');
    // Italic *text*
    escaped = escaped.replace(/\*(.*?)\*/g, '<em>$1</em>');
    // Inline code `text`
    escaped = escaped.replace(/`([^`]+)`/g, '<code>$1</code>');

    // Bullet points
    const lines = escaped.split('\n');
    let inList = false;
    let result = [];

    for (let line of lines) {
      const trimmed = line.trim();
      if (trimmed.startsWith('- ') || trimmed.startsWith('• ')) {
        if (!inList) {
          result.push('<ul>');
          inList = true;
        }
        result.push(`<li>${trimmed.substring(2)}</li>`);
      } else {
        if (inList) {
          result.push('</ul>');
          inList = false;
        }
        if (trimmed === '') {
          result.push('<br/>');
        } else {
          result.push(`<p>${line}</p>`);
        }
      }
    }
    if (inList) result.push('</ul>');

    return result.join('');
  }

  // Create & Inject Widget DOM
  function initWidget() {
    if (document.getElementById('kittybot-launcher')) return;

    // 1. Launcher button
    const launcher = document.createElement('button');
    launcher.id = 'kittybot-launcher';
    launcher.setAttribute('aria-label', 'Mở trợ lý KittyBot AI');
    launcher.innerHTML = `
      <span class="kb-launcher-icon">💬</span>
      <span>Tư vấn công nghệ | <strong>KittyBot AI ✦</strong></span>
      <span class="kb-launcher-badge"></span>
    `;
    launcher.addEventListener('click', toggleChat);
    document.body.appendChild(launcher);

    // 2. Chat Container
    const container = document.createElement('div');
    container.id = 'kittybot-container';
    container.innerHTML = `
      <div class="kb-header">
        <div class="kb-header-left">
          <div class="kb-avatar">
            🤖
            <span class="kb-avatar-status"></span>
          </div>
          <div>
            <div class="kb-header-title">KittyBot AI ✦</div>
            <div class="kb-header-subtitle">Chuyên viên công nghệ KittyPhone</div>
          </div>
        </div>
        <div class="kb-header-actions">
          <button class="kb-header-btn" id="kb-btn-clear" title="Xóa lịch sử chat">🔄</button>
          <button class="kb-header-btn" id="kb-btn-close" title="Thu nhỏ">✕</button>
        </div>
      </div>

      <div class="kb-messages" id="kb-messages-list">
        <!-- Messages rendered here -->
      </div>

      <div class="kb-suggestions">
        <button class="kb-chip" data-query="Tư vấn điện thoại chơi game tầm 10-15 triệu">🎮 Máy cày game 10-15tr</button>
        <button class="kb-chip" data-query="Điện thoại chụp ảnh đẹp nhất hiện nay">📸 Chụp ảnh đẹp nhất</button>
        <button class="kb-chip" data-query="Có điện thoại nào pin trâu dưới 7 triệu không?">🔋 Pin trâu dưới 7tr</button>
        <button class="kb-chip" data-query="Cửa hàng có chương trình khuyến mãi hay voucher gì không?">🎁 Khuyến mãi & Voucher</button>
      </div>

      <div class="kb-footer">
        <div class="kb-input-wrap">
          <input type="text" class="kb-input" id="kb-user-input" placeholder="Hỏi về máy, cấu hình, tồn kho..." autocomplete="off" />
          <button class="kb-send-btn" id="kb-btn-send" title="Gửi câu hỏi">➤</button>
        </div>
        <div class="kb-watermark">⚡ KittyBot Dual-Engine: Online Groq AI ➔ Offline Fallback</div>
      </div>
    `;
    document.body.appendChild(container);

    // Event listeners
    document.getElementById('kb-btn-close').addEventListener('click', toggleChat);
    document.getElementById('kb-btn-clear').addEventListener('click', clearHistory);
    document.getElementById('kb-btn-send').addEventListener('click', handleUserSend);

    const input = document.getElementById('kb-user-input');
    input.addEventListener('keydown', function (e) {
      if (e.key === 'Enter') {
        e.preventDefault();
        handleUserSend();
      }
    });

    // Chips click
    container.querySelectorAll('.kb-chip').forEach(chip => {
      chip.addEventListener('click', function () {
        const query = this.getAttribute('data-query');
        if (query) {
          sendUserMessage(query);
        }
      });
    });

    // Initial Welcome Message
    appendBotMessage(
      `Chào bạn! Mình là **KittyBot ✦** - Chuyên viên tư vấn công nghệ tại **KittyPhone**.\n\nMình có thể hỗ trợ bạn:\n- 📱 **Gợi ý chọn máy** theo ngân sách và nhu cầu (Chơi game, chụp ảnh, pin trâu...)\n- ⚡ **Tra cứu cấu hình & tồn kho thực tế** tại cửa hàng\n- 🎁 **Cập nhật khuyến mãi & voucher giảm giá** mới nhất.\n\nBạn đang quan tâm dòng máy nào?`,
      'Hệ thống KittyPhone'
    );
  }

  function toggleChat() {
    isChatOpen = !isChatOpen;
    const container = document.getElementById('kittybot-container');
    if (container) {
      if (isChatOpen) {
        container.classList.add('kb-open');
        setTimeout(() => {
          const input = document.getElementById('kb-user-input');
          if (input) input.focus();
        }, 150);
      } else {
        container.classList.remove('kb-open');
      }
    }
  }

  function openChat(initialPrompt) {
    if (!isChatOpen) {
      toggleChat();
    }
    if (initialPrompt && initialPrompt.trim()) {
      setTimeout(() => {
        sendUserMessage(initialPrompt.trim());
      }, 300);
    }
  }

  function clearHistory() {
    chatHistory = [];
    const list = document.getElementById('kb-messages-list');
    if (list) {
      list.innerHTML = '';
      appendBotMessage(
        `Đã làm mới cuộc trò chuyện. Bạn cần KittyBot hỗ trợ dòng máy hoặc ngân sách nào ạ?`,
        'KittyBot'
      );
    }
  }

  function appendUserMessage(text) {
    const list = document.getElementById('kb-messages-list');
    if (!list) return;

    const row = document.createElement('div');
    row.className = 'kb-msg-row kb-user';
    row.innerHTML = `
      <div class="kb-msg-bubble">
        <p>${text.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')}</p>
        <div class="kb-msg-meta">${new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</div>
      </div>
    `;
    list.appendChild(row);
    scrollToBottom();
  }

  function appendBotMessage(markdownText, engineInfo) {
    const list = document.getElementById('kb-messages-list');
    if (!list) return;

    const row = document.createElement('div');
    row.className = 'kb-msg-row kb-bot';
    row.innerHTML = `
      <div class="kb-msg-avatar">🤖</div>
      <div class="kb-msg-bubble">
        ${parseMarkdown(markdownText)}
        <div class="kb-msg-meta">
          <span>${new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</span>
          ${engineInfo ? `<span class="kb-engine-tag">${engineInfo}</span>` : ''}
        </div>
      </div>
    `;
    list.appendChild(row);
    scrollToBottom();
  }

  function showTypingIndicator() {
    const list = document.getElementById('kb-messages-list');
    if (!list) return;

    const row = document.createElement('div');
    row.id = 'kb-typing-indicator';
    row.className = 'kb-msg-row kb-bot';
    row.innerHTML = `
      <div class="kb-msg-avatar">🤖</div>
      <div class="kb-typing">
        <span class="kb-typing-dot"></span>
        <span class="kb-typing-dot"></span>
        <span class="kb-typing-dot"></span>
      </div>
    `;
    list.appendChild(row);
    scrollToBottom();
  }

  function removeTypingIndicator() {
    const indicator = document.getElementById('kb-typing-indicator');
    if (indicator) indicator.remove();
  }

  function scrollToBottom() {
    const list = document.getElementById('kb-messages-list');
    if (list) {
      list.scrollTop = list.scrollHeight;
    }
  }

  async function sendUserMessage(text) {
    if (!text || isSending) return;
    isSending = true;

    appendUserMessage(text);
    chatHistory.push({ role: 'user', content: text });

    const input = document.getElementById('kb-user-input');
    const sendBtn = document.getElementById('kb-btn-send');
    if (input) input.value = '';
    if (sendBtn) sendBtn.disabled = true;

    showTypingIndicator();

    try {
      const response = await fetch(CHAT_ENDPOINT, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          message: text,
          history: chatHistory
        })
      });

      removeTypingIndicator();

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}`);
      }

      const data = await response.json();
      if (data && data.reply) {
        appendBotMessage(data.reply, data.engine || 'KittyBot Dual-Engine');
        chatHistory.push({ role: 'assistant', content: data.reply });
      } else {
        appendBotMessage('Xin lỗi bạn, KittyBot chưa nhận được câu trả lời. Vui lòng thử lại nhé!', 'Lỗi hệ thống');
      }
    } catch (err) {
      removeTypingIndicator();
      console.warn('Lỗi gọi API chat:', err);
      appendBotMessage(
        '⚠️ Hiện tại chưa kết nối được với máy chủ KittyBot (Port 3001). Bạn vui lòng kiểm tra xem dịch vụ chatbot đã được khởi động chưa nhé!',
        'Lỗi mạng'
      );
    } finally {
      isSending = false;
      if (sendBtn) sendBtn.disabled = false;
      if (input) input.focus();
    }
  }

  function handleUserSend() {
    const input = document.getElementById('kb-user-input');
    if (input && input.value.trim()) {
      sendUserMessage(input.value.trim());
    }
  }

  // Expose global methods
  window.kittyOpenChat = openChat;
  window.kittyToggleChat = toggleChat;

  // Auto initialize on DOMContentLoaded
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initWidget);
  } else {
    initWidget();
  }
})();
