using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Mvc;

namespace EduMatch.PresentationLayer.Configurations
{
	public static class ApiBehaviorConfiguration
	{
		public static IServiceCollection AddApiBehavior(this IServiceCollection services)
		{
			services.Configure<ApiBehaviorOptions>(options =>
			{
				options.InvalidModelStateResponseFactory = context =>
				{
					// Gom lỗi theo từng field
					var errors = context.ModelState
						.Where(x => x.Value?.Errors.Count > 0)
						.ToDictionary(
							kvp => kvp.Key,
							kvp => kvp.Value!.Errors
								.Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
									? "Invalid"
									: e.ErrorMessage)
								.ToArray()
						);

					// Build ApiResponse chuẩn
					var response = ApiResponse<object>.Fail(
						message: "Validation failed",
						error: errors
					);

					return new BadRequestObjectResult(response);
				};
			});

			return services;
		}
	}
}

