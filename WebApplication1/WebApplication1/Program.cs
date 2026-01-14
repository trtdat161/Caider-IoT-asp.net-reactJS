using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.EntityFrameworkCore;
using WebApplication1.data;
using WebApplication1.Models;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(); // Để dùng Controllers (nếu có)
builder.Services.AddOpenApi(); // Để có Swagger UI test API

/*------------- đăng ký DataContext ------------*/
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

/* đăng ký mqtt services, MQTT client là luồng kết nối dài (persistent connection), 
không tạo đi tạo lại cho mỗi request là lý do dùng AddSingleton */ 
builder.Services.AddSingleton<MqttService>();
/* ------------ đăng ký cors để gọi api --------- */
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("reactApp", policy => // dùng policy tạo tên là reactApp
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174") // chỉ cho pháp 2 url này call api
              .AllowAnyHeader() // Cho phép gửi bất kỳ header nào (Authorization, Content-Type...)
              .AllowAnyMethod(); // // Cho phép GET, POST, PUT, DELETE...
    });
});

var app = builder.Build();


var micro = "api/microcontrollers";
var expansive = "api/expansiveboards";
var servo = "api/servos";
var motor = "api/motors";

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Hiển thị Swagger
}
//app.UseHttpsRedirection(); // Đã comment để CORS hoạt động trong dev
app.UseCors("reactApp");
app.UseAuthorization();
app.MapControllers();

/* ============= API GỬI LỆNH TỚI ESP32 ============= */

