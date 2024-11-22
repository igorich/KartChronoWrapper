var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

Console.WriteLine($"----==== Started {DateTime.Now} =====------");
Console.WriteLine($"TRACK_ID: {Environment.GetEnvironmentVariable("TRACK_ID")}"
    + $"TRACK_URL: {Environment.GetEnvironmentVariable("TRACK_URL")}"
    + $"BUCKET_NAME: {Environment.GetEnvironmentVariable("BUCKET_NAME")}"
    + $"TENANT_ID: {MaskSecretString(Environment.GetEnvironmentVariable("TENANT_ID"))}"
    + $"ACCESS_KEY: {MaskSecretString(Environment.GetEnvironmentVariable("ACCESS_KEY"))}"
    + $"SECRET_KEY: {MaskSecretString(Environment.GetEnvironmentVariable("SECRET_KEY"))}");

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

