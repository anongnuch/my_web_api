# CLAUDE.md — test_web_api

กติกาเฉพาะโปรเจค `test_web_api` ใช้ต่อยอดจาก CLAUDE.md ที่ root

## สถาปัตยกรรม: 3 ชั้น ห้ามข้ามชั้น

```
Controller  →  Service  →  Repository  →  MyDBContext (EF Core)
```

- **Controller ห้ามเรียก `MyDBContext` โดยตรง** ให้เรียกผ่าน `IProductService` / `IUserService` เสมอ
  (ข้อยกเว้นที่มีอยู่แล้ว: `ProductService` inject `MyDBContext` มาเพื่อเปิด transaction ใน `UpdateAsync` — ถ้าต้องใช้ transaction ให้ทำที่ชั้น Service เท่านั้น)
- เพิ่ม Service หรือ Repository ใหม่ ต้องสร้าง interface คู่กันเสมอ (`IProductService` + `ProductService`) แล้วลงทะเบียน DI ใน `Program.cs`

## Controller

- ใช้ `[Route("api/[controller]")]` + `[ApiController]` และ method เป็น `async Task<IActionResult>` ตามแบบ `ProductsController.cs`
- request body ต้องรับเป็น DTO ใน `Dtos/` (เช่น `ProductCreateDto`) **ห้ามรับ entity ตรง ๆ**
- ค่าที่ return: หาไม่เจอ → `NotFound()`, ลบสำเร็จ → `NoContent()`, ที่เหลือ → `Ok(...)`

## Validation

- business rule (เช่น ชื่อสินค้าซ้ำ) ตรวจที่ชั้น Service แล้ว `throw` ให้ Controller แปลงเป็น `BadRequest`
- ห้ามตรวจ business rule ใน Controller
