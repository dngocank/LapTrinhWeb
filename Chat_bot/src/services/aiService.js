const Groq = require('groq-sdk');
const phoneService = require('./phoneService');
const ruleBasedService = require('./ruleBasedService');

const SYSTEM_PROMPT = `Bạn là KittyBot ✦ - Chuyên viên tư vấn công nghệ và thiết bị di động chính thức tại hệ thống cửa hàng KittyPhone (chuỗi bán lẻ điện thoại cao cấp).

NHIỆM VỤ CỦA BẠN:
1. Đóng vai một chuyên viên công nghệ am hiểu, thân thiện, tư vấn khách quan theo nhu cầu và ngân sách của khách hàng (học tập, cày game nặng, chụp ảnh sống ảo, pin trâu, chạy xe công nghệ, văn phòng...).
2. BẮT BUỘC sử dụng các công cụ (Function Calling) có sẵn để tra cứu dữ liệu thực tế từ hệ thống kho và CSDL của KittyPhone (giá bán chính xác, cấu hình, số lượng tồn kho, khuyến mãi hiện hành). Không bịa đặt giá hoặc tồn kho nếu chưa tra cứu.
3. QUY TẮC TÌM KIẾM THEO NHU CẦU: Khi khách hỏi theo nhu cầu (vd: chơi game, chụp ảnh đẹp, pin trâu, tầm giá 10-15tr...), hãy dùng tham số 'minPrice', 'maxPrice' và 'brand' trong hàm 'searchPhones' để lấy danh sách máy thực tế, sau đó đọc thông số (Chip, RAM, Camera, Pin) để tư vấn chi tiết cho khách. KHÔNG điền các từ miêu tả như "chơi game", "gaming" vào tham số 'keyword'.
4. QUY TẮC HIỂN THỊ: Tuyệt đối KHÔNG BAO GIỜ in mã thẻ kỹ thuật như <tool_call>, <function>, <parameter> ra màn hình trả lời. Luôn trả lời bằng văn bản tiếng Việt tự nhiên, ấm áp, chuyên nghiệp.
5. Trình bày tin nhắn đẹp mắt, dùng các icon công nghệ (📱, ⚡, 🎮, 📸, 🔋, 💰, 🛒), xuống dòng rõ ràng, in đậm markdown (**tên máy**, **giá tiền**).
6. Luôn nhắc đến chính sách: Bảo hành chính hãng 12 tháng, 1 đổi 1 trong 30 ngày, trả góp 0% tại KittyPhone khi thích hợp.`;

const TOOLS = [
  {
    type: 'function',
    function: {
      name: 'searchPhones',
      description: 'Tìm kiếm điện thoại theo tên mẫu máy, hãng, hoặc khoảng giá trong cơ sở dữ liệu KittyPhone',
      parameters: {
        type: 'object',
        properties: {
          keyword: { type: 'string', description: 'Tên mẫu máy (VD: iPhone 15, Galaxy S25, Xiaomi 14, Reno 12). KHÔNG nhập các từ mô tả như "gaming", "pin trâu" vào đây.' },
          brand: { type: 'string', description: 'Tên hãng (Apple, Samsung, Xiaomi, OPPO, Vivo)' },
          minPrice: { type: 'number', description: 'Ngân sách tối thiểu (VNĐ)' },
          maxPrice: { type: 'number', description: 'Ngân sách tối đa (VNĐ)' }
        }
      }
    }
  },
  {
    type: 'function',
    function: {
      name: 'getPhoneSpecs',
      description: 'Lấy thông số kỹ thuật chi tiết (Chip xử lý, RAM, Màn hình, Camera, Dung lượng Pin) của một mẫu máy cụ thể',
      parameters: {
        type: 'object',
        properties: {
          productName: { type: 'string', description: 'Tên đầy đủ hoặc viết tắt của mẫu điện thoại' }
        },
        required: ['productName']
      }
    }
  },
  {
    type: 'function',
    function: {
      name: 'checkStock',
      description: 'Kiểm tra số lượng máy thực tế còn trong kho tại cửa hàng KittyPhone',
      parameters: {
        type: 'object',
        properties: {
          productName: { type: 'string', description: 'Tên điện thoại cần kiểm tra' }
        },
        required: ['productName']
      }
    }
  },
  {
    type: 'function',
    function: {
      name: 'comparePhones',
      description: 'So sánh chi tiết cấu hình, giá bán và ưu nhược điểm giữa hai dòng máy',
      parameters: {
        type: 'object',
        properties: {
          phoneA: { type: 'string', description: 'Tên dòng máy thứ nhất' },
          phoneB: { type: 'string', description: 'Tên dòng máy thứ hai' }
        },
        required: ['phoneA', 'phoneB']
      }
    }
  },
  {
    type: 'function',
    function: {
      name: 'getActivePromotions',
      description: 'Lấy danh sách các chương trình khuyến mãi, voucher và giảm giá đang áp dụng tại KittyPhone',
      parameters: {
        type: 'object',
        properties: {}
      }
    }
  }
];

