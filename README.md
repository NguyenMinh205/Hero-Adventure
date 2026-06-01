# Hero's Adventure

## 📖 Giới thiệu (Introduction)
**Hero's Adventure** là một tựa game nhập vai chiến thuật (RPG) kết hợp giải đố Nối ngọc (Line-Matching) 2D được phát triển trên nền tảng Unity. Trong game, người chơi sẽ hóa thân thành một anh hùng, khám phá các vùng đất mới, và chiến đấu với kẻ thù thông qua cơ chế nối ngọc đầy thú vị và mang tính chiến thuật cao.

---

## 🎮 Các tính năng chính (Main Features)
- **Đồ họa 2D & m thanh sống động**: Game sử dụng thiết kế nhân vật 2D đẹp mắt, hiệu ứng chiến đấu và cuộn nền (background scrolling) mượt mà trong quá trình di chuyển (Explore). m thanh đa dạng từ nhạc nền chiến đấu thường, đánh Boss cho đến tiếng bước chân.
- **Hai chế độ chơi hấp dẫn (Game Modes)**:
  - **Level Mode (Theo ải)**: Vượt qua từng đợt quái và đánh Boss cuối cùng.
  - **Endless Mode (Vô tận)**: Chiến đấu sinh tồn liên tục với độ khó tăng dần để đạt điểm số cao nhất.

---

## ⚔️ Cơ chế Gameplay (Gameplay Mechanics)

### 1. Hệ thống chiến đấu theo lượt (Turn-based Combat)
- Trận đấu diễn ra theo dạng Turn-based. 
- **Lượt của người chơi (Player Turn)**: Người chơi có một số **Điểm Hành Động (Action Points - AP)** nhất định (mặc định là 5 điểm). Mỗi lần di chuyển/ghép ngọc sẽ tiêu hao 1 AP. Khi hết AP, lượt sẽ chuyển sang cho kẻ thù.
- **Lượt của kẻ thù (Enemy Turn)**: Kẻ thù sẽ lần lượt tung đòn tấn công vào người chơi.

### 2. Cơ chế Giải đố Nối ngọc (Line-Matching System)
Thay vì sử dụng các đòn đánh thông thường, người chơi phải nối các viên ngọc trên lưới (GameGrid) để kích hoạt các hành động khác nhau:
- 🗡️ **Damage Gem (Ngọc Sát thương)**: Tấn công kẻ thù mục tiêu.
- 💖 **Health Gem (Ngọc Hồi máu)**: Hồi phục lượng máu (HP) cho nhân vật.
- 🛡️ **Shield Gem (Ngọc Khiên)**: Tạo lớp giáp bảo vệ chặn sát thương.
- 🎯 **Crit Rate Gem (Ngọc Tỉ lệ bạo kích)**: Tăng phần trăm tỉ lệ đánh chí mạng.
- 💥 **Crit Damage Gem (Ngọc Sát thương bạo kích)**: Tăng lượng sát thương chí mạng gây ra.
- 💨 **Dodge Gem (Ngọc Né tránh)**: Tăng tỉ lệ né đòn của kẻ địch.

### 3. Combo & Multiplier (Nhân phẩm & Kỹ năng)
- Nối được **2 viên ngọc** cùng loại sẽ kích hoạt hiệu ứng cơ bản.
- Nếu nối được **nhiều hơn 2 viên** (3, 4 viên...), sức mạnh của hành động sẽ được **nhân lên (Multiplier)** (mặc định tăng thêm 25% hiệu quả cho mỗi viên ngọc nối thêm), tạo ra các đòn tấn công uy lực hoặc lượng hồi phục khổng lồ.

---

## 🚀 Hướng dẫn cách chơi (How to play)
1. **Bắt đầu cuộc phiêu lưu**: Ngay khi chọn chế độ chơi (Level hoặc Endless), nhân vật của bạn sẽ tự động chạy thám hiểm. Hãy tận hưởng khung cảnh cho đến khi chạm trán kẻ thù và bước vào trận chiến.
2. **Thao tác nối ngọc (Line-Matching)**:
   - **Kéo nối ngọc**: Chạm (click) vào một viên ngọc và kéo rê (drag) qua các viên ngọc giống nó nằm liền kề (ngang, dọc, chéo) để tạo thành một đường nối nối liền chúng.
   - **Điều kiện**: Bạn phải nối được một chuỗi liên tiếp có **từ 2 viên ngọc cùng loại trở lên**. Khi thả tay ra, chuỗi ngọc sẽ biến mất và kích hoạt kỹ năng.
   - **Mỗi lần nối ngọc thành công**, bạn sẽ mất 1 Điểm Hành Động (AP).
3. **Kích hoạt Kỹ năng & Chọn mục tiêu**:
   - Khi bạn nối thành công **Ngọc Sát Thương (Damage Gem - 🗡️)**, trò chơi sẽ yêu cầu bạn chọn mục tiêu. Hãy **nhấn (click) vào kẻ thù** mà bạn muốn tấn công, nhân vật sẽ lập tức tung đòn sát thương vào mục tiêu đó.
   - Với các loại ngọc hỗ trợ (Hồi máu, Khiên, Buff chỉ số...), hiệu ứng sẽ tự động kích hoạt thẳng lên nhân vật của bạn.
4. **Chiến thuật & Lựa chọn**: 
   - Ưu tiên tiêu diệt các kẻ thù có lượng máu thấp hoặc sát thương cao trước để giảm bớt áp lực trong lượt của địch.
   - Luôn chú ý thanh máu (HP) của mình, đừng ngần ngại ghép ngọc Hồi máu hoặc Khiên để phòng thủ.
5. **Tiến trình**: Tiêu diệt toàn bộ kẻ thù để vượt qua Wave đó và tiếp tục hành trình. Trò chơi kết thúc (Game Over) khi máu của nhân vật bằng 0.

---

## 🛠️ Công cụ & Công nghệ sử dụng (Tech Stack)
- **Game Engine**: Unity (C#)
- **Design Pattern**: Singleton, Observer, Object Pooling, Strategy Pattern (trong Game Modes).

---

> *Dự án được xây dựng với tình yêu dành cho thể loại RPG & Line-Matching. Chúc các bạn chơi game vui vẻ!*
