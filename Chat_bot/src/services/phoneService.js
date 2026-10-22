const { executeQuery } = require('../config/db');

function formatCurrency(amount) {
  if (amount == null) return '0đ';
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
}

/**
 * Trích xuất các thông số kỹ thuật chính từ văn bản description
 */
function extractSpecsFromDescription(desc) {
  if (!desc) return {};
  const specs = {};

  const lines = desc.split('\n');
  for (const line of lines) {
    const trimmed = line.trim();
    if (trimmed.includes('Màn hình') || trimmed.includes('Màn hình chính')) {
      specs.screen = trimmed.replace(/^[-*•\s]+/, '');
    } else if (trimmed.includes('Vi xử lý') || trimmed.includes('CPU') || trimmed.includes('Chip')) {
      specs.cpu = trimmed.replace(/^[-*•\s]+/, '');
    } else if (trimmed.includes('RAM') || trimmed.includes('Bộ nhớ')) {
      specs.ram = trimmed.replace(/^[-*•\s]+/, '');
    } else if (trimmed.includes('Camera')) {
      if (!specs.camera) specs.camera = trimmed.replace(/^[-*•\s]+/, '');
      else specs.camera += ' | ' + trimmed.replace(/^[-*•\s]+/, '');
    } else if (trimmed.includes('Pin') || trimmed.includes('Sạc')) {
      specs.battery = trimmed.replace(/^[-*•\s]+/, '');
    }
  }

  return specs;
}

/**
 * 1. searchPhones: Tìm máy theo từ khóa, thương hiệu, khoảng giá
 */
async function searchPhones({ keyword = '', brand = '', minPrice, maxPrice, limit = 8 }) {
  try {
    let sql = `
      SELECT TOP (@limit) 
        p.product_id, p.product_name, b.brand_name, c.category_name, 
        p.selling_price, p.quantity, p.image_url, p.description
      FROM Product p
      JOIN Brand b ON p.brand_id = b.brand_id
      JOIN Category c ON p.category_id = c.category_id
      WHERE p.is_active = 1
    `;
    const params = { limit: parseInt(limit, 10) || 8 };

    if (keyword && keyword.trim()) {
      sql += ` AND (p.product_name LIKE @kw OR p.description LIKE @kw)`;
      params.kw = `%${keyword.trim()}%`;
    }

    if (brand && brand.trim()) {
      sql += ` AND b.brand_name LIKE @br`;
      params.br = `%${brand.trim()}%`;
    }

    if (minPrice != null && !isNaN(minPrice)) {
      sql += ` AND p.selling_price >= @minP`;
      params.minP = parseFloat(minPrice);
    }

    if (maxPrice != null && !isNaN(maxPrice)) {
      sql += ` AND p.selling_price <= @maxP`;
      params.maxP = parseFloat(maxPrice);
    }

    sql += ` ORDER BY CASE WHEN c.category_id = 1 THEN 0 ELSE 1 END, p.selling_price ASC`;

    let rows = await executeQuery(sql, params);

    // Fallback thông minh: Nếu tìm theo từ khóa không thấy kết quả (VD: model điền "gaming", "chơi game", "pin trâu"),
    // nhưng có tiêu chí khoảng giá hoặc hãng, tự động tìm theo khoảng giá/hãng để vẫn có sản phẩm tư vấn cho khách
    if (rows.length === 0 && keyword && (minPrice != null || maxPrice != null || brand)) {
      let fallbackSql = `
        SELECT TOP (@limit) 
          p.product_id, p.product_name, b.brand_name, c.category_name, 
          p.selling_price, p.quantity, p.image_url, p.description
        FROM Product p
        JOIN Brand b ON p.brand_id = b.brand_id
        JOIN Category c ON p.category_id = c.category_id
        WHERE p.is_active = 1
      `;
      const fallbackParams = { limit: parseInt(limit, 10) || 8 };

      if (brand && brand.trim()) {
        fallbackSql += ` AND b.brand_name LIKE @br`;
        fallbackParams.br = `%${brand.trim()}%`;
      }
      if (minPrice != null && !isNaN(minPrice)) {
        fallbackSql += ` AND p.selling_price >= @minP`;
        fallbackParams.minP = parseFloat(minPrice);
      }
      if (maxPrice != null && !isNaN(maxPrice)) {
        fallbackSql += ` AND p.selling_price <= @maxP`;
        fallbackParams.maxP = parseFloat(maxPrice);
      }
      fallbackSql += ` ORDER BY CASE WHEN c.category_id = 1 THEN 0 ELSE 1 END, p.selling_price ASC`;
      rows = await executeQuery(fallbackSql, fallbackParams);
    }

    return rows.map(r => ({
      productId: r.product_id,
      productName: r.product_name,
      brand: r.brand_name,
      category: r.category_name,
      price: r.selling_price,
      priceFormatted: formatCurrency(r.selling_price),
      quantity: r.quantity,
      inStock: r.quantity > 0,
      imageUrl: r.image_url,
      specs: extractSpecsFromDescription(r.description),
      overview: (r.description || '').substring(0, 160)
    }));
  } catch (err) {
    console.error('searchPhones error:', err.message);
    return [];
  }
}

