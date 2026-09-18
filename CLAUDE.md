# CLAUDE.md

กติกาหลักของ repository นี้ Claude Code จะอ่านไฟล์นี้อัตโนมัติทุกครั้งที่เปิดงานในโฟลเดอร์นี้

## โครงสร้าง repo

| โฟลเดอร์ | คืออะไร |
|---|---|
| `test_web_api/` | Web API หลัก ใช้ pattern Controller → Service → Repository (มี CLAUDE.md ของตัวเอง) |
| `stock_api/` | API ระบบ stock แบบ minimal API (มี CLAUDE.md ของตัวเอง) |
| `stock_api.Tests/` | ชุดทดสอบของ stock_api (xUnit) |
| `docs/brd/`, `docs/sa/` | เอกสาร BRD และเอกสาร SA |

## ภาษาที่ใช้

- ตอบผู้ใช้และเขียนเอกสารเป็น**ภาษาไทย** ศัพท์เทคนิค (endpoint, DTO, migration) คงเป็นภาษาอังกฤษ
- comment ในโค้ดและชื่อตัวแปรเป็นภาษาอังกฤษ

## คำสั่งที่ใช้บ่อย

```bash
dotnet build test_web_api.sln    # build ทั้ง solution
dotnet test stock_api.Tests      # รันเทสของ stock_api
```

## กติกาการแก้โค้ด

- **ห้ามแก้ไฟล์ใน `bin/`, `obj/`, `.vs/`** เป็น build output ที่ถูก generate ใหม่ทุกครั้ง
- แก้ไฟล์ `.cs` เสร็จ ต้อง `dotnet build` ให้ผ่านก่อนจึงบอกผู้ใช้ว่าเสร็จ ถ้า build ไม่ผ่านให้บอกตรง ๆ พร้อม error
- เอกสาร SA ที่สร้างใหม่ให้วางที่ `docs/sa/SA-<เลข 3 หลัก>-<slug>.md` เท่านั้น
- ห้าม `git commit` หรือ `git push` จนกว่าผู้ใช้จะสั่ง
