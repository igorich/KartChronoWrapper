using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();
// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

Log.Debug($"----==== Started {DateTime.Now} =====------");
Log.Debug($"TRACK_ID: {Environment.GetEnvironmentVariable("TRACK_ID")}\n"
    + $"TRACK_URL: {Environment.GetEnvironmentVariable("TRACK_URL")}\n"
    + $"BUCKET_NAME: {Environment.GetEnvironmentVariable("BUCKET_NAME")}\n"
    + $"TENANT_ID: {MaskSecretString(Environment.GetEnvironmentVariable("TENANT_ID"))}\n"
    + $"ACCESS_KEY: {MaskSecretString(Environment.GetEnvironmentVariable("ACCESS_KEY"))}\n"
    + $"SECRET_KEY: {MaskSecretString(Environment.GetEnvironmentVariable("SECRET_KEY"))}\n");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

static string MaskSecretString(string? input)
{
    if (string.IsNullOrEmpty(input))
        return "<empty>";

    int length = input.Length;

    if (length == 2)
        return "**";
    if (length < 8)
        return input[0] + new string('*', length - 2) + input[length - 1];

    string start = input.Substring(0, 4);
    string end = input.Substring(length - 4, 4);
    string middle = new string('*', length - 8);

    return start + middle + end;
}