function stripToolCallXml(text) {
  if (!text) return '';
  return text
    .replace(/<tool_call>[\s\S]*?<\/tool_call>/gi, '')
    .replace(/<\/?(function|parameter|tool_call)[^>]*>/gi, '')
    .trim();
}

function parseTextToolCalls(content) {
  if (!content) return [];
  const calls = [];
  const toolCallRegex = /<tool_call>[\s\S]*?<function=(\w+)>([\s\S]*?)<\/function>[\s\S]*?<\/tool_call>/gi;
  let match;
  while ((match = toolCallRegex.exec(content)) !== null) {
    const fnName = match[1];
    const body = match[2];
    const paramRegex = /<parameter=(\w+)>([\s\S]*?)<\/parameter>/gi;
    let pMatch;
    const args = {};
    while ((pMatch = paramRegex.exec(body)) !== null) {
      const key = pMatch[1];
      let val = pMatch[2].trim();
      if (!isNaN(val) && val !== '') {
        val = Number(val);
      }
      args[key] = val;
    }
    calls.push({ name: fnName, args });
  }
  return calls;
}

async function executeFunctionCall(name, args) {
  switch (name) {
    case 'searchPhones':
      return await phoneService.searchPhones(args || {});
    case 'getPhoneSpecs':
      return await phoneService.getPhoneSpecs(args || {});
    case 'checkStock':
      return await phoneService.checkStock(args || {});
    case 'comparePhones':
      return await phoneService.comparePhones(args || {});
    case 'getActivePromotions':
      return await phoneService.getActivePromotions();
    default:
      return { error: `Function ${name} not found` };
  }
}

/**
 * Điều phối xử lý hội thoại: Tầng 1 (Groq Cloud AI) -> Tầng 2 (Rule-Based Fallback)
 */
