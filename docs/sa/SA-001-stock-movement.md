# SA-001 : เอกสารวิเคราะห์ระบบบันทึกความเคลื่อนไหวสินค้าคงคลัง

อ้างอิง: [BRD-001](../brd/BRD-001-stock-movement.md) v1.0 | วันที่วิเคราะห์ 2026-09-12

---

## 0. สารบัญส่งมอบ (SA Deliverables)

| # | เอกสาร | ใช้ทำอะไรต่อ |
|---|---|---|
| 1 | Requirement Mapping | ผูก FR ของ BRD เข้ากับ UC/US/TC เพื่อตรวจความครบถ้วน |
| 2 | Use Case Specification | ให้ dev เห็นลำดับการทำงานและ exception ทุกเส้นทาง |
| 3 | User Story + Acceptance Criteria | ต้นฉบับของ Jira Story และเกณฑ์ปิดงาน |
| 4 | Flow Diagram | สื่อสาร decision point ที่ซับซ้อนให้ทีม |
| 5 | API Contract & Data Model | ต้นฉบับของ Jira Subtask ฝั่ง BE/DB |
| 6 | Test Case | ต้นฉบับของ Jira Subtask ฝั่ง QA |
| 7 | Traceability Matrix | ใช้ตรวจว่าทุก FR มีการ์ด Jira รองรับ |

---

## 1. Requirement Mapping

| FR ID | Use Case | User Story | Test Case | Jira Story |
|---|---|---|---|---|
| FR-001 | UC-001 | US-001 | TC-001 ถึง TC-004 | `[FR-001] รับสินค้าเข้าคลัง` |
| FR-002 | UC-002 | US-002 | TC-005 ถึง TC-009 | `[FR-002] เบิกสินค้าออกจากคลัง` |
| FR-003 | UC-003 | US-003 | TC-010 ถึง TC-012 | `[FR-003] ดูยอดคงเหลือและประวัติ` |
| FR-004 | UC-004 | US-004 | TC-013 ถึง TC-015 | `[FR-004] แจ้งเตือนสินค้าใกล้หมด` |

---

## 2. Use Case Specification

### UC-001 บันทึกรับสินค้าเข้าคลัง

```
Use Case ID  : UC-001
Use Case Name: บันทึกรับสินค้าเข้าคลัง
Actor        : เจ้าหน้าที่คลัง
Description  : บันทึกการรับสินค้าเข้าคลังและเพิ่มยอดคงเหลือของสินค้า

Precondition :
  - ผู้ใช้เข้าสู่ระบบและมีสิทธิ์บันทึกรายการ
  - สินค้าที่จะรับมีอยู่ในระบบและมีสถานะ Active

Main Flow:
  1. เจ้าหน้าที่คลัง เลือกสินค้า ระบุจำนวน และเลขที่เอกสารอ้างอิง
  2. System ตรวจสอบว่าจำนวนเป็นจำนวนเต็มบวกมากกว่า 0 (BL-002)
  3. System ตรวจสอบว่าสินค้ามีสถานะ Active (BL-006)
  4. เจ้าหน้าที่คลัง ยืนยันการบันทึก
  5. System เพิ่มยอดคงเหลือ และบันทึกประวัติความเคลื่อนไหวพร้อมผู้ทำรายการและเวลา (BL-003)
  6. System แสดงผลสำเร็จพร้อมยอดคงเหลือใหม่

Exception Flow:
  2a. จำนวนไม่ถูกต้อง (0, ติดลบ, ทศนิยม):
      1. System แสดง error "จำนวนต้องเป็นจำนวนเต็มบวกมากกว่า 0"
      2. กลับไปที่ Step 1
  3a. สินค้าไม่มีอยู่จริงหรือถูกปิดใช้งาน:
      1. System แสดง error "ไม่พบสินค้าที่ใช้งานอยู่"
      2. กลับไปที่ Step 1

Postcondition:
  - ยอดคงเหลือของสินค้าเพิ่มขึ้นตามจำนวนที่รับ
  - มีประวัติความเคลื่อนไหวประเภท Receive 1 รายการ
```

### UC-002 บันทึกเบิกสินค้าออกจากคลัง

