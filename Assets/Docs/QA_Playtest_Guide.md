# Ký Ức Sài Gòn - QA Playtest Guide

## 1) Mục tiêu
Tài liệu này dùng để team test bản prototype Unity 6 (URP, third-person), gồm:
- Flow đầy đủ từ BusHub -> 6 khu ký ức -> Ending.
- Tính đúng của tương tác, puzzle, restore effect, scene loading.
- Chế độ Developer Mode để mở full map nhanh khi test.

## 2) Thiết lập trước khi test
1. Mở project Unity.
2. Mở `Scene_00_BusHub`.
3. Vào menu `Ky Uc Sai Gon`:
   - Bật `Developer Mode` nếu cần test nhanh full route.
   - Tắt `Developer Mode` nếu test flow game chuẩn.
4. Nhấn Play.

## 3) Điều khiển
- `WASD`: di chuyển.
- `Chuột`: xoay camera third-person.
- `E`: tương tác.
- Tâm `+` ở giữa màn hình dùng để ngắm đối tượng tương tác.

## 4) Quy ước kết quả mong đợi
- Mỗi scene ký ức có flow:
  1) Tương tác NPC / item mở nhiệm vụ
  2) Giải puzzle
  3) Restore scene + nhận memory fragment
  4) Mở bus stop quay về Hub
- UI luôn có:
  - Memory progress `x/6`
  - Objective hiện tại
  - Prompt tương tác khi ở gần

## 5) Checklist tổng quát theo scene

### Scene_00_BusHub
- Player spawn đúng trong xe, đi lại không kẹt.
- Camera ngước lên được để nhìn bảng route/trần/đối tượng cao.
- Tương tác nút route bằng `E` hoạt động.
- Màu route:
  - Chưa mở: xám
  - Mở/chưa restore: vàng
  - Đã restore: xanh
- `Developer Mode ON`: mở full route + ending để test nhanh.
- `Developer Mode OFF`: logic mở khóa bình thường.

### Scene_01_NguyenHue_Tutorial
- Puzzle loa mở được.
- Nhập đúng `1-6-8` thì restore.
- Bus stop hiện sau restore và về Hub được.

### Scene_02_BenThanh
- Nhặt nón lá -> trả NPC -> puzzle giỏ trái cây hiện.
- Không bị kẹt nhân vật khi puzzle xuất hiện.
- Đáp án đúng: `1-3-2`.
- Restore xong mở bus stop về Hub.

### Scene_03_DinhDocLap
- Nhặt map -> trả NPC -> mở puzzle radio.
- Đáp án đúng: `1975`.
- Restore xong objective đổi về bus stop.

### Scene_04_NhaThoDucBa
- Nói chuyện NPC -> nhặt chuông -> trả NPC -> mở puzzle.
- Đáp án đúng: `La-Sol-Re-Mi-Si-Do`.
- Restore xong bồ câu hoạt hóa + bus stop mở.

### Scene_05_Bitexco
- Nhặt thẻ nhân viên -> nói chuyện NPC -> mở puzzle server.
- Đáp án đúng: `345`.
- Restore xong dải đèn/tone sáng hơn + bus stop mở.

### Scene_06_BachDang
- Nói chuyện bác lái tàu -> nhặt vé tàu -> trả NPC -> mở puzzle.
- Đáp án đúng: `28`.
- Restore xong đèn bờ sông bật + thuyền bob nhẹ + bus stop mở.

### Scene_07_Ending
- Nếu chưa đủ 6 fragment (và Developer Mode OFF):
  - Hiện: `Bạn chưa thu thập đủ ký ức.`
  - Tự quay về BusHub.
- Nếu đủ 6 fragment hoặc Developer Mode ON:
  - 6 shard sáng lần lượt.
  - Landmark 81 sáng.
  - Hiện text kết thúc.
  - Return trigger mở, `E` để về BusHub.

## 6) Test nhanh đề xuất

### A. Smoke test (10-15 phút)
1. Bật Developer Mode.
2. Từ BusHub vào ngẫu nhiên 2-3 scene.
3. Test tương tác `E`, mở puzzle, đóng puzzle, quay về Hub.
4. Vào Ending, xác nhận sequence chạy.

### B. Regression test chuẩn (30-60 phút)
1. Tắt Developer Mode.
2. Chạy đúng thứ tự game, gom đủ 6 mảnh ký ức.
3. Check memory progress tăng đúng từng scene.
4. Check ending chỉ mở sau khi đủ 6/6.

## 7) Các lỗi cần log nếu gặp
- Không hiện prompt tương tác dù đứng gần.
- Bấm `E` không mở puzzle / không load scene.
- Puzzle Submit/Close không bấm được.
- Nhân vật bị kẹt do collider object spawn.
- Camera bị khóa góc, không nhìn được landmark cao.
- Restore xong nhưng memory progress không tăng.
- Bus stop không xuất hiện sau restore.
- Console có compile error hoặc exception runtime.

## 8) Ghi chú cho tester
- Đây là graybox prototype, chỉ dùng primitives.
- Không đánh giá chất lượng art cuối.
- Tập trung vào gameplay flow, logic, tương tác, và độ ổn định.