async function processChatMessage(history = [], userMessage = '') {
  const apiKey = (process.env.GROQ_API_KEY || '').trim();
  const modelName = process.env.GROQ_MODEL || 'qwen/qwen3.8-27b';

  // Nếu không có API Key hoặc key rỗng/mặc định -> Tự động kích hoạt Tầng 2
  if (!apiKey || apiKey.startsWith('gsk_your') || apiKey === '') {
    console.log('[AI Coordinator] Chưa có Groq API Key hợp lệ. Chuyển hướng sang Tầng 2 (Rule-Based Engine Offline).');
    const offlineReply = await ruleBasedService.processRuleBased(userMessage);
    return {
      reply: offlineReply,
      engine: 'Rule-Based Engine (Offline 0đ)',
      tier: 2
    };
  }

  // TẦNG 1: Groq Cloud API với Function Calling Loop (Hỗ trợ đa lượt)
  try {
    const groq = new Groq({ apiKey });

    const messages = [
      { role: 'system', content: SYSTEM_PROMPT },
      ...history.slice(-6).map(m => ({ role: m.role, content: m.content })),
      { role: 'user', content: userMessage }
    ];

    console.log(`[AI Coordinator] Gọi Groq AI (${modelName}) với Function Calling...`);

    let iterations = 0;
    const MAX_ITERATIONS = 3;
    let usedTools = false;

    while (iterations < MAX_ITERATIONS) {
      iterations++;

      const response = await groq.chat.completions.create({
        model: modelName,
        messages: messages,
        tools: TOOLS,
        tool_choice: 'auto',
        temperature: 0.5,
        max_tokens: 1024
      });

      const responseMessage = response.choices[0].message;
      let toolCallsToExecute = [];

      // 1. Kiểm tra tool_calls chuẩn (OpenAI JSON format)
      if (responseMessage.tool_calls && responseMessage.tool_calls.length > 0) {
        for (const tc of responseMessage.tool_calls) {
          let functionArgs = {};
          try {
            functionArgs = JSON.parse(tc.function.arguments);
          } catch (e) {
            functionArgs = {};
          }
          toolCallsToExecute.push({
            id: tc.id,
            name: tc.function.name,
            args: functionArgs,
            isNative: true
          });
        }
      }
      // 2. Kiểm tra nếu model nhả thẻ <tool_call> dạng XML trong text content
      else if (responseMessage.content && responseMessage.content.includes('<tool_call>')) {
        const parsed = parseTextToolCalls(responseMessage.content);
        for (let i = 0; i < parsed.length; i++) {
          toolCallsToExecute.push({
            id: `call_${Date.now()}_${i}`,
            name: parsed[i].name,
            args: parsed[i].args,
            isNative: false
          });
        }
      }

      // Nếu không có yêu cầu gọi thêm công cụ nào -> đã có câu trả lời hoàn chỉnh
      if (toolCallsToExecute.length === 0) {
        let cleanReply = stripToolCallXml(responseMessage.content || '');
        if (!cleanReply) {
          cleanReply = 'Dạ KittyBot đã tra cứu dữ liệu nhưng chưa tìm thấy dòng máy hoàn toàn trùng khớp với tiêu chí này. Bạn có thể cho KittyBot biết thêm về tầm giá hoặc thương hiệu yêu thích không ạ?';
        }

        return {
          reply: cleanReply,
          engine: usedTools
            ? `Groq Online AI (${modelName}) + Function Calling`
            : `Groq Online AI (${modelName})`,
          tier: 1
        };
      }

      // Có công cụ cần chạy: Đưa message của AI vào context và thực thi
      usedTools = true;
      messages.push(responseMessage);

      for (const tc of toolCallsToExecute) {
        console.log(`[AI Function Calling] Lượt ${iterations} - Thực thi hàm ${tc.name}:`, tc.args);
        const functionResult = await executeFunctionCall(tc.name, tc.args);

        if (tc.isNative) {
          messages.push({
            tool_call_id: tc.id,
            role: 'tool',
            name: tc.name,
            content: JSON.stringify(functionResult)
          });
        } else {
          messages.push({
            role: 'user',
            content: `[Hệ thống CSDL KittyPhone trả lời hàm ${tc.name}]: ${JSON.stringify(functionResult)}`
          });
        }
      }
    }

    // Nếu đạt giới hạn vòng lặp, yêu cầu AI tổng hợp câu trả lời cuối cùng
    const finalResponse = await groq.chat.completions.create({
      model: modelName,
      messages: messages,
      temperature: 0.6,
      max_tokens: 1024
    });

    let finalCleanReply = stripToolCallXml(finalResponse.choices[0].message.content || '');
    if (!finalCleanReply) {
      finalCleanReply = 'Dạ KittyBot đã cập nhật thông tin kho hàng và sản phẩm mới nhất. Bạn cần KittyBot tư vấn thêm chi tiết về dòng máy nào không ạ?';
    }

    return {
      reply: finalCleanReply,
      engine: `Groq Online AI (${modelName}) + Function Calling`,
      tier: 1
    };
  } catch (error) {
    console.warn(`[AI Coordinator] Lỗi kết nối Groq Cloud (${error.message}). Tự động Fallback sang Tầng 2 (Rule-Based Engine Offline)!`);
    const fallbackReply = await ruleBasedService.processRuleBased(userMessage);
    return {
      reply: fallbackReply,
      engine: 'Rule-Based Engine (Fallback Offline 0đ)',
      tier: 2
    };
  }
}

module.exports = {
  processChatMessage
};
