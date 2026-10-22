# CellphoneS Design System Guidelines

Tài liệu hướng dẫn Design System, Token màu sắc, Typography và Quy chuẩn trải nghiệm người dùng Storefront & Dashboard theo nhận diện thương hiệu chuỗi bán lẻ công nghệ **CellphoneS** (https://cellphones.com.vn/).

---

## 1. Thông Tin Nhận Diện Thương Hiệu
- **Thương hiệu**: CellphoneS
- **Trang web mẫu**: https://cellphones.com.vn/
- **Phong cách thị giác**: Sạch sẽ, hiện đại, tối ưu cho thương mại điện tử, thân thiện người dùng thực tế.
- **Tone & Voice**: Ngắn gọn, chuyên nghiệp, đáng tin cậy.

---

## 2. Design Tokens Cốt Lõi

### 2.1. Bảng Màu (Color Tokens)
| Tên Token | Mã Màu / Giá Trị | Mục Đích Sử Dụng |
|---|---|---|
| `--cps-red` | `#d70018` | Màu đỏ thương hiệu CellphoneS, Header bar, CTA, Giá tiền nổi bật |
| `--cps-red-dark` | `#b80014` | Hover trạng thái nút bấm đỏ, active state |
| `--cps-red-light` | `#fee2e2` | Nền badge ưu đãi, highlight thông báo |
| `--text-primary` | `#18181b` | Tiêu đề sản phẩm, chữ chính, độ tương phản cao |
| `--text-secondary` | `#71717a` | Giá cũ gạch ngang, nhãn phụ, thông tin đánh giá |
| `--text-tertiary` | `#1d1d20` | Văn bản bổ trợ |
| `--bg-page` | `#f4f6f8` | Màu nền toàn trang (xám dịu mắt chống chói) |
| `--bg-card` | `#ffffff` | Nền thẻ sản phẩm, panel, modal |
| `--border-color` | `#e5e7eb` | Đường viền thẻ sản phẩm, ngăn cách menu |
| `--badge-discount` | `#d70018` | Tag giảm giá góc trái ảnh sản phẩm |
| `--badge-speed` | `#0284c7` | Nhãn giao hàng "2 Giờ" / Siêu tốc |
| `--star-rating` | `#f59e0b` | Sao đánh giá sản phẩm |

### 2.2. Typography (Phông Chữ Inter Chuẩn)
- **Primary Font Family**: `'Inter', Inter Fallback, Arial, sans-serif`
- **Base Size**: `14px`, `line-height: 21px`, `font-weight: 600 - 700` cho tiêu đề, `400 - 500` cho nội dung.
- **Typography Scale**:
  - `font.size.xs`: `12px` (Nhãn phụ, ghi chú Smember, giao 2h)
  - `font.size.sm`: `14px` (Tên sản phẩm, nội dung thông thường, menu)
  - `font.size.md`: `16px` (Giá bán hiện tại, tiêu đề tab)
  - `font.size.lg`: `20px` - `24px` (Tiêu đề danh mục, tiêu đề khối Hot Sale)

### 2.3. Quy Chuẩn Thẻ Sản Phẩm (Product Card)
Thẻ sản phẩm chuẩn CellphoneS được thiết kế tự nhiên, không cứng nhắc:
1. **Container**:
   - `background: #ffffff`
   - `border: 1px solid #e5e7eb`
   - `border-radius: 12px`
   - `box-shadow: 0 1px 3px rgba(0,0,0,0.06)`
   - `padding: 10px 10px 12px`
   - `transition: all 0.25s ease`
   - Hover: `transform: translateY(-3px); box-shadow: 0 8px 16px rgba(0,0,0,0.08); border-color: #d70018;`
2. **Badges**:
   - Tag giảm giá: Góc trên trái, chữ trắng nền đỏ `#d70018`, bo góc 4px.
   - Tag trả góp 0%: Nền xám viền mỏng hoặc nhãn xanh nhạt.
3. **Ảnh Sản Phẩm**:
   - Tỷ lệ vuông hoặc 160px - 180px, căn giữa, nền trong suốt/trắng, `object-fit: contain`.
4. **Tiêu Đề**:
   - 14px, `font-weight: 600`, màu `#18181b`, giới hạn tối đa 2 dòng (line-clamp: 2).
5. **Giá Cả**:
   - Giá khuyến mãi: Đỏ đậm `#d70018`, 16px, `font-weight: 700`.
   - Giá niêm yết: Xám `#71717a`, 12px, gạch ngang `text-decoration: line-through`.
6. **Khung Khuyến Mại / Ưu Đãi Phụ**:
   - Khung xám nhạt bo 4px, font 11px: "Smember giảm thêm đến 1%", "Học sinh sinh viên giảm 100K"...
7. **Footer Thẻ**:
   - Badge "2 Giờ" hoặc "Giao nhanh", sao đánh giá `★ 5.0`, và icon trái tim yêu thích.