```
Use Case ID  : UC-002
Use Case Name: บันทึกเบิกสินค้าออกจากคลัง
Actor        : เจ้าหน้าที่คลัง
Description  : ตัดยอดคงเหลือเมื่อมีการเบิกสินค้า โดยต้องไม่ทำให้ยอดคงเหลือติดลบ

Precondition :
  - ผู้ใช้เข้าสู่ระบบและมีสิทธิ์บันทึกรายการ
  - สินค้ามีสถานะ Active และมียอดคงเหลือมากกว่า 0

Main Flow:
  1. เจ้าหน้าที่คลัง เลือกสินค้า ระบุจำนวนที่เบิก และชื่อผู้ขอเบิก
  2. System ตรวจสอบว่าจำนวนเป็นจำนวนเต็มบวกมากกว่า 0 (BL-002)
  3. System ตรวจสอบว่ายอดคงเหลือเพียงพอ (BL-001)
  4. เจ้าหน้าที่คลัง ยืนยันการเบิก
  5. System ตัดยอดคงเหลือภายใน Transaction เดียว (NFR-004)
  6. System บันทึกประวัติความเคลื่อนไหวประเภท Issue พร้อมผู้ทำรายการและเวลา
  7. System ตรวจสอบยอดคงเหลือใหม่เทียบกับจุดสั่งซื้อ แล้วส่งต่อ UC-004 ถ้าเข้าเงื่อนไข
  8. System แสดงผลสำเร็จพร้อมยอดคงเหลือใหม่

Alternative Flow:
  7a. ยอดคงเหลือใหม่มากกว่าจุดสั่งซื้อ:
      1. System ข้ามการแจ้งเตือน
      2. ไปที่ Step 8

Exception Flow:
  2a. จำนวนไม่ถูกต้อง:
      1. System แสดง error "จำนวนต้องเป็นจำนวนเต็มบวกมากกว่า 0"
      2. กลับไปที่ Step 1
  3a. ยอดคงเหลือไม่เพียงพอ:
      1. System แสดง error "ยอดคงเหลือไม่เพียงพอ คงเหลือ {n} หน่วย"
      2. กลับไปที่ Step 1
  5a. มีการเบิกพร้อมกันจนยอดไม่พอระหว่างบันทึก (Race Condition):
      1. System ยกเลิก Transaction ทั้งหมด
      2. System แสดง error "ยอดคงเหลือเปลี่ยนแปลง กรุณาทำรายการใหม่"
      3. กลับไปที่ Step 1

Postcondition:
  - ยอดคงเหลือลดลงตามจำนวนที่เบิก และไม่ติดลบ
  - มีประวัติความเคลื่อนไหวประเภท Issue 1 รายการ
```

### UC-003 ดูยอดคงเหลือและประวัติความเคลื่อนไหว

```
Use Case ID  : UC-003
Use Case Name: ดูยอดคงเหลือและประวัติความเคลื่อนไหว
Actor        : เจ้าหน้าที่คลัง
Description  : แสดงยอดคงเหลือปัจจุบันและประวัติรับ-เบิกย้อนหลังของสินค้า

Precondition :
  - ผู้ใช้เข้าสู่ระบบแล้ว

Main Flow:
  1. เจ้าหน้าที่คลัง เลือกสินค้าและระบุช่วงวันที่ที่ต้องการดู
  2. System ดึงยอดคงเหลือปัจจุบันของสินค้า
  3. System ดึงประวัติความเคลื่อนไหวในช่วงวันที่ เรียงจากใหม่ไปเก่า
  4. System แสดงยอดคงเหลือและรายการประวัติแบบแบ่งหน้า

Alternative Flow:
  1a. ไม่ระบุช่วงวันที่:
      1. System ใช้ค่าเริ่มต้นเป็น 30 วันย้อนหลัง
      2. ไปที่ Step 2

Exception Flow:
  1a. วันที่เริ่มต้นมากกว่าวันที่สิ้นสุด:
      1. System แสดง error "ช่วงวันที่ไม่ถูกต้อง"
      2. กลับไปที่ Step 1
  3a. ไม่มีประวัติในช่วงที่เลือก:
      1. System แสดงข้อความ "ไม่พบรายการในช่วงวันที่ที่เลือก"
      2. ไปที่ Step 4

Postcondition:
  - ไม่มีการเปลี่ยนแปลงข้อมูลในระบบ
```

