import { GoogleGenerativeAI } from "@google/generative-ai";

async function initChatbot() {
    try {
        // 这里你应该从后端动态获取API Key
        const API_KEY = "AIzaSyAmdO999yK_FavK1FvaOr9kLGcj5fuomOA";  // 确保服务器正确传递该值
        console.log("API Key:", API_KEY);

        // 验证API Key是否正确传递
        if (!API_KEY || API_KEY === '@ViewBag.ApiKey') {  // 防止 API Key 未被正确替换
            console.error("API Key is missing.");
            return;
        }

        // 初始化生成模型
        const genAI = new GoogleGenerativeAI(API_KEY);
        const model = await genAI.getGenerativeModel({ model: "gemini-1.5-flash" });

        // 初始化聊天会话，必须由用户的消息开始
        let chatSession = model.startChat({
            history: [
                {
                    role: "user",
                    parts: [{
                        text: "I would like you to play the role of a gentle and compassionate mental health assistant. Your goal is to help me feel calm, supported, and reassured. Please respond with warmth, empathy, and positive energy."
                    }],
                },
                {
                    role: "model",
                    parts: [{
                        text: "Of course, I'm here for you. 😊 I'm your gentle mental health assistant, ready to listen and support you in any way I can. Please feel free to share whatever you're going through, and I'll be here to help you find peace and comfort."
                    }],
                }
            ]
        });


        document.getElementById('sendBtn').addEventListener('click', async () => {
            const userMessage = document.getElementById('userInput').value.trim();

            if (userMessage === '') {
                alert("输入内容不能为空！");
                return;
            }

            console.log("User Message:", userMessage);

            const chatBox = document.getElementById('chatBox');
            chatBox.innerHTML += `<div><strong>You:</strong> ${userMessage}</div>`;
            document.getElementById('userInput').value = '';

            try {
                // 发送用户的消息
                let result = await chatSession.sendMessage(userMessage);

                // 日志记录完整的返回值以调试
                console.log("API Response (Full Result):", result);

                // 检查返回的结构是否正确，防止未定义错误
                if (result && result.response && result.response.candidates && result.response.candidates[0] &&
                    result.response.candidates[0].content && result.response.candidates[0].content.parts &&
                    result.response.candidates[0].content.parts[0]) {

                    const responseText = result.response.candidates[0].content.parts[0].text;
                    chatBox.innerHTML += `<div><strong>Gemini:</strong> ${responseText}</div>`;
                } else {
                    chatBox.innerHTML += `<div><strong>Error:</strong> 无法获取有效的回复，请稍后再试。</div>`;
                }

                chatBox.scrollTop = chatBox.scrollHeight;
            } catch (error) {
                console.error("Error during API call:", error);
                chatBox.innerHTML += `<div><strong>Error:</strong> 无法获取回复，请稍后再试。</div>`;
            }
        });



    } catch (globalError) {
        console.error("Global Error:", globalError);
    }
}

// 启动聊天机器人
initChatbot();