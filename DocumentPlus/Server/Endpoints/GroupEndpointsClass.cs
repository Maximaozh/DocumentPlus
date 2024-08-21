namespace DocumentPlus.Server.Endpoints
{
    public static class GroupEndpointsClass
    {
        public static IEndpointRouteBuilder MapGroupEndpoints(this IEndpointRouteBuilder app)
        {
            //app.MapPost("api/user/login", Login);

            return app;
        }



        //private static async Task<IResult> Create(GroupService GroupService, [FromBody] GroupInfo group)
        //{
        //    //LoginResponse data = await GroupService.Create(group);
        //    return data is null ? Results.BadRequest() : Results.Json(data);
        //}
    }
}