### UC-004 แจ้งเตือนสินค้าใกล้หมด

```
Use Case ID  : UC-004
Use Case Name: แจ้งเตือนสินค้าใกล้หมด
Actor        : System (ถูกเรียกจาก UC-002)
Description  : สร้างการแจ้งเตือนให้หัวหน้าคลังเมื่อยอดคงเหลือต่ำกว่าหรือเท่ากับจุดสั่งซื้อ

Precondition :
  - มีการตัดสต็อกสำเร็จจาก UC-002

Main Flow:
  1. System อ่านยอดคงเหลือใหม่และจุดสั่งซื้อของสินค้า (BL-005)
  2. System เปรียบเทียบยอดคงเหลือใหม่กับจุดสั่งซื้อ
  3. System สร้างรายการแจ้งเตือนสถานะ Unread ให้หัวหน้าคลัง
  4. หัวหน้าคลัง เปิดดูรายการแจ้งเตือนและกดรับทราบ
  5. System เปลี่ยนสถานะการแจ้งเตือนเป็น Read

Alternative Flow:
  3a. มีการแจ้งเตือนสถานะ Unread ของสินค้าเดิมอยู่แล้ว:
      1. System ไม่สร้างรายการซ้ำ แต่อัปเดตยอดคงเหลือล่าสุดในรายการเดิม
      2. จบการทำงาน

Exception Flow:
  1a. สินค้าไม่ได้กำหนดจุดสั่งซื้อ:
      1. System ใช้ค่าเริ่มต้น 10 หน่วย
      2. ไปที่ Step 2

Postcondition:
  - มีรายการแจ้งเตือนสำหรับสินค้าที่ยอดคงเหลือถึงจุดสั่งซื้อ
```

---

## 3. User Story และ Acceptance Criteria

```
User Story ID : US-001
Title         : บันทึกรับสินค้าเข้าคลัง

As a     : เจ้าหน้าที่คลัง
I want to: บันทึกการรับสินค้าเข้าคลังพร้อมเลขที่เอกสารอ้างอิง
So that  : ยอดคงเหลือในระบบตรงกับของจริงและตรวจสอบย้อนหลังได้

Acceptance Criteria:
  - [ ] บันทึกรับสินค้าสำเร็จแล้วยอดคงเหลือเพิ่มขึ้นตามจำนวนที่ระบุ
  - [ ] ระบบบันทึกผู้ทำรายการและเวลาโดยอัตโนมัติ ผู้ใช้แก้ไขไม่ได้
  - [ ] ระบุจำนวนเป็น 0 ติดลบ หรือทศนิยม ระบบปฏิเสธพร้อมข้อความชัดเจน
  - [ ] ระบุสินค้าที่ไม่มีอยู่หรือถูกปิดใช้งาน ระบบปฏิเสธด้วย HTTP 404

Story Points: 3
Priority    : High
```

```
User Story ID : US-002
Title         : เบิกสินค้าออกจากคลัง

As a     : เจ้าหน้าที่คลัง
I want to: เบิกสินค้าออกจากคลังโดยระบบตรวจยอดคงเหลือให้
So that  : ไม่เกิดการเบิกเกินจำนวนที่มีอยู่จริง

Acceptance Criteria:
  - [ ] เบิกสำเร็จแล้วยอดคงเหลือลดลงตามจำนวนที่เบิก
  - [ ] เบิกเกินยอดคงเหลือ ระบบปฏิเสธด้วย HTTP 422 และแจ้งยอดคงเหลือปัจจุบัน
  - [ ] ยอดคงเหลือไม่ติดลบในทุกกรณี รวมถึงการเบิกพร้อมกันหลายรายการ
  - [ ] การตัดสต็อกและการบันทึกประวัติอยู่ใน Transaction เดียว ล้มเหลวแล้ว rollback ทั้งคู่
  - [ ] ยอดคงเหลือใหม่ถึงจุดสั่งซื้อแล้วเกิดการแจ้งเตือนตาม US-004

Story Points: 5
Priority    : High
```

