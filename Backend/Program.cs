using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Backend.API.Data.Components;
using Backend.API.Data.Context;
using Backend.API.Data.Models;
using Backend.API.Services;
using Scalar.AspNetCore;
using System.Text;
using Backend.API.Extensions;

namespace Backend;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        #region --Services--

        builder.ServicesRegistry();

        var app = builder.Build();

        #endregion +----------+

        #region --Middleware--

        app.MiddlewareRegistry();

        #endregion +----------+

        #region --EndPoints--

        app.EndPointsRegistry();

        #endregion +----------+

        app.Run();
    }
}
