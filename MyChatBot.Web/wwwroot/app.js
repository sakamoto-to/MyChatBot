// SignalR接続の設定
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .withAutomaticReconnect()
    .build();

let currentMessage = "";
let currentMessageElement = null;
let isReceiving = false;

// DOM要素
const chatMessages = document.getElementById("chatMessages");
const messageInput = document.getElementById("messageInput");
const sendButton = document.getElementById("sendButton");
const typingIndicator = document.getElementById("typingIndicator");
const providerInfo = document.getElementById("providerInfo");

// SignalRイベントハンドラー
connection.on("ReceiveMessageChunk", (chunk) => {
    if (!isReceiving) {
        isReceiving = true;
        typingIndicator.classList.remove("active");

        // 新しいボットメッセージ要素を作成
        const messageDiv = document.createElement("div");
        messageDiv.className = "message bot";

        const iconDiv = document.createElement("div");
        iconDiv.className = "message-icon";
        iconDiv.textContent = "🤖";

        const bubbleDiv = document.createElement("div");
        bubbleDiv.className = "message-bubble";

        messageDiv.appendChild(iconDiv);
        messageDiv.appendChild(bubbleDiv);
        chatMessages.appendChild(messageDiv);

        currentMessageElement = bubbleDiv;
        currentMessage = "";
    }

    currentMessage += chunk;
    currentMessageElement.textContent = currentMessage;
    chatMessages.scrollTop = chatMessages.scrollHeight;
});

connection.on("ReceiveMessageComplete", () => {
    isReceiving = false;
    currentMessage = "";
    currentMessageElement = null;
    sendButton.disabled = false;
    messageInput.disabled = false;
    messageInput.focus();
});

connection.on("ReceiveError", (error) => {
    isReceiving = false;
    typingIndicator.classList.remove("active");

    const errorDiv = document.createElement("div");
    errorDiv.className = "error-message";
    errorDiv.textContent = `エラー: ${error}`;
    chatMessages.appendChild(errorDiv);

    chatMessages.scrollTop = chatMessages.scrollHeight;
    sendButton.disabled = false;
    messageInput.disabled = false;
});

// メッセージ送信
async function sendMessage() {
    const message = messageInput.value.trim();
    if (!message) return;

    // ユーザーメッセージを表示
    const messageDiv = document.createElement("div");
    messageDiv.className = "message user";

    const iconDiv = document.createElement("div");
    iconDiv.className = "message-icon";
    iconDiv.textContent = "👤";

    const bubbleDiv = document.createElement("div");
    bubbleDiv.className = "message-bubble";
    bubbleDiv.textContent = message;

    messageDiv.appendChild(bubbleDiv);
    messageDiv.appendChild(iconDiv);
    chatMessages.appendChild(messageDiv);

    messageInput.value = "";
    chatMessages.scrollTop = chatMessages.scrollHeight;

    // ボタンを無効化
    sendButton.disabled = true;
    messageInput.disabled = true;
    typingIndicator.classList.add("active");

    try {
        await connection.invoke("SendMessage", message);
    } catch (error) {
        console.error("メッセージ送信エラー:", error);
        const errorDiv = document.createElement("div");
        errorDiv.className = "error-message";
        errorDiv.textContent = `送信エラー: ${error.message}`;
        chatMessages.appendChild(errorDiv);

        sendButton.disabled = false;
        messageInput.disabled = false;
        typingIndicator.classList.remove("active");
    }
}

// イベントリスナー
sendButton.addEventListener("click", sendMessage);
messageInput.addEventListener("keypress", (e) => {
    if (e.key === "Enter") {
        sendMessage();
    }
});

// 接続開始
connection.start()
    .then(() => {
        console.log("SignalR接続成功");
        providerInfo.textContent = "接続済み";

        // プロバイダー情報を取得（appsettings.jsonから）
        fetch("/appsettings.json")
            .then(res => res.json())
            .then(config => {
                const provider = config.Provider || "OpenAI";
                providerInfo.textContent = `📡 ${provider}`;
            })
            .catch(() => {
                providerInfo.textContent = "📡 接続済み";
            });
    })
    .catch(err => {
        console.error("SignalR接続エラー:", err);
        providerInfo.textContent = "接続エラー";

        const errorDiv = document.createElement("div");
        errorDiv.className = "error-message";
        errorDiv.textContent = "サーバーに接続できませんでした。appsettings.jsonにAPIキーが設定されているか確認してください。";
        chatMessages.appendChild(errorDiv);
    });

// 再接続ハンドラー
connection.onreconnecting(() => {
    providerInfo.textContent = "再接続中...";
    sendButton.disabled = true;
    messageInput.disabled = true;
});

connection.onreconnected(() => {
    providerInfo.textContent = "📡 接続済み";
    sendButton.disabled = false;
    messageInput.disabled = false;
});

connection.onclose(() => {
    providerInfo.textContent = "切断";
    sendButton.disabled = true;
    messageInput.disabled = true;
});
