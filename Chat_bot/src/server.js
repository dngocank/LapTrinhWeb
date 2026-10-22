const express = require('express');
const cors = require('cors');
const path = require('path');
const dotenv = require('dotenv');

dotenv.config({ path: path.join(__dirname, '../.env') });

const chatRoutes = require('./routes/chatRoutes');
const { getPool, checkHealth } = require('./config/db');

const app = express();
const PORT = process.env.PORT || 3001;

// Middleware
app.use(cors({ origin: '*' }));
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// Serve static files (public)
app.use(express.static(path.join(__dirname, '../public')));

// Routes
app.use('/api', chatRoutes);

// Root route redirects or serves index.html
app.get('/', (req, res) => {
  res.sendFile(path.join(__dirname, '../public/index.html'));
});

// Start server
async function startServer() {
  console.log('========================================================');
  console.log('  ĐANG KHỞI ĐỘNG KITTYBOT AI MICROSERVICE (PORT ' + PORT + ')');
  console.log('========================================================');

  // Thử kết nối Database
  try {
    await getPool();
    const isDbOk = await checkHealth();
    console.log(`[SQL Server] Trạng thái PhoneStoreDB: ${isDbOk ? '✅ SẴN SÀNG' : '⚠️ CHƯA KẾT NỐI'}`);
  } catch (err) {
    console.warn('[SQL Server] Cảnh báo kết nối ban đầu:', err.message);
  }

  app.listen(PORT, () => {
    console.log(`[KittyBot Server] Đang chạy tại: http://localhost:${PORT}`);
    console.log(`[Health Check] http://localhost:${PORT}/api/health`);
    console.log(`[Test UI]     http://localhost:${PORT}/index.html`);
    console.log('========================================================\n');
  });
}

startServer();