```
User Story ID : US-003
Title         : ดูยอดคงเหลือและประวัติความเคลื่อนไหว

As a     : เจ้าหน้าที่คลัง
I want to: ดูยอดคงเหลือปัจจุบันและประวัติรับ-เบิกของสินค้า
So that  : ตรวจสอบความถูกต้องของสต็อกและหาสาเหตุผลต่างได้

Acceptance Criteria:
  - [ ] แสดงยอดคงเหลือปัจจุบันและประวัติเรียงจากใหม่ไปเก่า แบ่งหน้าได้
  - [ ] ไม่ระบุช่วงวันที่ ระบบใช้ค่าเริ่มต้น 30 วันย้อนหลัง
  - [ ] วันที่เริ่มต้นมากกว่าวันที่สิ้นสุด ระบบปฏิเสธด้วย HTTP 400
  - [ ] ผลรวมของประวัติรับหักด้วยเบิกต้องเท่ากับยอดคงเหลือที่แสดง

Story Points: 3
Priority    : Medium
```

```
User Story ID : US-004
Title         : แจ้งเตือนสินค้าใกล้หมด

As a     : หัวหน้าคลัง
I want to: ได้รับการแจ้งเตือนเมื่อสินค้าถึงจุดสั่งซื้อ
So that  : สั่งซื้อเพิ่มได้ทันก่อนสินค้าขาดสต็อก

Acceptance Criteria:
  - [ ] ยอดคงเหลือใหม่น้อยกว่าหรือเท่ากับจุดสั่งซื้อ เกิดรายการแจ้งเตือนสถานะ Unread
  - [ ] สินค้าที่ยังมีแจ้งเตือน Unread ค้างอยู่ ไม่สร้างรายการซ้ำ แต่อัปเดตยอดล่าสุด
  - [ ] สินค้าที่ไม่ได้กำหนดจุดสั่งซื้อ ใช้ค่าเริ่มต้น 10 หน่วย
  - [ ] หัวหน้าคลังกดรับทราบแล้วสถานะเปลี่ยนเป็น Read และไม่แสดงในรายการค้าง

Story Points: 3
Priority    : Medium
```

---

## 4. Flow Diagram — UC-002 เบิกสินค้าออกจากคลัง

```
[START]
   |
   v
[เจ้าหน้าที่คลังระบุสินค้า จำนวน ผู้ขอเบิก]
   |
   v
<จำนวนเป็นจำนวนเต็มบวก > 0 ?>
   |                        |
  NO                       YES
   |                        |
   v                        v
[error: จำนวนไม่ถูกต้อง]  <สินค้า Active ?>
   |                        |          |
   |                       NO         YES
   |                        |          |
   |                        v          v
   |              [error: ไม่พบสินค้า] <ยอดคงเหลือเพียงพอ ?>
   |                        |             |            |
   |                        |            NO           YES
   |                        |             |            |
   |                        |             v            v
   |                        |   [error: ยอดไม่พอ]  [BEGIN TRANSACTION]
   |                        |             |                 |
   |                        |             |                 v
   |                        |             |        [ตัดยอดคงเหลือ]
   |                        |             |                 |
   |                        |             |                 v
   |                        |             |        [บันทึกประวัติ Issue]
   |                        |             |                 |
   |                        |             |                 v
   |                        |             |        <บันทึกสำเร็จ ?>
   |                        |             |             |        |
   |                        |             |            NO       YES
   |                        |             |             |        |
   |                        |             |             v        v
   |                        |             |      [ROLLBACK]  [COMMIT]
   |                        |             |             |        |
   |                        |             |             v        v
   |                        |             |   [error: ทำรายการใหม่]  <ยอดใหม่ <= จุดสั่งซื้อ ?>
   |                        |             |             |              |           |
   |                        |             |             |             YES         NO
   |                        |             |             |              |           |
   |                        |             |             |              v           |
   |                        |             |             |   [สร้างแจ้งเตือน UC-004] |
   |                        |             |             |              |           |
   |                        |             |             |              +-----+-----+
   |                        |             |             |                    |
   |                        |             |             |                    v
   |                        |             |             |       [แสดงผลสำเร็จ + ยอดใหม่]
   |                        |             |             |                    |
   +------------------------+-------------+-------------+--------------------+
                                          |
                                          v
                                       [END]
```

