var builder = WebApplication.CreateBuilder(args);

builder.AddSerilog();
builder.Services.AddHostServices(builder.Configuration);

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}
else
{
    app.UseExceptionHandler(opt => { });
    app.UseStatusCodePages();
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseCors("Frontend");
app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();