// ấn nút mới connect IoT
app.MapPost("api/mqtt/connect", async (MqttService mqtt) =>
{
    try
    {
    // truyền vào 2 tham số là địa chỉ broker.hivemq.com và port mặc đinh mqtt là 1883
        await mqtt.ConnectAsync(
           broker: "broker.hivemq.com",// sài free của HiveMQ địa chỉ broker.hivemq.com
           port: 1883// port mặc định mqtt
       );
       /*
       backend kết nối tới broker HiveMQ
       sau đó backend đã online trên broker
       nó có thể Publish / Subscribe
       */
        return Results.Ok(new
        {
            success = true,
            message = "Connect MQTT OK"
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"lỗi connect mqtt: {ex.Message}");
    }
});

app.MapPost("api/mqtt/command", async (CommandRequest cmd, MqttService mqtt) =>
{
    try
    {
        // Gửi tên hàm tới ESP32
        await mqtt.PublishAsync("esp32/command", cmd.FunctionName);

        return Results.Ok(new
        {
            success = true,
            message = "Đã gửi lệnh"
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Lỗi: {ex.Message}");
    }
});

// API dừng khẩn cấp
app.MapPost("api/mqtt/stop", async (MqttService mqtt) =>
{
    await mqtt.PublishAsync("esp32/command", "STOP");
    return Results.Ok(new { message = "Đã gửi lệnh dừng!" });
});

// GET - danh sách + phân trang
app.MapGet(micro, async (DataContext db, int pageNumber = 1) =>
{
    try
    {
        if (pageNumber < 1) pageNumber = 1;
        int pageSize = 6;// set cứng luôn là 6 hàng 

        var query = db.MicroControls.Where(x => x.DeletedAt == null);

        // đếm xem con bao nhiu trag
        var cntTotal = await query.CountAsync();

        // lay 6 ban ghi
        var items = await query.OrderBy(x => x.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        
        // tra ve ket qua
        var result = new { 
            items = items, // 6 hang
            cntTotal = cntTotal,
            pageNumber = pageNumber,
            pageSize = pageSize
        };
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem("Lỗi khi tải danh sách: " + ex.Message);
    }
});
app.MapGet(expansive, async (DataContext db, int pageNumber = 1) =>
{
    try
    {
        if (pageNumber < 1) pageNumber = 1;
        int pageSize = 6;

        var query = db.ExpansiveBoards.Where(x => x.DeletedAt == null);
        var cntTotal = await query.CountAsync();
        var items = await query.OrderBy(x => x.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        // gom lại thành object ẩn danh sau FE chỉ cần chấm để truy cập
        // tức lúc này trả về là object nên cách gọi api lưu ý hơn là chỉ gọi danh sách mảng như ban đầu
        var result = new
        {
            items = items,
            cntTotal = cntTotal,
            pageNumber = pageNumber,
            pageSize = pageSize,
        };
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem("Lỗi khi tải danh sách: " + ex.Message);
    }
});
app.MapGet(servo, async (DataContext db, int pageNumber = 1) =>
{
    try
    {
        if (pageNumber < 1) pageNumber = 1;
        int pageSize = 6;

        var query = db.Servos.Where(x => x.DeletedAt == null);
        var cntTotal = await query.CountAsync();
        var items = await query.OrderBy(x => x.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        var reslut = new
        {
            items = items,
            cntTotal = cntTotal,
            pageNumber = pageNumber,
            pageSize = pageSize,
        };
        return Results.Ok(reslut);
    }
    catch (Exception ex)
    {
        return Results.Problem("lỗi khi tải danh sách: " + ex.Message);
    }
});
app.MapGet(motor, async (DataContext db, int pageNumber = 1) =>
{
    try
    {
        if (pageNumber < 1) pageNumber = 1;
        int pageSize = 6;

        var query = db.Motors.Where(x => x.DeletedAt == null);
        var cntTotal = await query.CountAsync();
        var items = await query.OrderBy(x => x.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        var result = new
        {
            items = items,
            cntTotal = cntTotal,
            pageNumber = pageNumber,
            pageSize = pageSize,
        };
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem("lõi khi tải danh sách: " + ex.Message);
    }
});

// GET by ID (dùng để xem chi tiết - ap dụng cho form sửa, add chung khi có id sẽ là edit ko có là add)
app.MapGet($"{micro}/{{id}}", async (int id, DataContext db) =>
{
    try
    {
        var mc = await db.MicroControls.FindAsync(id);
        return mc is not null ? Results.Ok(mc) : Results.NotFound("không tìm thấy micro");
    }
    catch (Exception ex)
    {
        return Results.Problem("error: " + ex.Message);
    }
});
app.MapGet($"{expansive}/{{id}}", async (int id, DataContext db) =>
{
    try
    {
        var es = await db.ExpansiveBoards.FindAsync(id);
        return es is not null ? Results.Ok(es) : Results.NotFound("Không tìm thấy expansive");
    }
    catch (Exception ex)
    {
        return Results.Problem("error: " + ex.Message);
    }
});
app.MapGet($"{servo}/{{id}}", async (int id, DataContext db) =>
{
    try
    {
        var sv = await db.Servos.FindAsync(id);
        return sv is not null ? Results.Ok(sv) : Results.NotFound("Không tìm thấy servo");
    }
    catch (Exception ex)
    {
        return Results.Problem("error: " + ex.Message);
    }
});
app.MapGet($"{motor}/{{id}}", async (int id, DataContext db) =>
{
    try
    {
        var mt = await db.Motors.FindAsync(id);
        return mt is not null ? Results.Ok(mt) : Results.NotFound("Không tìm thấy motor");
    }
    catch (Exception ex)
    {
        return Results.Problem("error: " + ex.Message);
    }
});

// POST - thêm mới
app.MapPost(micro, async (MicroControl mc, DataContext db) =>
{
    try
    {
        if (string.IsNullOrEmpty(mc.Name))
        {
            return Results.BadRequest("vui lòng nhập loại vi điều khiển !");
        }
        mc.CreatedAt = DateTime.Now;
        mc.UpdatedAt = DateTime.Now;
        // thêm vào db thật
        db.MicroControls.Add(mc);
        await db.SaveChangesAsync();
        // Trả về 201 Created + object mới
        return Results.Created($"{micro}/{mc.Id}", mc);
    }
    catch (DbUpdateException dbEx)
    {
        return Results.Problem("error database" + dbEx.InnerException?.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem("error: " + ex.Message);
    }
});
// bo mở rộng có quan hệ
app.MapPost(expansive, async (ExpansiveBoard eb, DataContext db) =>
{
    try
    {
        if (string.IsNullOrEmpty(eb.Name))
        {
            return Results.BadRequest("vui lòng nhập loại board mở rộng !");
        }
        if (eb.MicroControlId <= 0)// chưa dc chọn
        {
            return Results.BadRequest("vui lòng chọn microcontrol !");
        }
        // Microcontrols trùng tên trong datacontext (hàm AnyAsync kiểm tra xem có record nào thỏa đk ko)
        var microExists = await db.MicroControls
        .AnyAsync(micro => micro.Id == eb.MicroControlId && micro.DeletedAt == null);
        if (!microExists)
        {
            return Results.BadRequest("MicroController không tồn tại!");
        }
        eb.CreatedAt = DateTime.Now;
        eb.UpdatedAt = DateTime.Now;
        // thêm vào db thật
        db.ExpansiveBoards.Add(eb);
        await db.SaveChangesAsync();
        // Trả về 201 Created + object mới
        return Results.Created($"{expansive}/{eb.Id}", eb);
    }
    catch (DbUpdateException dbEx)
    {
        return Results.Problem("error database: " + dbEx.InnerException?.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem("error: " + ex.Message);
    }
});
app.MapPost(servo, async (Servo sv, DataContext db) =>
{
    try
    {
        if (string.IsNullOrEmpty(sv.Name))
        {
            return Results.BadRequest("vui lòng nhập loại động cơ servo !");
        }
        sv.CreatedAt = DateTime.Now;
        sv.UpdatedAt = DateTime.Now;
        // thêm vào db thật
        db.Servos.Add(sv);
        await db.SaveChangesAsync();
        // Trả về 201 Created + object mới
        return Results.Created($"{servo}/{sv.Id}", sv);
    }
    catch (DbUpdateException dbEx)
    {
        return Results.Problem("error database: " + dbEx.InnerException?.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem("error: " + ex.Message);
    }
});
app.MapPost(motor, async (Motor mt, DataContext db) =>
{
    try
    {
        if (string.IsNullOrEmpty(mt.Name))
        {
            return Results.BadRequest("vui lòng nhập loại động cơ !");
        }
        mt.CreatedAt = DateTime.Now;
        mt.UpdatedAt = DateTime.Now;
        // thêm vào db thật
        db.Motors.Add(mt);
        await db.SaveChangesAsync();
        // Trả về 201 Created + object mới
        return Results.Created($"{motor}/{mt.Id}", mt);
    }
    catch (DbUpdateException dbEx)
    {
        return Results.Problem("error database: " + dbEx.InnerException?.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem("error: " + ex.Message);
    }
});

// PUT - sửa
app.MapPut($"{micro}/{{id}}", async (int id, MicroControl input, DataContext db) =>
{
    try
    {
        var mc = await db.MicroControls.FindAsync(id);
        if (mc is null) return Results.NotFound("không thể sửa !");
        if (string.IsNullOrEmpty(input.Name))
        {
            return Results.BadRequest("hãy nhập vào vi điều khiển muốn sửa !");
        }
        mc.Name = input.Name;
        mc.Note = input.Note;
        mc.UpdatedAt = DateTime.Now;
        // lưu thay đổi thật
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    catch (DbUpdateException dbEx)
    {
        return Results.Problem("error database: " + dbEx.InnerException?.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem("error: " + ex.Message);
    }
});
app.MapPut($"{expansive}/{{id}}", async (int id, ExpansiveBoard input, DataContext db) =>
{
    try
    {
        var es = await db.ExpansiveBoards.FindAsync(id);
        if (es is null) return Results.NotFound("không thể sửa !");
        if (string.IsNullOrEmpty(input.Name))
        {
            return Results.BadRequest("hãy nhập loại board mở rộng muốn sửa !");
        }
        es.Name = input.Name;
        es.Note = input.Note;
        es.MicroControlId = input.MicroControlId;// FK
        es.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    catch (DbUpdateException dbEx)
    {
        return Results.Problem("error database: " + dbEx.InnerException?.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem("error: " + ex.Message);
    }
});
app.MapPut($"{servo}/{{id}}", async (int id, Servo input, DataContext db) =>
{
    try
    {
        var sv = await db.Servos.FindAsync(id);
        if (sv is null) return Results.NotFound("không thể sửa !");
        if (string.IsNullOrEmpty(input.Name))
        {
            return Results.BadRequest("hãy nhập vào servo muốn sửa !");
        }
        sv.Name = input.Name;
        sv.Note = input.Note;
        sv.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    catch (DbUpdateException dbEx)
    {
        return Results.Problem("error databsae: " + dbEx.InnerException?.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem("error: " + ex.Message);
    }
});
app.MapPut($"{motor}/{{id}}", async (int id, Motor input, DataContext db) =>
{
    try
    {
        var mt = await db.Motors.FindAsync(id);
        if (mt is null) return Results.NotFound("không thể sửa !");
        if (string.IsNullOrEmpty(input.Name))
        {
            return Results.BadRequest("hãy nhập vào motor muốn sửa !");
        }
        mt.Name = input.Name;
        mt.Note = input.Note;
        mt.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    catch (DbUpdateException dbEx)
    {
        return Results.Problem("error database: " + dbEx.InnerException?.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem("error: " + ex.Message);
    }
});

// DELETE - soft delete
app.MapDelete($"{micro}/{{id}}", async (int id, DataContext db) =>
{
    try
    {
        var mc = await db.MicroControls.FindAsync(id);
        if (mc is null) return Results.NotFound("không thể xóa!");
        mc.DeletedAt = DateTime.Now;
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    catch (DbUpdateException dbEx)
    {
        return Results.Problem("error database: " + dbEx.InnerException?.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem("error : " + ex.Message);
    }
});
app.MapDelete($"{expansive}/{{id}}", async (int id, DataContext db) =>
{
    try
    {
        var es = await db.ExpansiveBoards.FindAsync(id);
        if (es is null) return Results.NotFound("không thể xóa");
        es.DeletedAt = DateTime.Now;
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    catch (DbUpdateException dbEx)
    {
        return Results.Problem("error database: " + dbEx.InnerException?.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem("error: " + ex.Message);
    }
});
app.MapDelete($"{servo}/{{id}}", async (int id, DataContext db) =>
{
    try
    {
        var sv = await db.Servos.FindAsync(id);
        if (sv is null) return Results.NotFound("không thể xóa");
        sv.DeletedAt = DateTime.Now;
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    catch (DbUpdateException dbEx)
    {
        return Results.Problem("error database: " + dbEx.InnerException?.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem("error: " + ex.Message);
    }
});
app.MapDelete($"{motor}/{{id}}", async (int id, DataContext db) =>
{
    try
    {
        var mt = await db.Motors.FindAsync(id);
        if (mt is null) return Results.NotFound("không thể xóa");
        mt.DeletedAt = DateTime.Now;
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    catch (DbUpdateException dbEx)
    {
        return Results.Problem("error database: " + dbEx.InnerException?.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem("error: " + ex.Message);
    }
});

// DELETE - permanent delete
app.MapDelete($"{micro}/{{id}}/permanent", async (int id, DataContext db) =>
{
    try
    {
        var mc = await db.MicroControls.FindAsync(id);
        if (mc is null) return Results.NotFound("không thể xóa");
        db.MicroControls.Remove(mc);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    catch (DbUpdateException dbEx)
    {
        return Results.Problem("error database: " + dbEx.InnerException?.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem("error: " + ex.Message);
    }
});
app.MapDelete($"{expansive}/{{id}}/permanent", async (int id, DataContext db) =>
{
    try
    {
        var es = await db.ExpansiveBoards.FindAsync(id);
        if (es is null) return Results.NotFound("không thể xóa");
        db.ExpansiveBoards.Remove(es);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    catch (DbUpdateException dbEx)
    {
        return Results.Problem("error database: " + dbEx.InnerException?.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem("error: " + ex.Message);
    }
});
app.MapDelete($"{servo}/{{id}}/permanent", async (int id, DataContext db) =>
{
    try
    {
        var sv = await db.Servos.FindAsync(id);
        if (sv is null) return Results.NotFound("không thể xóa");
        db.Servos.Remove(sv);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    catch (DbUpdateException dbEx)
    {
        return Results.Problem("error database: " + dbEx.InnerException?.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem("error: " + ex.Message);
    }
});
app.MapDelete($"{motor}/{{id}}/permanent", async (int id, DataContext db) =>
{
    try
    {
        var mt = await db.Motors.FindAsync(id);
        if (mt is null) return Results.NotFound("không thể xóa");
        db.Motors.Remove(mt);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    catch (DbUpdateException dbEx)
    {
        return Results.Problem("error database: " + dbEx.InnerException?.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem("error: " + ex.Message);
    }
});

app.Run();

// DTO
public record CommandRequest(string FunctionName);