---

## 5. API Contract

| Method | Endpoint | FR | Request Body | Success | Error |
|---|---|---|---|---|---|
| POST | `/api/stock/receive` | FR-001 | `{ productId, quantity, referenceNo, note? }` | 201 + `StockMovementDto` | 400 จำนวนผิด, 404 ไม่พบสินค้า |
| POST | `/api/stock/issue` | FR-002 | `{ productId, quantity, requesterName, note? }` | 201 + `StockMovementDto` | 400 จำนวนผิด, 404 ไม่พบสินค้า, 422 ยอดไม่พอ, 409 race condition |
| GET | `/api/stock/{productId}/balance` | FR-003 | - | 200 + `StockBalanceDto` | 404 ไม่พบสินค้า |
| GET | `/api/stock/{productId}/movements?from=&to=&page=&pageSize=` | FR-003 | - | 200 + `PagedResult<StockMovementDto>` | 400 ช่วงวันที่ผิด |
| GET | `/api/stock/alerts?status=unread` | FR-004 | - | 200 + `List<StockAlertDto>` | 403 ไม่ใช่หัวหน้าคลัง |
| PATCH | `/api/stock/alerts/{id}/acknowledge` | FR-004 | - | 204 | 403, 404 |

**รูปแบบ Error Response มาตรฐาน**

```json
{ "code": "INSUFFICIENT_STOCK", "message": "ยอดคงเหลือไม่เพียงพอ คงเหลือ 3 หน่วย", "details": { "productId": 12, "available": 3 } }
```

---

## 6. Data Model (ส่วนที่เพิ่ม)

```
StockMovement
-------------
Id              int        PK
ProductId       int        FK -> Product.Id, Index
MovementType    enum       Receive | Issue
Quantity        int        > 0
BalanceAfter    int        ยอดคงเหลือหลังทำรายการ (สำหรับตรวจย้อนหลัง)
ReferenceNo     string?    ใช้กับ Receive
RequesterName   string?    ใช้กับ Issue
Note            string?
CreatedByUserId int        FK -> User.Id
CreatedAt       datetime   กำหนดโดยระบบ (BL-003)
CancelledAt     datetime?  รองรับ BL-004 ในเฟสถัดไป

StockAlert
----------
Id              int        PK
ProductId       int        FK -> Product.Id
BalanceAtAlert  int
ReorderPoint    int
Status          enum       Unread | Read
CreatedAt       datetime
AcknowledgedAt  datetime?
AcknowledgedBy  int?       FK -> User.Id

Product (แก้ไขเพิ่ม)
-------------------
StockOnHand     int        default 0, ห้ามติดลบ (BL-001)
ReorderPoint    int        default 10 (BL-005)
IsActive        bool       default true (BL-006)
```

**Index ที่ต้องมี:** `StockMovement(ProductId, CreatedAt DESC)` สำหรับ FR-003, `StockAlert(ProductId, Status)` สำหรับกันแจ้งเตือนซ้ำ

---

## 7. Test Case

