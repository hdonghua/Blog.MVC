using BeaverX.Core;
using Blog.MVC;

var builder = WebApplication.CreateBuilder(args);

builder.AddBeaverX<BlogMVCModule>();

var app = builder.Build();

app.InitializeBeaverX();

app.Run();
