using Microsoft.EntityFrameworkCore;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);
// הזרקת ה-DbContext לשירותים
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"), 
    Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.40-mysql")));

// הוספת שירות CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        });
});

// הוספת Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// הפעלת Swagger
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty; // זה יגרום ל-Swagger UI להיות זמין ב-root
    });
// }

// הפעלת מדיניות CORS
app.UseCors("AllowAllOrigins");

app.MapGet("/", ()=>"ToDoApi is runing!");

// Route לשליפת כל המשימות
app.MapGet("/items", async (ToDoDbContext db) =>
    await db.Items.ToListAsync());

// Route להוספת משימה חדשה
app.MapPost("/item", async (string todo, ToDoDbContext db) =>
{
    var newItem = new Item
    {
        Name = todo,
        IsComplete = false
    };
    db.Items.Add(newItem);
    await db.SaveChangesAsync();
    return Results.Created($"/items/{newItem.Id}", newItem);
});

// Route לעדכון משימה
app.MapPut("/item/{id}", async (int id, bool isComplete, ToDoDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound();
    item.IsComplete = isComplete;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Route למחיקת משימה
app.MapDelete("/item/{id}", async (int id, ToDoDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound();
    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();