| TC ID | Related UC | Test Scenario | Input | Expected Result | Type |
|---|---|---|---|---|---|
| TC-001 | UC-001 | รับสินค้าเข้าปกติ | productId=1, quantity=10 | 201, StockOnHand เพิ่ม 10, มี movement Receive 1 รายการ | Positive |
| TC-002 | UC-001 | รับสินค้าจำนวน 1 หน่วย | productId=1, quantity=1 | 201, StockOnHand เพิ่ม 1 | Positive |
| TC-003 | UC-001 | รับสินค้าจำนวนเป็น 0 | quantity=0 | 400, code INVALID_QUANTITY, StockOnHand ไม่เปลี่ยน | Negative |
| TC-004 | UC-001 | รับสินค้าที่ปิดใช้งาน | productId ของสินค้า IsActive=false | 404, code PRODUCT_NOT_FOUND | Negative |
| TC-005 | UC-002 | เบิกสินค้าปกติ | StockOnHand=10, quantity=4 | 201, StockOnHand=6, มี movement Issue | Positive |
| TC-006 | UC-002 | เบิกเท่ายอดคงเหลือทั้งหมด | StockOnHand=5, quantity=5 | 201, StockOnHand=0, ไม่ติดลบ | Positive |
| TC-007 | UC-002 | เบิกเกินยอดคงเหลือ | StockOnHand=3, quantity=5 | 422, code INSUFFICIENT_STOCK, available=3, StockOnHand คงเดิม | Negative |
| TC-008 | UC-002 | เบิกจำนวนติดลบ | quantity=-2 | 400, code INVALID_QUANTITY | Negative |
| TC-009 | UC-002 | เบิกพร้อมกัน 2 รายการรวมเกินยอด | StockOnHand=5, เบิก 3 และ 3 พร้อมกัน | สำเร็จ 1 รายการ อีกรายการได้ 409 หรือ 422, StockOnHand=2 | Negative |
| TC-010 | UC-003 | ดูยอดคงเหลือและประวัติ | productId=1 ไม่ระบุวันที่ | 200, แสดงประวัติ 30 วันย้อนหลัง เรียงใหม่ไปเก่า | Positive |
| TC-011 | UC-003 | ผลรวมประวัติตรงกับยอดคงเหลือ | สินค้าที่มี Receive 10 และ Issue 4 | 200, StockOnHand=6 เท่ากับผลรวมประวัติ | Positive |
| TC-012 | UC-003 | ช่วงวันที่กลับด้าน | from=2026-09-10, to=2026-09-01 | 400, code INVALID_DATE_RANGE | Negative |
| TC-013 | UC-004 | เบิกจนถึงจุดสั่งซื้อ | ReorderPoint=10, StockOnHand=12, เบิก 2 | เกิด StockAlert สถานะ Unread 1 รายการ | Positive |
| TC-014 | UC-004 | เบิกซ้ำขณะมีแจ้งเตือนค้าง | มี Alert Unread อยู่แล้ว เบิกอีก 1 | ไม่เกิดรายการใหม่ BalanceAtAlert อัปเดตเป็นยอดล่าสุด | Positive |
| TC-015 | UC-004 | เจ้าหน้าที่คลังเรียกดู alerts | ผู้ใช้สิทธิ์ Staff | 403, code FORBIDDEN | Negative |

---

## 8. Traceability Matrix

Epic: **SCRUM-6** `[BRD-001] ระบบบันทึกความเคลื่อนไหวสินค้าคงคลัง`

| BRD | FR | BL ที่ผูก | UC | US | TC | Jira Story | Subtask |
|---|---|---|---|---|---|---|---|
| BRD-001 | FR-001 | BL-002, BL-003, BL-006 | UC-001 | US-001 | TC-001..004 | SCRUM-7 | ยังไม่ซอย |
| BRD-001 | FR-002 | BL-001, BL-002, BL-003, NFR-004 | UC-002 | US-002 | TC-005..009 | SCRUM-8 | SCRUM-11 (DB), SCRUM-12 (BE), SCRUM-13 (BE), SCRUM-14 (QA) |
| BRD-001 | FR-003 | BL-003 | UC-003 | US-003 | TC-010..012 | SCRUM-9 | ยังไม่ซอย |
| BRD-001 | FR-004 | BL-005 | UC-004 | US-004 | TC-013..015 | SCRUM-10 | ยังไม่ซอย |

**NFR ที่ยังไม่มีการ์ดเฉพาะ:** NFR-001 (performance test) และ NFR-002 (audit log retention) ถูกบันทึกเป็นข้อสังเกตให้ยกเป็นการ์ดเฟสถัดไป

## 9. สมมติฐานของผู้วิเคราะห์

- ใช้ EF Core Transaction + optimistic concurrency ด้วย RowVersion เพื่อแก้ BL-001 ร่วมกับ NFR-004
- JWT และตาราง User ใช้ของเดิมใน stock_api ไม่สร้างใหม่
- ยังไม่ทำหน้าจอ UI ในเฟสนี้ ส่งมอบเป็น API และทดสอบผ่าน Swagger
