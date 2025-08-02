// ---- Shared helpers for both the page and the floating widget ----
async function callChatApi(question) {
    const res = await fetch('/api/chat', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ question })
    });

    // Read as text first, then try to parse
    const text = await res.text();
    let data;
    try { data = JSON.parse(text); }
    catch { throw new Error(text); }

    if (!res.ok) {
        throw new Error(data.error || data.detail || 'Chat failed');
    }
    return data.answer;
}

function makeBubble(role, content) {
    const div = document.createElement('div');
    div.className =
        "rounded-xl p-3 text-sm border border-white/10 " +
        (role === 'user' ? "bg-slate-800" : "bg-slate-900");
    div.innerHTML =
        `<div class="opacity-70 text-xs mb-1">${role}</div><div>${content}</div>`;
    return div;
}

// ---- Floating widget (Shared/_Layout.cshtml) ----
function toggleChat() {
    const panel = document.getElementById('chat-panel');
    if (!panel) return;
    panel.classList.toggle('hidden');
}

async function sendChat() {
    const input = document.getElementById('chat-input');
    const msgs = document.getElementById('chat-messages');
    const errBox = document.getElementById('chat-error');
    if (!input || !msgs || !errBox) return;

    const q = (input.value || '').trim();
    if (!q) return;
    input.value = '';

    errBox.classList.add('hidden'); errBox.textContent = '';
    msgs.appendChild(makeBubble('user', q));
    msgs.scrollTop = msgs.scrollHeight;

    try {
        const a = await callChatApi(q);
        msgs.appendChild(makeBubble('assistant', a));
        msgs.scrollTop = msgs.scrollHeight;
    } catch (e) {
        errBox.textContent = (e && e.message) ? e.message : String(e);
        errBox.classList.remove('hidden');
    }
}

// ---- Page (/Views/Chat/Index.cshtml) hook, if you want to reuse helpers ----
async function sendChatPage() {
    const input = document.getElementById('chat-input-page');
    const msgs = document.getElementById('chat-messages-page');
    const errBox = document.getElementById('chat-error-page');
    if (!input || !msgs || !errBox) return;

    const q = (input.value || '').trim();
    if (!q) return;
    input.value = '';

    errBox.classList.add('hidden'); errBox.textContent = '';
    msgs.appendChild(makeBubble('user', q));
    msgs.scrollTop = msgs.scrollHeight;

    try {
        const a = await callChatApi(q);
        msgs.appendChild(makeBubble('assistant', a));
        msgs.scrollTop = msgs.scrollHeight;
    } catch (e) {
        errBox.textContent = (e && e.message) ? e.message : String(e);
        errBox.classList.remove('hidden');
    }
}
