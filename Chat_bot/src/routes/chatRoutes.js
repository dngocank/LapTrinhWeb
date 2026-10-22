const express = require('express');
const router = express.Router();
const aiService = require('../services/aiService');
const { checkHealth } = require('../config/db');

// POST /api/chat
router.post('/chat', async (req, res) => {
  try {
    const { message, history } = req.body;

    if (!message || typeof message !== 'string' || !message.trim()) {
      return res.status(400).json({
        success: false,
        error: 'Nội dung tin nhắn không được để trống'
      });
    }

    const result = await aiService.processChatMessage(history || [], message.trim());
    return res.json({
      success: true,
      reply: result.reply,
      engine: result.engine,
      tier: result.tier,
      timestamp: new Date().toISOString()
    });
  } catch (error) {
    console.error('Chat error:', error);
    return res.status(500).json({
      success: false,
      error: 'Có lỗi xảy ra trong quá trình xử lý tin nhắn.',
      details: error.message
    });
  }
});

// GET /api/health
router.get('/health', async (req, res) => {
  const dbStatus = await checkHealth();
  return res.json({
    status: 'OK',
    service: 'KittyBot AI Microservice',
    port: process.env.PORT || 3001,
    database: dbStatus ? 'Connected' : 'Disconnected',
    groqConfigured: !!(process.env.GROQ_API_KEY && !process.env.GROQ_API_KEY.startsWith('gsk_your')),
    model: process.env.GROQ_MODEL || 'llama-3.3-70b-versatile',
    timestamp: new Date().toISOString()
  });
});

module.exports = router;
