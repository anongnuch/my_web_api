# CLAUDE.md

## Project overview

`flood_api` คือ ASP.NET Core 8 minimal API + หน้าแผนที่ Leaflet สำหรับดูสถานการณ์น้ำ (ใช้ส่วนตัว) ทุกแหล่งข้อมูลเป็น public และ**ไม่ต้องใช้ API key**

| แหล่ง | endpoint ต้นทาง | ใช้ทำอะไร | ขอบเขต |
|---|---|---|---|
| POPNIX Flood | `api_overview.php` | ระดับน้ำคลองของสำนักการระบายน้ำ (ราว 200 จุด) พร้อมเกณฑ์เฝ้าระวัง/วิกฤต และแนวโน้ม | กทม. เท่านั้น |
| POPNIX Flood | `api_roads.php` | sensor วัดน้ำบนถนนและในอุโมงค์ (ราว 250 จุด) หน่วยเป็นซม. | กทม. เท่านั้น |
| ThaiWater (สสน.) | `waterlevel_load` | สถานีระดับน้ำ telemetry (ราว 800 จุด) ใช้ `situation_level` (5 = ล้นตลิ่ง, 4 = น้ำมาก) | ทั้งประเทศ |
| ThaiWater (สสน.) | `rain_24h` | ฝนสะสม 1 ชม. / 24 ชม. (ราว 4,600 จุด) | ทั้งประเทศ |

- record ของ ThaiWater มี field `Province` หน้าเว็บใช้ค่านี้ทำตัวเลือกจังหวัด ส่วนข้อมูลคลองและถนนไม่มี field นี้ เพราะเป็นข้อมูลของ กทม. เท่านั้น
- ยังไม่มีพื้นที่น้ำท่วมจากดาวเทียม (GISTDA Disaster Platform) เพราะต้องใช้ API key ที่ต้องสมัครที่ api-gateway.gistda.or.th

- ห้ามใช้ ThaiWater `canal_waterlevel` เป็นแหล่งข้อมูลคลอง เพราะ feed นั้นค้างอยู่หลายวัน จึงเปลี่ยนมาใช้ POPNIX แทน
- เงื่อนไขของ POPNIX ต้องแสดงเครดิต "สำนักการระบายน้ำ กรุงเทพมหานคร ผ่าน POPNIX Flood" และห้ามนำเสนอเป็นประกาศเตือนภัยทางการ ข้อความนี้อยู่ใน `wwwroot/index.html` แล้ว ห้ามลบออก
- POPNIX จำกัดการเรียกไว้ที่ 30 ครั้งต่อนาทีต่อ IP ตอนนี้เราเรียกแค่ 2 ครั้งทุก 5 นาที

## Commands

```bash
dotnet run --project flood_api    # แล้วเปิด http://localhost:5180
```

## Architecture

- `Services/FloodDataService.cs` เป็น `BackgroundService` ที่ดึงข้อมูลทั้ง 4 แหล่งทุก `ThaiWater:RefreshMinutes` นาที แล้วเก็บ snapshot ไว้ใน memory (ไม่มี DB) ถ้าแหล่งไหนดึงไม่สำเร็จจะใช้ข้อมูลรอบก่อนของแหล่งนั้นต่อ
- `Services/PopnixParser.cs` และ `Services/ThaiWaterParser.cs` แปลง JSON ดิบเป็น record ใน `Models/FloodModels.cs` สถานะทุกชั้นข้อมูลถูกแปลงเป็นค่าชุดเดียวกัน คือ `normal` / `warning` / `critical` / `unknown` (สำหรับถนน: dry / slight / flood / off)
- `IsStale = true` เมื่อจุดวัด offline หรือเวลาวัดเก่ากว่า `ThaiWater:StaleAfterHours` หน้าแผนที่ซ่อนจุดเหล่านี้เป็นค่าเริ่มต้น และไม่นับเข้ารายการแจ้งเตือน
- Endpoints: `/api/flood` (ทั้งหมด ใช้โดยหน้าแผนที่), `/api/roads` (ถนนที่น้ำท่วมตอนนี้ เรียงจากลึกสุด, `?all=true` = ทุก sensor), `/api/canals`, `/api/rivers`, `/api/rain`, `/api/status`
- หน้าแผนที่อยู่ที่ `wwwroot/index.html` (ไฟล์เดียว โหลด Leaflet จาก cdnjs) ใช้ `preferCanvas: true` เพื่อให้วาดหมุดหลายพันจุดได้ลื่น ข้อมูล `/api/flood` มีขนาดหลาย MB จึงเปิด response compression ไว้ใน `Program.cs`