/**
 * 2. getPhoneSpecs: Lấy thông số kỹ thuật chi tiết của 1 mẫu máy
 */
async function getPhoneSpecs({ productName }) {
  try {
    if (!productName) return null;
    const sql = `
      SELECT TOP 1 
        p.product_id, p.product_name, b.brand_name, c.category_name,
        p.selling_price, p.quantity, p.image_url, p.description
      FROM Product p
      JOIN Brand b ON p.brand_id = b.brand_id
      JOIN Category c ON p.category_id = c.category_id
      WHERE p.is_active = 1 AND p.product_name LIKE @pName
      ORDER BY CASE WHEN p.product_name = @rawName THEN 0 ELSE 1 END, p.product_id ASC
    `;
    const rows = await executeQuery(sql, {
      pName: `%${productName.trim()}%`,
      rawName: productName.trim()
    });

    if (rows.length === 0) return null;
    const r = rows[0];
    return {
      productId: r.product_id,
      productName: r.product_name,
      brand: r.brand_name,
      category: r.category_name,
      price: r.selling_price,
      priceFormatted: formatCurrency(r.selling_price),
      quantity: r.quantity,
      inStock: r.quantity > 0,
      imageUrl: r.image_url,
      descriptionRaw: r.description,
      specs: extractSpecsFromDescription(r.description)
    };
  } catch (err) {
    console.error('getPhoneSpecs error:', err.message);
    return null;
  }
}

/**
 * 3. checkStock: Kiểm tra tồn kho của sản phẩm
 */
async function checkStock({ productName }) {
  try {
    if (!productName) return [];
    const sql = `
      SELECT TOP 5 
        p.product_id, p.product_name, b.brand_name, p.quantity, p.selling_price, p.image_url
      FROM Product p
      JOIN Brand b ON p.brand_id = b.brand_id
      WHERE p.is_active = 1 AND p.product_name LIKE @pName
      ORDER BY p.quantity DESC
    `;
    const rows = await executeQuery(sql, { pName: `%${productName.trim()}%` });
    return rows.map(r => ({
      productId: r.product_id,
      productName: r.product_name,
      brand: r.brand_name,
      quantity: r.quantity,
      status: r.quantity > 5 ? 'Sẵn hàng dồi dào' : (r.quantity > 0 ? `Còn ít hàng (${r.quantity} máy)` : 'Tạm hết hàng'),
      priceFormatted: formatCurrency(r.selling_price),
      imageUrl: r.image_url
    }));
  } catch (err) {
    console.error('checkStock error:', err.message);
    return [];
  }
}

/**
 * 4. comparePhones: So sánh 2 dòng điện thoại
 */
async function comparePhones({ phoneA, phoneB }) {
  try {
    const [specA, specB] = await Promise.all([
      getPhoneSpecs({ productName: phoneA }),
      getPhoneSpecs({ productName: phoneB })
    ]);

    return {
      phoneA: specA || { productName: phoneA, notFound: true },
      phoneB: specB || { productName: phoneB, notFound: true }
    };
  } catch (err) {
    console.error('comparePhones error:', err.message);
    return { error: 'Không thể so sánh thông tin do lỗi truy vấn' };
  }
}

/**
 * 5. getActivePromotions: Lấy các chương trình khuyến mãi hiện hành
 */
async function getActivePromotions() {
  try {
    const sql = `
      SELECT promotion_id, promotion_name, discount_percent, start_date, end_date, description
      FROM Promotion
      WHERE end_date >= CAST(GETDATE() AS DATE)
      ORDER BY discount_percent DESC
    `;
    const rows = await executeQuery(sql);
    if (rows.length === 0) {
      // Fallback nếu không có CTKM khớp ngày
      const fallbackRows = await executeQuery(`SELECT TOP 5 promotion_id, promotion_name, discount_percent, start_date, end_date, description FROM Promotion ORDER BY promotion_id DESC`);
      return fallbackRows.map(r => ({
        id: r.promotion_id,
        name: r.promotion_name,
        discount: `${r.discount_percent}%`,
        description: r.description
      }));
    }
    return rows.map(r => ({
      id: r.promotion_id,
      name: r.promotion_name,
      discount: `${r.discount_percent}%`,
      startDate: r.start_date,
      endDate: r.end_date,
      description: r.description
    }));
  } catch (err) {
    console.error('getActivePromotions error:', err.message);
    return [];
  }
}

module.exports = {
  searchPhones,
  getPhoneSpecs,
  checkStock,
  comparePhones,
  getActivePromotions,
  formatCurrency
};
